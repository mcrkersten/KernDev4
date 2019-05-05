using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveGenerator : MonoBehaviour {
    private float perlinNoise;
    private float multiplier = .5f;
    private float time;
    private Vector3 position;
    private Renderer objectRenderer;


    public Material playableColor;
    public Material nonPlayableColor;
    public bool isOwnSea;

    private void Start() {
        position = this.gameObject.transform.position;
        objectRenderer = this.gameObject.GetComponent<Renderer>();
        GameStateMachine.OnFaseChange += ChangeFaseHandler;
    }

    private void ChangeFaseHandler(ProcessFase fase) {
        if (fase == ProcessFase.PlacingFase) {
            if (isOwnSea) {
                objectRenderer.material = playableColor;
            }
            else {
                objectRenderer.material = nonPlayableColor;
            }
        }
    }

    // Update is called once per frame
    void Update() {
        time = Time.time/2;
        perlinNoise = Mathf.PerlinNoise(position.x /7 + time, position.z /7);
        this.gameObject.transform.position = new Vector3(position.x, position.y + (perlinNoise * multiplier), position.z);
    }

    private void OnDisable() {
        GameStateMachine.OnFaseChange -= ChangeFaseHandler;
    }
}
