using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogCatcher : MonoBehaviour
{
    public Color disconnectColor;
    public Color connectedColor;
    private List<GameObject> logObject = new List<GameObject>();
    public GameObject logPrefab;
    [HideInInspector]
    public string output = "";
    [HideInInspector]
    public string stack = "";

    private GameObject disconnected;

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        output = logString;
        stack = stackTrace;
        if (output.Length > 7) {
            if (output.Substring(0, 8) != "Argument") {
                if (output.Substring(0, 8) != "[Server]") {
                    if(output.Substring(0, 8) != "[Client]") {
                        if(output.Length > 11) {
                            if (output.Substring(0, 12) != "[Game Event]") {
                                CreateLogObject(output);
                            }
                        }
                        else {
                            CreateLogObject(output);
                        }
                    }
                }
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
        if(output.Length == 12) {
            if (output.Substring(0, 12) == "Disconnected") {
                logObject.Remove(newLog);
                newLog.transform.position = new Vector3(-5, 0, 16.5f);
                newLog.GetComponent<TextMeshPro>().color = disconnectColor;
                disconnected = newLog;
            }
        }

        //ON CONNECT
        else if (output.Length == 9) {
            if (output.Substring(0, 9) == "Connected") {
                newLog.GetComponent<TextMeshPro>().color = connectedColor;
                newLog.GetComponent<Fade>().enabled = true;
            }
        }
        else {
            newLog.GetComponent<Fade>().enabled = true;
            if (disconnected != null) {
                disconnected.GetComponent<Fade>().enabled = true;
            }
        }
    }
}
