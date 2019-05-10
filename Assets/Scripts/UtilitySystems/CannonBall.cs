using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public GameObject explosion;
    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Ship")) {
            Instantiate(explosion, transform.position, this.transform.rotation);
        }
    }
}
