using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public class ChangeNameSystem : MonoBehaviour
{
    [Header("Change Name")]
    [SerializeField]
    private TMP_InputField changeNameToField, changeNamePassword;
    [SerializeField]
    private TextMeshPro errorField3;
    [SerializeField]
    private Button changeNameButton;

    // Start is called before the first frame update
    void Start()
    {
        changeNameButton.onClick.AddListener(SendNameChangeRequestToServer);
    }

    private void SendNameChangeRequestToServer() {
        StartCoroutine(DatabaseFunctions.UpdateNameRequest(changeNameToField.text, changeNamePassword.text, errorField3));
    }
}
