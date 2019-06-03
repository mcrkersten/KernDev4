using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {
    public TextMeshProUGUI username;
    public TextMeshProUGUI password;

    public GameObject loginScreen;
    public GameObject mainMenu;

    public void OnStartButton() {
        SceneManager.LoadScene(1);
    }

    public void OnStatistics() {
        SceneManager.LoadScene(2);
    }

    public void OnHighScores() {
        SceneManager.LoadScene(3);
    }

    public void OnMainMenu() {
        SceneManager.LoadScene(0);
    }

    public void OnQuit() {
        Application.Quit();
    }

    public void Login() {
        if (LoginSystem.Login(username.text, password.text)) { //SUCCES
            loginScreen.SetActive(false);
            mainMenu.SetActive(true);
        }
        else { //FAILED
            //Trigger error message
        }
    }

    public void Register() {
        LoginSystem.Register(username.text, password.text);
    }
}
