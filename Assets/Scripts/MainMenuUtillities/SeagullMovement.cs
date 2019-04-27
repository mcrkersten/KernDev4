using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeagullMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.RotateAround(this.transform.position, Vector3.up, 1f);
    }
}
