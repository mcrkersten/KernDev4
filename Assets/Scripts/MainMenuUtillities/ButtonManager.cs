using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
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
}
