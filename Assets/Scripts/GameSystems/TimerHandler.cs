using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerHandler : MonoBehaviour
{
    public TextMeshPro countdownText;
    public TextMeshPro countdownTitleText;
    private Color colorRed;
    private Color colorWhite;
    public GameObject counter;
    private IEnumerator coroutine;

    // Start is called before the first frame update
    void OnEnable() {
        colorRed = Color.red;
        colorWhite = Color.white;
        ClientBehaviour.OnStartCountdown += StartCountdown; 
    }

    private void OnDisable() {
        ClientBehaviour.OnStartCountdown -= StartCountdown;
    }

    public void StartCountdown(int countdownNumber, int countdownLenght) {

        if(coroutine != null) {
            StopCoroutine(coroutine);
        }
        coroutine = CountDown(countdownLenght);     
        StartCoroutine(coroutine);
        switch (countdownNumber) {
            case 0:
                countdownTitleText.text = "Ship placement starts in:";
                break;
            case 1:
                countdownTitleText.text = "Ship placement ends in:";
                break;
            case 2:
                countdownTitleText.text = "Game starts in:";
                break;
            case 3:
                countdownTitleText.text = "Your turn ends in:";
                break;
            case 4:
                countdownTitleText.text = "Enemy turn ends in:";
                break;
        }
    }

    IEnumerator CountDown(int time) {
        counter.SetActive(true);
        float normalizedTime = time;
        while (normalizedTime >= 0) {
            if(normalizedTime < 5) {
                countdownText.color = Color.Lerp(Color.red, Color.white, normalizedTime/time);
            }
            countdownText.text = Mathf.Round(normalizedTime).ToString();
            normalizedTime -= Time.deltaTime;
            yield return null;
        } 
    }
}
