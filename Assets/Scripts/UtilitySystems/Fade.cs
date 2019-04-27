using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Fade : MonoBehaviour
{
    public float time = 7;
    private bool faded = false;

    private void Update() {
        time -= Time.deltaTime;
        if(time < 0 && !faded) {
            faded = true;
            StartCoroutine(FadeTo(0, 1));
        }
    }

    IEnumerator FadeTo(float aValue, float aTime) {
        float alpha = transform.GetComponent<TextMeshPro>().color.a;
        Color newColor = transform.GetComponent<TextMeshPro>().color;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime) {
            newColor = new Color(newColor.r, newColor.g, newColor.b, Mathf.Lerp(alpha, aValue, t));
            transform.GetComponent<TextMeshPro>().color = newColor;
            yield return null;
        }
    }
}
