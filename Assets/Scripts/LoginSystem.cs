using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public class LoginSystem : MonoBehaviour
{
    public static LoginSystem instance;

    private void Start() {
        instance = this;
    }

    public static bool Login(string username, string password) {
        //Failed Login
        return false;
    }

    public static bool Register(string username, string password) {
        //Failed Registration
        return false;
    }


    private static void GetServerInfo() {
        instance.StartCoroutine(GetText());
    }

    static IEnumerator GetText() {
        UnityWebRequest request = UnityWebRequest.Get("https://studenthome.hku.nl/~Max.kersten/PHP/RequestServerIP.php");
        yield return request.SendWebRequest();
        if (request.isDone) {
            if (request.isNetworkError || request.isHttpError) {
                Debug.Log(request.error);
            }
            else {
                PlayerAccountInformation x = PlayerAccountInformation.CreateFromJSON(request.downloadHandler.text);
            }
        }
    }
}

public class PlayerAccountInformation {
    public string PlayerID;
    public string Username;

    public static PlayerAccountInformation CreateFromJSON(string jsonString) {
        return JsonUtility.FromJson<PlayerAccountInformation>(jsonString);
    }
}
