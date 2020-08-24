using System;
using Mono.Data.Sqlite;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class QuestionMode : MonoBehaviour
{

    Canvas Canvas;
    GameObject Ball;
    GameObject Wall;
    TextMeshProUGUI WallText;
    TrailRenderer Line;

    TextMeshProUGUI QuestionText;
    Button NextBtn;
    InputField AnswerField;

    GameObject ActualAnswer;
    GameObject MarkScheme;
    Button PlayBtn;
    Button AnswerBtn;
    Button MarkSchemeBtn;
    TextMeshProUGUI ActualAnswerText;
    TextMeshProUGUI MethodText;
    TextMeshProUGUI MsgTextbox;

    GameObject Settings;
    Button SettingsBtn;
    TextMeshProUGUI QuestionCountText;
    int QuestionCount = 5;
    Button UpArrowBtn;
    Button DownArrowBtn;
    Dropdown DifficultyDropdown;
    //The list is used to cycle through the different difficulties picked by the user
    List<string> DifficultyType = new List<string>() { "Easy", "Medium", "Hard"};

    int index;
    int count;
    int score;

    float RdnUVel;
    float RdnAngle;
    float AngleRadians;
    float Rdns0;

    Button FullscreenBtn;
    Button BackBtn;
    public static int Fullscreen;
   

    void Start()
    {
        Canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        QuestionText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        AnswerField = transform.GetChild(1).GetComponent<InputField>();
        PlayBtn = transform.GetChild(3).GetComponent<Button>();
        NextBtn = transform.GetChild(4).GetComponent<Button>();
        AnswerBtn = transform.GetChild(2).GetComponent<Button>();

        ActualAnswer = transform.GetChild(7).gameObject;
        MarkSchemeBtn = ActualAnswer.transform.GetChild(0).GetComponent<Button>();
        ActualAnswerText = MarkSchemeBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        MarkScheme = ActualAnswer.transform.GetChild(2).gameObject;
        MethodText = MarkScheme.transform.GetChild(1).GetComponent<TextMeshProUGUI>();


        Settings = transform.GetChild(6).gameObject;
        SettingsBtn = transform.GetChild(5).GetComponent<Button>();
        QuestionCountText = Settings.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        UpArrowBtn = Settings.transform.GetChild(2).GetComponent<Button>();
        DownArrowBtn = Settings.transform.GetChild(3).GetComponent<Button>();
        DifficultyDropdown = Settings.transform.GetChild(4).GetComponent<Dropdown>();
        DifficultyDropdown.AddOptions(DifficultyType);
        FullscreenBtn = transform.GetChild(8).GetComponent<Button>();
        BackBtn = transform.GetChild(9).GetComponent<Button>();

        //This variable stores the value passed through the prefab in the menu or when scenes change
        //between fullscreen and normal QuestionMode so it is updated
        Fullscreen = PlayerPrefs.GetInt("Fullscreen");
        print(Fullscreen);

        //The prefab Fullscreen determines whether or not the user clicked on expanding the view of the quiz
        //If the quiz is fullscreen, gameobjects such as the wall and ball are not needed hence they are
        //only assigned to their UI elements when the Quiz mode is in the top right corner of the screen.
        if (Fullscreen == 0)
        {
            Ball = GameObject.Find("Ball").gameObject;
            Wall = Canvas.transform.GetChild(13).gameObject;
            WallText = Wall.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            Line = Ball.transform.GetComponent<TrailRenderer>();
            MsgTextbox = Canvas.transform.GetChild(16).GetComponent<TextMeshProUGUI>();
        }
        else if (Fullscreen == 1)
        {
            MsgTextbox = transform.GetChild(10).GetComponent<TextMeshProUGUI>();
        }


        //FullscreenBtn and BackBtn change the scene view so the quiz can enter and exit fullscreen mode.
        FullscreenBtn.onClick.AddListener(delegate
        {
            PlayerPrefs.SetInt("Fullscreen", 1);
            SceneManager.LoadScene("QuizFullscreen");
        });

        BackBtn.onClick.AddListener(delegate
        {
            PlayerPrefs.SetInt("Fullscreen", 0);
            SceneManager.LoadScene("MainScene");
        });

        //This function is for the Quiz play button and calls a procedure from the MainActivity script
        //In order to prevent any errors by resetting the ball and it's location
        PlayBtn.onClick.AddListener(delegate
        {
            if (Fullscreen == 0)
            {
                MainActivity main = Ball.GetComponent<MainActivity>();
                main.ResetFunc();
            }

            count = 0;
            DatabaseQuestion();
            UI();

        });

        SettingsBtn.onClick.AddListener(ActivateSettings);
    }


    //UI function attachs the gameobjects in the editor to their corresponding code
    //Next button has an internal function which is carried to make sure user has entered an answer
    //and clears the answer field when the user moves onto the next question.

    void UI()
    {
        UpArrowBtn.onClick.AddListener(UpArrow);
        DownArrowBtn.onClick.AddListener(DownArrow);
        NextBtn.onClick.AddListener(delegate {
        if (AnswerField.text != "")
            {
                if (Fullscreen == 0)
                {
                    MainActivity main = Ball.GetComponent<MainActivity>();
                    main.ResetFunc();
                }
                MsgTextbox.text = "";
                DatabaseQuestion();
                AnswerField.text = "";
            }
        else
            {
                MsgTextbox.text = "Enter an answer to move onto the next question";
            }
        });
        AnswerBtn.onClick.AddListener(ActivateAnswer);
        MarkSchemeBtn.onClick.AddListener(ActivateMarkScheme);
    }


    //ActivateSettings and ActivateAnswer are similar to the ActivateWall function in terms of their purpose.
    //This is to check if they are currently spawned in the UI and if not they are activated.
    //Also, the settings and answer are positioned very close to each other so that they overlap. 
    //To overcome this, one is deactivated when the other is switched on. My reasoning for this is to prevent
    //the UI from taking up to much screen space for use in class. The ActivateMarkScheme function is a child
    //gameobject of the ActivateAnswer hence why I don't need to deactivate the settings in its code.

    void ActivateSettings()
    {
        if (Settings.activeSelf == true)
        {
            Settings.SetActive(false);
        }
        else
        {
            Settings.SetActive(true);
            ActualAnswer.SetActive(false);
        }
    }

    void ActivateAnswer()
    {
        if (ActualAnswer.activeSelf == true)
        {
            ActualAnswer.SetActive(false);
        }
        else
        {
            ActualAnswer.SetActive(true);
            Settings.SetActive(false);
        }
    }

    void ActivateMarkScheme()
    {
        if (MarkScheme.activeSelf == true)
        {
            MarkScheme.SetActive(false);
        }
        else
        {
            MarkScheme.SetActive(true);
        }
    }

    //The up and down arrow function are seperate to the MainActivity script and because they are both private
    //(if not stated otherwise, C# makes the function private automatically) they are not affecting each other
    //These functions change the value of the number of questions chosen by the user in the settings menu.
    //As you can read below the lower and upper limits of the number of questions are 1 and 10 for the quiz.

    void UpArrow()
    {
        if (QuestionCount < 10)
        {
            ++QuestionCount;
            QuestionCountText.text = QuestionCount.ToString();
        }
    }

    void DownArrow()
    {
        if (QuestionCount > 1)
        {
            --QuestionCount;
            QuestionCountText.text = QuestionCount.ToString();
        }
    }

    //DatabaseQuestion is the function which selects the question to be displayed in the quiz by querying
    //the database.
   
    void DatabaseQuestion()
    {
        //Initial difficulty is easy hence why the limit is 4
        int limit = 4;
        //UserDifficulty gets the pointer value of the mode selected in the difficulty dropdown
        int UserDifficulty = DifficultyDropdown.value;

        //Switch statement uses the UserDifficulty value to assign the limit
        //The limit is used in the range of creating a random index value
        //The different values for limit correlate to the difficulty and number of questions in the database
        //So the the higher the number, the more records and hence questions can be queried in the database
        //The more difficult questions are at the bottom of the database table. So easy to hard is 1-8
        //The smallest possible limit is 4 to ensure that the quiz has varied questions.
        switch (UserDifficulty)
        {
            case 0:
                limit = 4;
                break;
            case 1:
                limit = 6;
                break;
            case 2:
                //Limit is 9 here because UnityEngine.Random.Range returns a random integer number between
                //and inclusive minimum but an exclusive maximum hence why the limit is 9 when I have 8 
                //records in the database.
                limit = 9;
                break;
        }
        index = UnityEngine.Random.Range(1, limit);
        //If the index is greater than 3, then every record in the database (4-8)
        if (index > 3)
        {
            Rdns0 = UnityEngine.Random.Range(5, 15);
            //Check if fullscreen mode is on because if it is then the Wall and related objects are not needed
            if (Fullscreen == 0)
            {
                Wall.SetActive(true);
                Ball.transform.position = new Vector3(-630f, -56f, 0f);
                WallText.text = Rdns0 + "m";
            }

            //This is responsible for generating a random value for initial height, a value which may be
            //required in producing certain questions
        }
        else
        {
            if (Fullscreen == 0)
            {
                Wall.SetActive(false);
                Ball.transform.position = new Vector3(-655f, -228f, 0f);
            }
        }

        //This section of code is used to generate random values for the velocity and angle 
        RdnAngle = UnityEngine.Random.Range(10, 90);
        AngleRadians = RdnAngle * Mathf.PI / 180;
        RdnUVel = UnityEngine.Random.Range(10, 50);

        //Finds the location of the database and opens a connection to the database
        string conn = "URI=file:" + Application.dataPath + "/Question.db";
        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        dbconn.Open(); 
        
        //dbcmd is used to interrogate the database
        //The index value is used here again to decide which record to search for in the database.
        IDbCommand dbcmd = dbconn.CreateCommand();
        //SQL code used to make a query in the databse in oerder to find a record using index as a pointer
        string sqlQuery = "SELECT * FROM QuestionSet WHERE QuestionID = " + index;
        dbcmd.CommandText = sqlQuery;
        IDataReader reader = dbcmd.ExecuteReader();

        //As long as there are records which can be read from the database, the following code is used to 
        //substitute the keywords which are encapsulated in '{}' brackets in the question to assign
        //a random number value to make the question before it is displayed to the user
        while (reader.Read())
        {
            string Question = reader.GetString(1);
            Question = Question.Replace("{UV}", RdnUVel.ToString()).Replace("{A}", RdnAngle.ToString());
            if (index > 3)
            {
                Question = Question.Replace("{s0}", Rdns0.ToString());
            }
            QuestionText.text = (Question);
        }

        //The following lines of code are used to disconnect from the database to prevent leaking data.
        //Essentially, it is good practice to close connections whenever the database is not in use so
        //the data stored in the database is no longer accessible.
        reader.Close();
        reader = null;
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Close();
        dbconn = null;

        //DatabaseMethod function is called and the index value is passed in as the argument
        //This is to allow the DatabaseMethod to find the corresponding data to the question
        DatabaseMethod(index);

    }

    void DatabaseMethod(int i)
    {
        //Argument is assigned to the index variable in this function
        index = i;


        //Following code is similar to that above in the DatabaseQuestion. However, a different field is
        //accessed in this function
        string conn = "URI=file:" + Application.dataPath + "/Question.db";
        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        dbconn.Open();

        IDbCommand dbcmd = dbconn.CreateCommand();
        string sqlQuery = "SELECT * FROM QuestionSet WHERE QuestionID = " + index;
        dbcmd.CommandText = sqlQuery;
        IDataReader reader = dbcmd.ExecuteReader();

        //The value in the parameters, '2', is used to access the MarkScheme field of the database.
        while (reader.Read())
        {
            string Method = reader.GetString(2);

            //Calling the Type functions passing the index and Method values in the parameters as they are
            //needed to distinguish which type of method to use to calculate the answer and adjust
            //the unique keywords in the markscheme and input appropriate values
            Type(index, Method);
        }

        reader.Close();
        reader = null;
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Close();
        dbconn = null;
    }


    void Type(int i, string method)
    {

        //Switch statement compares the value of index to the number beside the case
        switch (index)
        {
            case 1:
                Time(method, 1);
                break;
            case 2:
                VerticalDistance(method, 1);
                break;
            case 3:
                HorizontalDistance(method, 1);
                break;
            case 4:
                Time(method, 2);
                break;
            case 5:
                HorizontalDistance(method, 2);
                break;
            case 6:
                VerticalDistance(method, 2);
                break;
            case 7:
                SpeedAngle(method, 1);
                break;
            case 8:
                SpeedAngle(method, 2);
                break;
        }
    }

    //The following functions are all related to calculating the final answer depending on the question
    //being asked so it can then be compared to the user's answer.
    //Each function can diverge into a different if statement depending on the argument passed through.
    //This is to prevent writing redundant code and extra functions which would be nearly identical.

    //In each If statement, mathematical calculations which include the use of suvat equations are
    //carried out to find the answer.

    //Also every function below will have a similar ending which decides which keywords to select in the
    //method string and replace it with the values calculated so it can be displayed as the mark scheme.
    //.ToString() is used to temporarily convert the data type of the values calculated to a string format
    //in order to be substituted for the method string's keywords.


    //Time function is used to calculate answers to the two types of questions which ask for the time

    void Time(string m, int i)
    {
        index = i;
        string Method = m;

        //Time value must be assigned to zero even though it is automatically assigned to this value
        //This is because Unity will not permit calculations to happen unless a value is assigned as the
        //code inside the if statements are disregarded during this process.
        float time = 0;


        if (index == 1)
        {
            time = 2 * RdnUVel * Mathf.Sin(AngleRadians) / 9.8f;
            Method = Method.Replace("{UV}", RdnUVel.ToString()).Replace("{A}", RdnAngle.ToString());
        }
        else if (index == 2)
        {
            time = Mathf.Sqrt(Rdns0 / 4.9f);
            Method = Method.Replace("{s0}", Rdns0.ToString());
        }

        //answer is rounded to 2 decimal places
        time = (float)Math.Round(time, 2);

        ActualAnswerText.text = time.ToString() + "s";
        Method = Method.Replace("{ANS}", time.ToString());
        MethodText.text = Method;
        Score(time);
    }

    //This function is used to calculate types of questions asking for the maximum horizontal distance

    void HorizontalDistance(string m, int i)
    {
        index = i;
        string Method = m;
        float time = 0; // time and xDistance must be assigned a value so code at bottom can be accepted
        float xDistance = 0; 

        if (index == 1)
        {
            time = 2 * RdnUVel * Mathf.Sin(AngleRadians) / 9.8f;
            xDistance = RdnUVel * Mathf.Cos(AngleRadians) * time;
            Method = Method.Replace("{A}", RdnAngle.ToString());
        }
        else if (index == 2)
        {
            time = Mathf.Sqrt(2*Rdns0 / 4.9f);
            xDistance = time * RdnUVel;
            Method = Method.Replace("{s0}", Rdns0.ToString());
        }

        xDistance = (float)Math.Round(xDistance, 2);

        ActualAnswerText.text = xDistance.ToString() + "m";
        Method = Method.Replace("{T}", time.ToString()).Replace("{ANS}", xDistance.ToString())
        .Replace("{UV}", RdnUVel.ToString());
        MethodText.text = Method;
        Score(xDistance);

    }

    void VerticalDistance(string m, int i)
    {
        index = i;
        string Method = m;
        float time;
        float yDistance;

        time = RdnUVel * Mathf.Sin(AngleRadians) / 9.8f;
        yDistance = RdnUVel * Mathf.Sin(AngleRadians) * time + .5f * -9.8f * Mathf.Pow(time, 2);

        if (index == 2)
        {
            yDistance = Rdns0 + yDistance;
            Method = Method.Replace("{s0}", Rdns0.ToString());
        }

        yDistance = (float)Math.Round(yDistance, 2);

        Method = Method.Replace("{T}", time.ToString()).Replace("{ANS}", yDistance.ToString())
        .Replace("{UV}", RdnUVel.ToString()).Replace("{A}", RdnAngle.ToString());   
        ActualAnswerText.text = yDistance.ToString() + "m";
        MethodText.text = Method;
        Score(yDistance);
    }

    void SpeedAngle(string m, int i)
    {
        index = i;
        string Method = m;
        float yV;
        float Answer = 0;
        yV = Mathf.Sqrt(2f * 9.8f * Rdns0);
        if (index == 1)
        {
            Answer = Mathf.Sqrt(Mathf.Pow(yV, 2) * Mathf.Pow(RdnUVel, 2));
            Method = Method.Replace("{s0}", Rdns0.ToString().Replace("{H}", Answer.ToString()));
        }
        else if (index == 2)
        {
            Answer = Mathf.Atan(yV / RdnUVel);
            Answer = Answer * 180 / Mathf.PI;
            Method = Method.Replace("{A}", Answer.ToString());
        }

        Answer = (float)Math.Round(Answer, 2);
        if (index == 1)
        {
            ActualAnswerText.text = Answer.ToString() + "m/s";
        }
        else if (index == 2)
        {
            ActualAnswerText.text = Answer.ToString() + " degrees";
        }
        Method = Method.Replace("{UV}", RdnUVel.ToString()).Replace("{yV}", yV.ToString());
        MethodText.text = Method;
        Score(Answer);

    }

    //Score function compares the user's answer to the calculations and increments the user's score if correct

    void Score(float answer)
    {
        float UserAnswer = 0;
        count++;
        float Answer = answer;
        if (AnswerField.text != "")
        {
            UserAnswer = float.Parse(AnswerField.text);
        }

        if (Answer == UserAnswer)
        {
            score++;
        }

        //When the count is equal to the question count, the score is displayed to the user.

        if (count == QuestionCount)
        {
            QuestionText.text = "You have completed the quiz";
            QuestionText.text = ("Your score is " + score + "/" + QuestionCount);
        }

    }

}
