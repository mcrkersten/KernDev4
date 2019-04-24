using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveGenerator : MonoBehaviour {
    private float perlinNoise;
    private float multiplier = .5f;
    private float time;
    private Vector3 position;

    private void Awake() {
        position = this.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update() {
        time = Time.time/2;
        perlinNoise = Mathf.PerlinNoise(position.x /7 + time, position.z /7);
        this.gameObject.transform.position = new Vector3(position.x, position.y + (perlinNoise * multiplier), position.z);
    }
}
