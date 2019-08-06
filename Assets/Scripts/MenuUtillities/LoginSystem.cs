using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public class LoginSystem : MonoBehaviour {
    [Header("Login")]
    public GameObject loginFields;
    public GameObject mainMenu;
    public TMP_InputField username1;
    public TMP_InputField password1;
    public TextMeshPro errorField2;


    [Header("Register")]
    public GameObject registerFields;
    public TMP_InputField username2;
    public TMP_InputField password2;
    public TMP_InputField repeatPassword;
    public TMP_InputField email;
    public TextMeshPro errorField;

    [Header("Buttons")]
    public Button logInButton;
    public Button registerButton;
    public Button switchField;

    public TextMeshPro welcomeText;
    public TextMeshPro registerButtonText;

    private ConnectionType connectionType = ConnectionType.Login;
    private string databaseID;

    public void Start() {
        logInButton.onClick.AddListener(Submit);
        registerButton.onClick.AddListener(Submit);
        switchField.onClick.AddListener(SwitchField);
    }

    public void Submit() {
        switch (connectionType) {
            case ConnectionType.Login:
                StartCoroutine(ServerCallLogin());
                break;

            case ConnectionType.Logout:
                StartCoroutine(ServerCallLogout());
                break;

            case ConnectionType.Register:
                StartCoroutine(ServerCallRegister());
                break;
        }
    }

    #region Login
    IEnumerator ServerCallLogin() {
        WWWForm form = new WWWForm();
        form.AddField("username", username1.text);
        form.AddField("password", password1.text);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/Login.php", form);
        yield return www.SendWebRequest();
        //Check for errors:

        switch (www.downloadHandler.text) {
            case string a when a.Contains("Warning"):
                Debug.Log("Server most likely offline");
                break;
            case string a when a.Contains("PERROR"):
                Debug.Log("Session does not exist");
                break;
            case string a when a.Contains("QERROR"):
                Debug.Log("Wrong Query");
                break;
            case string a when a.Contains("FAIL"):
                errorField2.text = "Password or user not correct";
                break;
            case "":
                errorField2.text = "Server offline";
                break;
            default:
                print(www.downloadHandler.text);
                Debug.Log("Succes");
                UserInformation.Instance.userID = www.downloadHandler.text;
                Succes();
                break;
        }
    }
    #endregion

    #region Logout
    IEnumerator ServerCallLogout() {
        UnityWebRequest www = UnityWebRequest.Get("https://studenthome.hku.nl/~max.kersten/PHP/Logout.php");
        yield return www.SendWebRequest();
        //Check for errors:
        if (www.downloadHandler.text.Contains("Warning")) {    //Connection error = database servers are off
            Debug.Log("Server most likely offline");
        }
        else
        if (www.downloadHandler.text.Contains("PERROR")) {    //Session error = the session does not exist, which means the player has logged out
            Debug.Log("Session does not exist");
        }
        else
        if (www.downloadHandler.text.Contains("QERROR")) {    //Query error = the query is wrong (so the code needs to be updated)
            Debug.Log("Wrong Query");
        }
        else
        if (www.downloadHandler.text.Contains("ERROR")) {     //Data Error = the data is wrong, so the player may have signed a wrong username of password
            Debug.Log("Could not log in");
        }
        else {
            Debug.Log("Succes");
            Succes();
        }
    }
    #endregion

    #region Register
    IEnumerator ServerCallRegister() {
        WWWForm form = new WWWForm();
        form.AddField("username", username2.text);
        form.AddField("password", password2.text);
        form.AddField("reenter", repeatPassword.text);
        form.AddField("email", email.text);
        //form.AddField("email", email.text);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/Register.php", form);
        yield return www.SendWebRequest();
        //Check for errors:

        switch (www.downloadHandler.text) {
            case string a when a.Contains("Warning"):
                errorField.text = "Server most likely offline";
                break;
            case string a when a.Contains("PERROR"):
                errorField.text = "Session does not exist";
                break;
            case string a when a.Contains("QERROR"):
                errorField.text = "Wrong Query";
                break;
            case string a when a.Contains("NAME-ERROR"):
                errorField.text = "Illigal sign in name: only [aA-zZ 0-9]";
                break;
            case string a when a.Contains("NAME-TAKEN"):
                errorField.text = "Name is not available";
                break;
            case string a when a.Contains("PASS-ERROR1"):
                errorField.text = "Passwords do not match";
                break;
            case string a when a.Contains("PASS-ERROR2"):
                errorField.text = "Password minimal 5 characters";
                break;
            case string a when a.Contains("EMAIL-ERROR"):
                errorField.text = "Check if email is correct";
                break;
            case string a when a.Contains("REGISTERED"):
                Succes();
                break;
        }
    }
    #endregion

    public void Succes() {
        switch (connectionType) {
            case ConnectionType.Login:
                LoggedIn();
                switchField.gameObject.SetActive(false);
                StartCoroutine(DatabaseFunctions.GetUsername(welcomeText, "Welcome "));
                break;
            case ConnectionType.Register:
                SwitchField();
                break;
            case ConnectionType.Logout:
                break;
        }
    }

    private void SwitchField() {
        switch (connectionType) {
            case ConnectionType.Login:
                loginFields.SetActive(false);
                registerFields.SetActive(true);
                registerButtonText.text = "Login";
                connectionType = ConnectionType.Register;
                break;
            case ConnectionType.Register:
                registerFields.SetActive(false);
                loginFields.SetActive(true);
                registerButtonText.text = "Register";
                connectionType = ConnectionType.Login;
                break;
        }
    }

    private void LoggedIn() {
        loginFields.SetActive(false);
        mainMenu.SetActive(true);
    }
}

public enum ConnectionType {
    Login,
    Logout,
    Register
}