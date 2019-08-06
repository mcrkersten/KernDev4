using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WelcomeUpdate : MonoBehaviour
{
    public void OnEnable() {
        StartCoroutine(DatabaseFunctions.GetUsername(this.GetComponent<TextMeshPro>(), "Welcome "));
    }
}
