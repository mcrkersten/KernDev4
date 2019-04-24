using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OnHover))]
public class ShipPlacing : MonoBehaviour {
	private Vector3 screenPoint;
    private OnHover onHover;
    private Vector3 offset;
    public Vector2 shipSize;

    [HideInInspector]
    public bool connected;

    private void Start() {
        onHover = this.gameObject.GetComponent<OnHover>();
    }

    public void Update() {
        if (Input.GetMouseButtonDown(0) && onHover.isHovering && !connected) {
            StartFollowing();
        }
        else if(Input.GetMouseButtonDown(0) && connected) {
            EndFollowing();
        }

        if (connected) {
            Follow();
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
                this.gameObject.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x,
                    transform.eulerAngles.y + 90,
                    transform.eulerAngles.z);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
                this.gameObject.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x,
                    transform.eulerAngles.y - 90,
                    transform.eulerAngles.z);
            }
        }
    }

    private void StartFollowing() {
        connected = true;
        onHover.OnMouseExit();
    }

    private void EndFollowing() {
        connected = false;
    }


    void Follow() {
        Plane plane = new Plane(Vector3.up, new Vector3(0, .35f, 0));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance)) {
            transform.position = ray.GetPoint(distance);
        }
        Snap();
    }

    void Snap() {
        var currentPos = transform.position;
        transform.position = new Vector3(Mathf.Round(currentPos.x),
                                     currentPos.y,
                                     Mathf.Round(currentPos.z));
    }
}
