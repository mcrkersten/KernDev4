using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject forfeit, win, loss;

    [SerializeField]
    private GameObject[] objectsToDisableOnForfeit, objectsToDisableOnWin, objectsToDisableOnLoss;

    [SerializeField]
    private Material materialColorOnForfeit;

    [SerializeField]
    private Renderer groundRenderer;

    public void OpenForfeitMenu() {
        for(int x = 0; x < objectsToDisableOnForfeit.Length - 1; x++) {
            objectsToDisableOnForfeit[x].SetActive(false);
        }
        forfeit.SetActive(true);
        groundRenderer.material = materialColorOnForfeit;
    }

    public void OpenWinMenu() {
        for (int x = 0; x < objectsToDisableOnForfeit.Length - 1; x++) {
            objectsToDisableOnWin[x].SetActive(false);
        }
        win.SetActive(true);
        groundRenderer.material = materialColorOnForfeit;
        StartCoroutine(DatabaseFunctions.UpdateGameToDatabase(UserInformation.Instance.userID, 1));
    }

    public void OpenLossMenu() {
        for (int x = 0; x < objectsToDisableOnLoss.Length - 1; x++) {
            objectsToDisableOnLoss[x].SetActive(false);
        }
        loss.SetActive(true);
        groundRenderer.material = materialColorOnForfeit;
        StartCoroutine(DatabaseFunctions.UpdateGameToDatabase(UserInformation.Instance.userID, 0));
    }
}
