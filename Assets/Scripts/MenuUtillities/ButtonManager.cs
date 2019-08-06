using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {
    private static ButtonManager instance = null;
    public static ButtonManager Instance
    {
        get {
            if (instance == null) {
                // This is where the magic happens.
                instance = FindObjectOfType(typeof(ButtonManager)) as ButtonManager;
            }

            // If it is still null, create a new instance
            if (instance == null) {
                //error;
            }
            return instance;
        }
    }

    [SerializeField]
    private GameObject loginScreen, mainMenu, statistics, registerButton, changeNameMenu, highscoreMenu;

    private void Start() {
        StartCoroutine(DatabaseFunctions.CheckSession()); 
    }

    public void OnStartButton() {
        SceneManager.LoadScene(1);
    }

    public void BackToMainMenu() {
        SceneManager.LoadScene(0);
    }

    public void OnStatistics() {
        mainMenu.SetActive(false);
        registerButton.SetActive(false);
        statistics.SetActive(true);
    }

    public void OnHighScores() {
        mainMenu.SetActive(false);
        statistics.SetActive(false);
        changeNameMenu.SetActive(false);
        highscoreMenu.SetActive(true);
    }

    public void OnChangeNameMenu() {
        mainMenu.SetActive(false);
        registerButton.SetActive(false);
        highscoreMenu.SetActive(false);
        changeNameMenu.SetActive(true);
    }

    public void OnMainMenu() {
        statistics.SetActive(false);
        loginScreen.SetActive(false);
        registerButton.SetActive(false);
        changeNameMenu.SetActive(false);
        highscoreMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void LoginScreen() {
        statistics.SetActive(false);
        loginScreen.SetActive(true);
        registerButton.SetActive(true);
        changeNameMenu.SetActive(false);
        highscoreMenu.SetActive(false);
        mainMenu.SetActive(false);
    }

    public void OnQuit() {
        Application.Quit();
    }
}
