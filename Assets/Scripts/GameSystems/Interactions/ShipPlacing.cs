using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OnHover), typeof(BoatSidewaysMovement))]
public class ShipPlacing : MonoBehaviour {


    public delegate void OnShipPlacement();
    public static event OnShipPlacement OnShipPlacing;

    private CoordinateManager coordinateManager;
    private List<Vector2> setCoordinates = new List<Vector2>();
    private BoatSidewaysMovement boatSidewaysMovement;
    private Vector3 screenPoint;
    private OnHover onHover;
    private Vector3 offset;


    private Vector3 startPosistion;
    private Quaternion startRotation;

    public int shipSize;
    private List<Transform> positionBlocks = new List<Transform>();
    private int shipRotation = 0;

    [HideInInspector]
    public bool connected;

    private void Awake() {
        boatSidewaysMovement = this.GetComponent<BoatSidewaysMovement>();
        boatSidewaysMovement.enabled = false;
        startPosistion = this.transform.position;
        startRotation = this.transform.rotation;
        coordinateManager = CoordinateManager.Instance;
        onHover = this.gameObject.GetComponent<OnHover>();
        GameObject block = GameObject.FindGameObjectWithTag("PlayerPositions");
        foreach (Transform child in block.transform) {
            positionBlocks.Add(child);
        }
    }

    public void Update() {

        if(ClientBehaviour.Instance.gameStateMachine.CurrentState == ProcessFase.PlacingFase) {
            if (Input.GetMouseButtonDown(0) && onHover.isHovering && !connected) {
                StartFollowing();
                OnShipPlacing?.Invoke();
                ResetSetCoordinates();
            }
            else if (Input.GetMouseButtonDown(0) && connected) {
                EndFollowing();
                OnShipPlacing?.Invoke();
            }

            if (Input.GetMouseButtonDown(1) && connected) {
                OnShipPlacing?.Invoke();
                ResetBoatToStartPosition();
            }

            if (connected) {
                Follow();
                if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
                    this.gameObject.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x,
                        transform.eulerAngles.y + 90,
                        transform.eulerAngles.z);
                    if (shipRotation < 3) {
                        shipRotation++;
                    }
                    else {
                        shipRotation = 0;
                    }
                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
                    this.gameObject.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x,
                        transform.eulerAngles.y - 90,
                        transform.eulerAngles.z);
                    if (shipRotation > 0) {
                        shipRotation--;
                    }
                    else {
                        shipRotation = 3;
                    }
                }
            }
        }
    }

    private void StartFollowing() {
        connected = true;
        onHover.OnMouseExit();
        boatSidewaysMovement.enabled = false;
    }

    private void EndFollowing() {
        SetToGrid();
    }


    void Follow() {
        Plane plane = new Plane(Vector3.up, new Vector3(0, .45f, 0));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance)) {
            transform.position = ray.GetPoint(distance);
        }
        if (this.transform.position.x >= 0 && this.transform.position.x <= 9) {
            if (this.transform.position.z >= 0 && this.transform.position.z <= 9) {
                Snap();
            }
        }
    }

    void Snap() {
        var currentPos = transform.position;
        transform.position = new Vector3(Mathf.Round(currentPos.x),
                                     currentPos.y,
                                     Mathf.Round(currentPos.z));
    }

    void SetToGrid() {
        if (this.transform.position.x >= 0 && this.transform.position.x <= 9) {
            if (this.transform.position.z >= 0 && this.transform.position.z <= 9) {
                FindClosestPositionBlock();
            }
        }
        else {
            ResetBoatToStartPosition();
        }
    }

    void FindClosestPositionBlock() {
        foreach (Transform t in positionBlocks) {
            float distance = Vector3.Distance(t.position, this.transform.position);
            if (distance < .6f) {
                connected = false;
                this.transform.parent = t;
                UpdateCoordinationManager();
                return;
            }
        }
        ResetBoatToStartPosition();
    }

    void UpdateCoordinationManager() {
        switch (shipRotation) {
            case 0:
                int z = (int)this.transform.position.z + 2;
                for (int zz = shipSize; zz > 0; zz--) {
                    //Check if all positions are free
                    if(coordinateManager.playerTerritory[(int)this.transform.position.x, z - zz] != Coordinate.ship) {
                        coordinateManager.playerTerritory[(int)this.transform.position.x, z - zz] = Coordinate.ship;
                        setCoordinates.Add(new Vector2((int)this.transform.position.x, z - zz));
                    }
                    else {
                        ResetBoatToStartPosition();
                        return;
                    }
                }
                break;

            case 1:
                int x = (int)this.transform.position.x - (shipSize - 2);
                for (int xx = 0; xx < shipSize; xx++) {
                    if(coordinateManager.playerTerritory[x + xx, (int)this.transform.position.z] != Coordinate.ship) {
                        coordinateManager.playerTerritory[x + xx, (int)this.transform.position.z] = Coordinate.ship;
                        setCoordinates.Add(new Vector2(x + xx, (int)this.transform.position.z));
                    }
                    else {
                        ResetBoatToStartPosition();
                        return;
                    }
                }
                break;

            case 2:
                z = (int)this.transform.position.z - 1;
                for (int zz = 0; zz < shipSize; zz++) {
                    if(coordinateManager.playerTerritory[(int)this.transform.position.x, z + zz] != Coordinate.ship) {
                        coordinateManager.playerTerritory[(int)this.transform.position.x, z + zz] = Coordinate.ship;
                        setCoordinates.Add(new Vector2((int)this.transform.position.x, z + zz));
                    }
                    else {
                        ResetBoatToStartPosition();
                        return;
                    }
                }
                break;

            case 3:
                x = (int)this.transform.position.x + (shipSize - 1);
                for (int xx = shipSize; xx > 0; xx--) {
                    if(coordinateManager.playerTerritory[x - xx, (int)this.transform.position.z] != Coordinate.ship) {
                        coordinateManager.playerTerritory[x - xx, (int)this.transform.position.z] = Coordinate.ship;
                        setCoordinates.Add(new Vector2(x - xx, (int)this.transform.position.z));
                    }
                    else {
                        ResetBoatToStartPosition();
                        return;
                    }
                }
                break;
        }
        boatSidewaysMovement.enabled = true;
        PlaceBoat();
    }

    void PlaceBoat() {
        var currentPos = transform.localPosition;
        transform.localPosition = new Vector3(Mathf.Round(currentPos.x),
                                     .4f,
                                     Mathf.Round(currentPos.z));
    }

    private void ResetSetCoordinates() {
        foreach(Vector2 coordinate in setCoordinates) {
            coordinateManager.playerTerritory[(int)coordinate.x, (int)coordinate.y] = Coordinate.water;
        }
        setCoordinates.Clear();
    }

    private void ResetBoatToStartPosition() {
        connected = false;
        ResetSetCoordinates();
        StartCoroutine(ReturnToStartPos(1));
    }

    private IEnumerator ReturnToStartPos(float time) {
        float elapsedTime = 0;
        while (elapsedTime < time) {
            this.transform.position = Vector3.Lerp(this.transform.position, startPosistion, (elapsedTime / time));
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, startRotation, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
