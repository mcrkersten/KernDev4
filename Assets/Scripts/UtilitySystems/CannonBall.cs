using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public GameObject explosion;
    public GameObject fire;
    public bool isMissAnimation;

    private void OnTriggerEnter(Collider other) {
        if (!isMissAnimation) {
            Instantiate(explosion, new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), this.transform.rotation, this.gameObject.transform.parent);
            Instantiate(fire, new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z), this.transform.rotation, this.gameObject.transform.parent);
        }
        Destroy(this.gameObject);
    }
}
