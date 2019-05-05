using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerHandler : MonoBehaviour
{
    public TextMeshPro countdownText;
    public TextMeshPro countdownTitleText;
    public GameObject counter;

    // Start is called before the first frame update
    void OnEnable() {
        ClientBehaviour.OnStartCountdown += StartCountdown; 
    }

    private void OnDisable() {
        ClientBehaviour.OnStartCountdown -= StartCountdown;
    }

    public void StartCountdown(int countdownNumber, int countdownLenght) {

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
                countdownTitleText.text = "Turn ends in:";
                break;
            case 4:
                countdownTitleText.text = "Enemy turn ends in";
                break;
        }

        StartCoroutine(CountDown(countdownLenght));
    }

    IEnumerator CountDown(int time) {
        counter.SetActive(true);
        float normalizedTime = time;
        while (normalizedTime >= 0) {
            countdownText.text = Mathf.Round(normalizedTime).ToString();
            normalizedTime -= Time.deltaTime;
            yield return null;
        } 
    }
}
