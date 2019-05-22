using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Coordinate {
    water = 0,
    ship = 1,
    hit = 2,
    miss = 3,
};

public class CoordinateManager
{
    private static CoordinateManager instance = null;
    private static readonly object padlock = new object();
    public static CoordinateManager Instance
    {
        get {
            lock (padlock) {
                if (instance == null) {
                    instance = new CoordinateManager();
                }
                return instance;
            }
        }
    }
    private CoordinateManager coordinateManager;

    //Gtes used in the TerritoryManager.cs to activate animations
    public delegate void FireOnShip(Vector2 coordinate, Coordinate state, bool isEnemyTerritory);
    public static event FireOnShip OnFireOnShip;

    //Constructor | Activate new virtual playingfield
    CoordinateManager() {
        Instantiate();
    }

    public GameObject selectedGameObject;
    public Coordinate[,] playerTerritory;
    public Coordinate[,] enemyTerritory;
    public Vector2 selectedCoordinate;

    public void Instantiate() {
        playerTerritory = new Coordinate[10, 10];
        enemyTerritory = new Coordinate[10, 10];
    }

    public void UpdatePlayerTerritory(Vector2 coordinate, Coordinate state) {
        playerTerritory[(int)coordinate.x,(int)coordinate.y] = state;
        OnFireOnShip?.Invoke(coordinate, state, false);
    }

    public void UpdateEnemyTerritory(Vector2 coordinate, Coordinate state) {
        enemyTerritory[(int)coordinate.x, (int)coordinate.y] = state;
        OnFireOnShip?.Invoke(coordinate, state, true);
    }
}
