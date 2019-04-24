using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatSidewaysMovement : MonoBehaviour
{
    private Vector3 position;
    private float multiplier = 7;
    private Vector3 rotation;
    private float time;

    private void Start() {
        position = this.gameObject.transform.position;
        rotation = this.gameObject.transform.eulerAngles;
    }
    // Update is called once per frame
    void Update()
    {
        time = Time.time / 2;
        float perlinNoise1 = Mathf.PerlinNoise(position.x + 1/ 25 + time, position.z / 25);
        float perlinNoise2 = Mathf.PerlinNoise(position.x / 25 + time, position.z / 25);
        float rotationForShipX = perlinNoise1 - perlinNoise2;

        float perlinNoise3 = Mathf.PerlinNoise(position.x/ 25 + time, position.z + 1 / 25);
        float perlinNoise4 = Mathf.PerlinNoise(position.x / 25 + time, position.z / 25);
        float rotationForShipZ = perlinNoise3 - perlinNoise4;

        this.gameObject.transform.localEulerAngles = new Vector3(rotation.x + (rotationForShipX * multiplier), rotation.y, rotation.z + (rotationForShipZ * multiplier));

    }

    float GetRandomBetweenValue(float min, float max) {
        return Random.Range(min, max);
    }
}
