using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBlock : MonoBehaviour {
    private float perlinNoise;
    private float multiplier = .5f;
    private float time;
    private Vector3 position;

    public Vector2 blockCoordinate { get; private set; }
    public Renderer objectRenderer { get; private set; }

    private ProcessFase currentFase;
    private bool selected;

    private Material playableMaterial;
    private Material nonPlayableMaterial;

    private Material playerPlayedMaterial;
    private Material enemyPlayedMaterial;

    private bool isOwnSea;
    public bool isPlayed = false;

    private void Start() {
        SetMaterials();
        if(this.transform.parent.name == "PlayerTerritory") {
            isOwnSea = true;
        }

        char[] blockCoordinateChars = this.gameObject.name.ToCharArray();
        blockCoordinate = new Vector2((int)char.GetNumericValue(blockCoordinateChars[0]), (int)char.GetNumericValue(blockCoordinateChars[2]));
        position = this.gameObject.transform.position;
        objectRenderer = this.gameObject.GetComponent<Renderer>();
        GameStateMachine.OnFaseChange += ChangeFaseHandler;
    }

    private void SetMaterials() {
        playableMaterial = Resources.Load("Materials/WaterTileTurn", typeof(Material)) as Material;
        nonPlayableMaterial = Resources.Load("Materials/WaterTileNoTurn", typeof(Material)) as Material;

        playerPlayedMaterial = Resources.Load("Materials/PlayerPlayed", typeof(Material)) as Material;
        enemyPlayedMaterial = Resources.Load("Materials/EnemyPlayed", typeof(Material)) as Material;
    }

    private void ChangeFaseHandler(ProcessFase fase) {
        currentFase = fase;
        switch (fase) {
            case ProcessFase.PlacingFase:
                if (isOwnSea) {
                    objectRenderer.material = playableMaterial;
                }
                break;

                //Change materials on EnemyTurn
            case ProcessFase.EnemyTurn:
                if (!isPlayed) {
                    objectRenderer.material = nonPlayableMaterial;
                } else if(isOwnSea){
                    objectRenderer.material = enemyPlayedMaterial;
                } else {
                    objectRenderer.material = playerPlayedMaterial;
                }

                if (CoordinateManager.Instance.selectedGameObject != null) {
                    CoordinateManager.Instance.selectedGameObject = null;
                }
                break;

                //Change materials on PlayerTurn
            case ProcessFase.PlayerTurn:
                if (!isPlayed && isOwnSea) {         //Is not played and is own sea
                    objectRenderer.material = nonPlayableMaterial;
                }else if (isPlayed && isOwnSea) {    //Is played and is own sea
                    objectRenderer.material = enemyPlayedMaterial;
                }else if(!isPlayed && !isOwnSea) {   //Is not played and is not own sea
                    objectRenderer.material = playableMaterial;
                }else if(isPlayed && !isOwnSea) {    //Is played and is not own sea
                    objectRenderer.material = playerPlayedMaterial;
                }
                break;
        }
    }

    private void OnMouseEnter() {
        if (currentFase == ProcessFase.PlayerTurn && !isOwnSea && !isPlayed) {          
            objectRenderer.material = nonPlayableMaterial;
        }
    }

    private void OnMouseOver() {
        if (currentFase == ProcessFase.PlayerTurn && !isOwnSea && !isPlayed) {

            if (Input.GetMouseButtonDown(0)) {
                selected = true;
                if (CoordinateManager.Instance.selectedGameObject != null) {
                    CoordinateManager.Instance.selectedGameObject.GetComponent<WaterBlock>().objectRenderer.material = playableMaterial;
                    CoordinateManager.Instance.selectedGameObject.GetComponent<WaterBlock>().selected = false;
                }
                CoordinateManager.Instance.selectedCoordinate = blockCoordinate;
                CoordinateManager.Instance.selectedGameObject = this.gameObject;
            }

        }
    }

    private void OnMouseExit() {
        if (currentFase == ProcessFase.PlayerTurn && !isOwnSea && !isPlayed) {
            if (!selected) {
                objectRenderer.material = playableMaterial;
            }
        }
    }

    private void Update() {
        time = Time.time/2;
        perlinNoise = Mathf.PerlinNoise(position.x /7 + time, position.z /7);
        this.gameObject.transform.position = new Vector3(position.x, position.y + (perlinNoise * multiplier), position.z);
    }

    private void OnDisable() {
        GameStateMachine.OnFaseChange -= ChangeFaseHandler;
    }
}
