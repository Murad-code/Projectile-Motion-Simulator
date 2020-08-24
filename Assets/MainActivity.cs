using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class MainActivity : MonoBehaviour
{
    // UI Elements
    Canvas Canvas;
    GameObject Ball;
    GameObject QuestionMode;
    GameObject TutorialMode;
    GameObject DistanceText;
    GameObject Wall;
    GameObject BallLabels;

    TextMeshProUGUI XDistanceText;
    TextMeshProUGUI YDistanceText;
    TextMeshProUGUI SFText;
    int SFLabel = 1;

    TrailRenderer Line;
    Vector3 CurrentPos;
    Button StartBtn;
    Button PauseBtn;
    Button SlowerBtn;
    Button FasterBtn;
    bool Moving;
   
    Button ResetBtn;
    Button HomeBtn;
    Button UpArrowBtn;
    Button DownArrowBtn;
    Button WallBtn;
    Button LabelsBtn;

    TextMeshProUGUI MsgTextbox;
    TextMeshProUGUI XVelocityText;
    TextMeshProUGUI YVelocityText;

    InputField InputUVelocity;
    InputField InputAngle;

    float SF_UVelocity;
    float SF = 11; // Scale Factor for calculating distance travelled to prevent ball moving off the screen
    int TF = 3; //Time Factor for changing speed of the flight
    int Index;

    //This array is used to hold all the limits displayed in the placeholder for inital velocity
    string[] Range = new string[]
    { "0-10", "10-20", "20-30", "30-40", "40-50"};

    //This array is used to pick the correct scale factor depending on which SF is chosen in the UI
    float[] SFArray = new float[5]
    { 11f, 11/2f, 11/3f, 11/4f, 11/5f };

    // Suvat Variables
    float UVelocity;
    float uxVelocity, uyVelocity;
    float Angle;
    float Timer;
    float totalTime;
    float xTotalDistance, yTotalDistance;
    float xDistance, yDistance;

    //Check for when the ball hits the ground
    Boolean collision = false;

    //Variable which holds the data saved from the main menu scene
    int selectedMode;

    void Start()
    {
        // enabled stops Update() running. Ball would fall down otherwise
        enabled = false;

        //This section is just assigning the UI gameobjects 

        //Started using transform.GetChild instead of GameObject.find because it is more efficient 
        //in terms of the computer processing the data and loading the scene everytime. This is because
        //GameObject.Find will search the entire scene for a gameobject whereas Transform.getchild
        //checks a specific gameobject for its contents. 
        //To search for a gameobject inside a parent gameobject, I use Transform
        //The numbers are just pointers for where the gameobject is placed in the Unity hierarchy
        //https://gamedev.stackexchange.com/questions/157709/is-gameobject-find-a-bad-idea-even-for-one-frame    

        Ball = gameObject;
        BallLabels = transform.GetChild(0).gameObject;
        Canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        DistanceText = Canvas.transform.GetChild(11).gameObject;
        Wall = Canvas.transform.GetChild(13).gameObject;

        StartBtn = Canvas.transform.GetChild(1).GetComponent<Button>();
        PauseBtn = Canvas.transform.GetChild(3).GetComponent<Button>();
        SlowerBtn = Canvas.transform.GetChild(4).GetComponent<Button>();
        FasterBtn = Canvas.transform.GetChild(5).GetComponent<Button>();
        ResetBtn = Canvas.transform.GetChild(2).GetComponent<Button>();
        HomeBtn = Canvas.transform.GetChild(6).GetComponent<Button>();
        UpArrowBtn = Canvas.transform.GetChild(7).GetComponent<Button>();
        DownArrowBtn = Canvas.transform.GetChild(8).GetComponent<Button>();
        WallBtn = Canvas.transform.GetChild(14).GetComponent<Button>();
        LabelsBtn = Canvas.transform.GetChild(15).GetComponent<Button>();

        MsgTextbox = Canvas.transform.GetChild(16).GetComponent<TextMeshProUGUI>();
        XVelocityText = BallLabels.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        YVelocityText = BallLabels.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        InputUVelocity = Canvas.transform.GetChild(9).GetComponent<InputField>();
        InputAngle = Canvas.transform.GetChild(10).GetComponent<InputField>();

        SFText = Canvas.transform.GetChild(12).GetComponent<TextMeshProUGUI>();
        YDistanceText = DistanceText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        XDistanceText = DistanceText.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        Line = Ball.transform.GetComponent<TrailRenderer>();

        QuestionMode = GameObject.Find("QuestionMode");
        TutorialMode = GameObject.Find("TutorialMode");

        //Using the saved data from the user's selection of mode in the previous scene,
        //This code decides which UI elements to disable
        selectedMode = PlayerPrefs.GetInt("selectedMode");
        if (selectedMode == 1)
        {
            //This means playground mode was selected hence the QuestionMode UI are now disabled
            QuestionMode.SetActive(false);
            
            //TutorialMode.SetActive(false);
        }
        if (selectedMode == 2)
        {
            //This means QuestionMode was selected hence labels used in Playground UI are now disabled
            //Also a prefab is made to prevent errors with which code should be active in the QuestionMode
            //script and is stored under the name 'Fullscreen' with a default value of 0 meaning
            //the fullscreen mode is off when the scene is loaded
            PlayerPrefs.SetInt("Fullscreen", 0);
            DistanceText.SetActive(false);
        }

        //Calling the UI function which enables all the UI elements to be used by the user
        UI();
    }

    void UI()
    {
        //The UI adds listeners to all buttons so when user clicks on one, their corresponding 
        //functions are called. Some use 'delegate' which essentially is a function for code which
        //may be small enough that they do not require me to create a whole function and call it
        //which is inefficient

        StartBtn.onClick.AddListener( delegate {
            //Line is the ball's projectile line which must be enabled before ball is launched
            Line.enabled = true;
            Moving = true;
            Validation();
        });

        PauseBtn.onClick.AddListener(delegate
        {
            if (Moving == true)
            {
                //'enabled' controls whether or not the update function is active
                enabled = false;
                Moving = false;
            }
            else
            {
                enabled = true;
                Moving = true;
            }
        });

        FasterBtn.onClick.AddListener(delegate
        { 
            //This is the upper limit of the TF
            if (TF < 9)
            {
                TF++;
            }
        });

        SlowerBtn.onClick.AddListener(delegate 
        {
            //This is the lower limit of the TF
            if (TF > 1)
            {
                TF--;
            }
        });

        ResetBtn.onClick.AddListener(ResetFunc);
        HomeBtn.onClick.AddListener(Home);
        UpArrowBtn.onClick.AddListener(UpArrow);
        DownArrowBtn.onClick.AddListener(DownArrow);
        WallBtn.onClick.AddListener(ActivateWall);

        //This button will display the current velocity labels attached to the ball
        LabelsBtn.onClick.AddListener(delegate
        {
            if (BallLabels.activeSelf == false)
            {
                BallLabels.SetActive(true);
                XVelocityText.text = uxVelocity.ToString();
                YVelocityText.text = uyVelocity.ToString();
            }
            else
            {
                BallLabels.SetActive(false);
            }
        });
    }

    void Validation()
    {
        //Validation checks the user's inputs for the velocity and angle to ensure they are
        //appropriate. If not an error message is displayed. Also, the user can only type numbers into these 
        //input fields so this function is an extra check for the size of the integers they input.
        if (InputAngle.text != "")
        {
            Angle = float.Parse(InputAngle.text);
        }

        bool angle = true;
        if (Angle < 10 || Angle > 90)
        {
            MsgTextbox.text = "Error: Select a value between 10 and 90 degrees";
            angle = false;
        }

        //As you can see below I decided to use a switch statement instead of multiple of if statements
        //This is because it maintains better code readability. I researched this on forums discussing their
        //differences. The following link is a good example:
        //https://stackoverflow.com/questions/4241768/switch-vs-if-statements


        if (InputUVelocity.text != "")
        {
            UVelocity = float.Parse(InputUVelocity.text);
        }

        bool velocity = true;

        //The switch statement uses the Index value to compare with the conditions.
        //This is because Index is altered when SF is changed the limits displayed to the user are changed
        //The following conditions match the limits displayed as it uses the same pointer, which is the Index.
        switch (Index)
        {
            case 0:
                if (UVelocity < 0 || UVelocity > 10)
                {
                    velocity = false;
                }
                break;
            case 1:
                if (UVelocity < 10 || UVelocity > 20)
                {
                    velocity = false;
                }
                break;
            case 2:
                if (UVelocity < 20 || UVelocity > 30)
                {
                    velocity = false;
                }
                break;
            case 3:
                if (UVelocity < 30 || UVelocity > 40)
                {
                    velocity = false;
                }
                break;
            case 4:
                if (UVelocity < 40 || UVelocity > 50)
                {
                    velocity = false;
                }
                break;
        }

        //Only when the velocity and angle inputted is valid, then the program will continue 
        //Otherwise the user will be asked to re-enter different values which are more appropriate
        if (velocity == true && angle == true)
        {
            Conversion();
        }
        else
        {
            MsgTextbox.text = "Error: Invalid input for Angle/Velocity, check if the value is within limits";
        }

    }

    //Home function is called when the Home button is clicked
    void Home()
    {
        //Loads the main menu
        SceneManager.LoadScene("Menu");
    }

    //UpArrow and DownArrow are functions related to when the user clicks on the arrows to change the SF value
    //e.g. when user clicks up arrow, the SF label's value increment as well as the value of the Index variable
    //Index value is then used as pointer for selecting item in the Range array which is used to display
    //the limits the user can input. This is the same for down arrow but the values decrease.

    void UpArrow()
    {
        if (SFLabel < 5)
        {
            ++SFLabel;
            SFText.text = SFLabel.ToString();

            ++Index;

            InputUVelocity.placeholder.GetComponent<Text>().text = Range[Index];
            SF = SFArray[Index];
        }
    }

    void DownArrow()
    {
        if (SFLabel > 1)
        {
            --SFLabel;
            SFText.text = SFLabel.ToString();
                
            --Index;

            InputUVelocity.placeholder.GetComponent<Text>().text = Range[Index];
            SF = SFArray[Index];
        }
    }

    //The following function is called when the user clicks on the wall button
    //It determines if the Wall the ball sits on is already active, if so it is disabled and relocates
    //the ball to the ground, if inactive, the wall is spawned and the ball is moved on top
    //Line.Clear and Line.enabled prevents any lines being drawn as the ball moves (it follows the ball).

    void ActivateWall()
    {
        if (Wall.activeSelf == true && Moving == false)
        {
            ResetFunc();
            Wall.SetActive(false);
            Ball.transform.position = new Vector3(-655f, -228f, 100f);
        }
        else if (Wall.activeSelf == false && Moving == false)
        {
            ResetFunc();
            Wall.SetActive(true);
            Ball.transform.position = new Vector3(-655f, -56f, 100f);
        }
    }

    //The conversion function is called to convert the ball's velocity into horizontal and vertical components
    //Also, the angle is converted into radians as C# calculates trigonmetric functions using radians
    //Hence the angle inputted by the user is converted into radians.
    //After the values are converted the finalvalues and Calculations functions are called.

    void Conversion()
    {
        enabled = true;
        SF_UVelocity = UVelocity * SF;
        Angle = Angle * Mathf.PI / 180;

        uxVelocity = UVelocity * Mathf.Cos(Angle);
        uyVelocity = UVelocity * Mathf.Sin(Angle);
        FinalValues();

        uxVelocity =  SF_UVelocity * Mathf.Cos(Angle);
        uyVelocity = SF_UVelocity * Mathf.Sin(Angle);
        Calculations();
    }

    void Calculations()
    {
        //Timer adds the current time with the change in time between each frame
        Timer = Time.deltaTime;

        //Timer multiplied by TF which is the time factor
        Timer = Timer * TF; 

        // Assigning time value to zero will cause resultant distance to be zero
        if (collision == true)
        {
            Timer = 0f;
            enabled = false;
        }

        xDistance = uxVelocity * Timer;
        // s = ut + .5at²
        yDistance = (uyVelocity * Timer) + (0.5f * -9.8f * (float)Math.Pow(Timer, 2f));

        //Creating a final velocity for Y axis and then setting it to the initial velocity for next iteration
        uyVelocity = (float)Math.Round(uyVelocity + -9.8f * Timer, 2);
        uxVelocity = (float)Math.Round(uxVelocity, 2);
    }

    //FinalValues function calculates the maximum horizontal and vertical distance travelled
    //The equations used v=u+at, d=s*t, and s = ut+.5at². These equations are rearranged to solve for the answer
    //The values are also rounded so the number isn't too long and more readable to the user.
    void FinalValues()
    {
        totalTime = uyVelocity / 9.8f * 2f;
        xTotalDistance = uxVelocity * totalTime;
        yTotalDistance = uyVelocity * (totalTime / 2f) + .5f * -9.8f * (float)Math.Pow(Timer, 2f);

        xTotalDistance = (float)Math.Round(xTotalDistance, 2);
        yTotalDistance = (float)Math.Round(yTotalDistance, 2);

    }

    // Update is called once per frame
    void Update()
    {
        CurrentPos = Ball.transform.position;

        //Used "==" initially but it may skip the exact y value and go below
        //Also, I used booleans so all code related to calcualtions for the movement of the ball
        //are inside the Calculations function so it makes reading the code easier for others      

        if (CurrentPos.y <= -228.4f)
        {
            collision = true;
            Moving = false;
            XDistanceText.text = xTotalDistance.ToString() + "m";
            YDistanceText.text = yTotalDistance.ToString() + "m";

        }

        //This statement checks to see if gameobject containing the ball's labels are turned on.
        //If so, the ball's current velocity in the vertical and horizontal component are displayed live.
        if (BallLabels.activeSelf == true)
        {
            XVelocityText.text = uxVelocity.ToString() + "m/s";
            YVelocityText.text = uyVelocity.ToString() + "m/s";
        }

        //transform.Translate is similar to transform.position but instead adds the object's current coordinates
        //This prevents redundant coding of adding the checking the current ball's position and adding the
        //distance calculated to each x and y coordinate.
        transform.Translate(new Vector3(xDistance, yDistance, 0));

        //Calculations is called again to update the distance values as the progresses through its flight
        Calculations();
    }

    //Called when the Reset button is clicked, ResetFunc clears all changes made after ball was launched
    //Moving the ball back to its original position.
    public void ResetFunc()
    {
        enabled = false;
        Line.Clear();
        Line.enabled = false;

        InputUVelocity.text = "";
        InputAngle.text = "";
        MsgTextbox.text = "";
        XVelocityText.text = "0";
        YVelocityText.text = "0";

        //If statement checks whether or not wall is loaded and thus changing the ball's position accordingly
        if (Wall.activeSelf == true)
        {
            Ball.transform.position = new Vector3(-655f, -56, 100f); 
        }
        else
        {
            Ball.transform.position = new Vector3(-655f, -228f, 100f);
        }
        collision = false;
    }
}
