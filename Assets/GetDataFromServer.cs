using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GetDataFromServer : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro winNumber, playedNumber, topPlayers, yourPositionOnHighscore;

    private void OnEnable() {
        if (winNumber != null && playedNumber != null) {
            StartCoroutine(DatabaseFunctions.GetGamesWon(UserInformation.Instance.userID, winNumber));
            StartCoroutine(DatabaseFunctions.GetPlayedGames(UserInformation.Instance.userID, playedNumber));
        }
        if (topPlayers != null && yourPositionOnHighscore != null) {
            StartCoroutine(DatabaseFunctions.GetTopPlayers("0", topPlayers));
            StartCoroutine(DatabaseFunctions.GetHighscorePosition("0", yourPositionOnHighscore));
        }
    }
}
