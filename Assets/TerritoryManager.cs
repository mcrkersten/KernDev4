using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryManager : MonoBehaviour
{
    public bool isPlayerTerritory;
    private GameObject hitAnimation;
    private GameObject missAnimation;
    private List<WaterBlock> blocks = new List<WaterBlock>();

    private void OnEnable() {

        hitAnimation = Resources.Load("Prefabs/HitAnimation", typeof(GameObject)) as GameObject;
        missAnimation = Resources.Load("Prefabs/missAnimation", typeof(GameObject)) as GameObject;

        CoordinateManager.OnFireOnShip += PlayAnimation;


        foreach(Transform child in this.gameObject.transform) {
            blocks.Add(child.gameObject.GetComponent<WaterBlock>());
        }
    }

    private void OnDisable() {
        CoordinateManager.OnFireOnShip -= PlayAnimation;
    }

    private void PlayAnimation(Vector2 coordinate, Coordinate state, bool isPlayerTerritory) {
        if(isPlayerTerritory == this.isPlayerTerritory) {
            foreach (WaterBlock x in blocks) {
                if (x.blockCoordinate == coordinate) {
                    if (state == Coordinate.hit || state == Coordinate.ship) {
                        Instantiate(hitAnimation, x.gameObject.transform);
                    }
                    else {
                        Instantiate(missAnimation, x.gameObject.transform);
                    }
                    x.isPlayed = true;
                }
            }
        }
    }
}
