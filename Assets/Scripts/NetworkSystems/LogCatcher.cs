using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogCatcher : MonoBehaviour
{
    public Color warningColor;
    private List<GameObject> logObject = new List<GameObject>();
    public GameObject logPrefab;
    [HideInInspector]
    public string output = "";
    [HideInInspector]
    public string stack = "";

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        output = logString;
        stack = stackTrace;
        if(output.Substring(0,  8) != "Argument") {
            if(output.Substring(0, 8) != "[Server]") {
                CreateLogObject(output);
            }
        }
    }

    void CreateLogObject(string log) {
        foreach(GameObject x in logObject) {
            x.transform.position = new Vector3(x.transform.position.x, x.transform.position.y, x.transform.position.z - 1.5f);
        }
        GameObject newLog;
        logObject.Add(newLog = Instantiate(logPrefab, this.transform));
        newLog.GetComponent<TextMeshPro>().text = log;
        newLog.transform.position = new Vector3(-5, 0, 15);
        MessageInspector(newLog);

    }

    void MessageInspector(GameObject newLog) {
        //ON DISCONNECT
        if (output.Substring(0, 12) == "Disconnected") {
            newLog.GetComponent<TextMeshPro>().color = warningColor;
        }
    }
}
