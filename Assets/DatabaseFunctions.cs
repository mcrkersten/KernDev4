using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public static class DatabaseFunctions
{
    public static IEnumerator UpdateGameToDatabase(string gameID, int score) {
        WWWForm form = new WWWForm();
        form.AddField("game-id", gameID);
        form.AddField("score", score);
        form.AddField("local-id", UserInformation.Instance.userID);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/UpdateDatabase.php", form);
        yield return www.SendWebRequest();
    }

    public static IEnumerator UpdateNameRequest(string username, string password, TextMeshPro text) {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        form.AddField("local-id", UserInformation.Instance.userID);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/ChangeNameRequest.php", form);
        yield return www.SendWebRequest();
        text.text = www.downloadHandler.text;
    }

    public static IEnumerator GetPlayedGames(string gameID, TextMeshPro text) {
        WWWForm form = new WWWForm();
        form.AddField("game-id", gameID);
        form.AddField("local-id", UserInformation.Instance.userID);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/GetPlayedGames.php", form);
        yield return www.SendWebRequest();
        text.text = www.downloadHandler.text;
    }

    public static IEnumerator GetGamesWon(string gameID, TextMeshPro text) {
        WWWForm form = new WWWForm();
        form.AddField("game-id", gameID);
        form.AddField("local-id", UserInformation.Instance.userID);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/GetGamesWon.php", form);
        yield return www.SendWebRequest();
        text.text = www.downloadHandler.text;
    }

    public static IEnumerator GetUsername(TextMeshPro text, string toAdd = "") {
        WWWForm form = new WWWForm();
        form.AddField("local-id", UserInformation.Instance.userID);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/GetUsername.php", form);
        yield return www.SendWebRequest();
        text.text = toAdd + www.downloadHandler.text;
    }

    public static IEnumerator GetTopPlayers(string gameID, TextMeshPro text) {
        WWWForm form = new WWWForm();
        form.AddField("game-id", gameID);
        form.AddField("local-id", UserInformation.Instance.userID);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/GetTopPlayers.php", form);
        yield return www.SendWebRequest();
        text.text = www.downloadHandler.text;
        Debug.Log(www.downloadHandler.text);
    }

    public static IEnumerator GetHighscorePosition(string gameID, TextMeshPro text) {
        WWWForm form = new WWWForm();
        form.AddField("game-id", gameID);
        form.AddField("local-id", UserInformation.Instance.userID);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/GetHighscorePosition.php", form);
        yield return www.SendWebRequest();
        text.text = www.downloadHandler.text;
        Debug.Log(www.downloadHandler.text);
    }

    public static IEnumerator CheckSession() {
        WWWForm form = new WWWForm();
        form.AddField("local-id", UserInformation.Instance.userID);
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/PHP/CheckForActiveSession.php", form);
        yield return www.SendWebRequest();
        switch (www.downloadHandler.text) {
            case string a when a.Contains("WRONGUSER"):
                ButtonManager.Instance.LoginScreen();
                break;
            case string a when a.Contains("NOSESSION"):
                Debug.Log("test");
                ButtonManager.Instance.LoginScreen();
                break;
            default:
                ButtonManager.Instance.OnMainMenu();
                break;
        }
        Debug.Log(www.downloadHandler.text);
    }
}
