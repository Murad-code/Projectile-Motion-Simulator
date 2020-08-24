using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class Menu : MonoBehaviour
{
    GameObject mainMenu;

    Button StartButton;
    Button QuitButton;

    Dropdown Dropdown;
    TextMeshProUGUI ErrorTextbox;

    //This variable is used as a prefab, so when the MainActivity script opens, this variable can be accessed
    public static int selectedMode;

    //The following list are all the options added to the dropdown in the main menu 
    List<string> modes = new List<string>() { "Select Mode", "Playground", "Questions" };


    void Start()
    {
        //mainMenu is the container which holds all the Menu UI elements 
        mainMenu = transform.GetChild(1).gameObject;
        //ErrorTextbox will print any error messages to guide the user through the menu
        ErrorTextbox = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        StartButton = mainMenu.transform.GetChild(0).GetComponent<Button>();
        Dropdown = mainMenu.transform.GetChild(1).GetComponent<Dropdown>();
        QuitButton = mainMenu.transform.GetChild(2).GetComponent<Button>();
        // Below is code which adds the modes to the dropdown
        Dropdown.AddOptions(modes);
        MenuScreen();
    }

    public void LoadGame()
    {
        //The value selected by the user is assigned to variable seletedMode
        selectedMode = Dropdown.value;
        PlayerPrefs.SetInt("selectedMode", selectedMode);

        //Validation to ensure user has selected a mode 
        if (selectedMode != 0)
        {
            //Load the next scene and script
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            ErrorTextbox.text = "Error: Please select a mode";
        }
    }

    public void MenuScreen()
    {
        StartButton.onClick.AddListener(LoadGame);
        QuitButton.onClick.AddListener(QuitGame);
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}