using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireButton : MonoBehaviour
{
    public delegate void FireOnCoordinate(Vector2 coordinate);
    /// <summary>
    /// Send the Coordinate to the ClientBehaviour.cs
    /// </summary>
    public static event FireOnCoordinate OnFireCoordinate;
    public GameObject child;
    private Collider buttonCollider;

    private void Start() {
        buttonCollider = this.gameObject.GetComponent<Collider>();
        GameStateMachine.OnFaseChange += ChangeFaseHandler;
    }

    private void ChangeFaseHandler(ProcessFase fase) {

        if(fase == ProcessFase.EnemyTurn || fase == ProcessFase.PlayerTurn) {

            switch (fase) {

                case ProcessFase.PlayerTurn:
                    child.SetActive(true);
                    buttonCollider.enabled = true;
                    break;

                case ProcessFase.EnemyTurn:
                    child.SetActive(false);
                    buttonCollider.enabled = true;
                    break;
            }
        }
    }

    private void OnMouseEnter() {
        this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y - .05f, this.gameObject.transform.position.z);
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            OnFireCoordinate?.Invoke(CoordinateManager.Instance.selectedCoordinate);
        }
    }

    private void OnMouseExit() {
        this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + .05f, this.gameObject.transform.position.z);
    }
}
