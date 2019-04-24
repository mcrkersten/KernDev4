using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Coordinate {
    water = 0,
    ship = 1
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

    //Constructor | Activate new virtual playingfield
    CoordinateManager() {
        Instantiate();
    }

    public Coordinate[,] playerTerritory;
    public Coordinate[,] enemyTerritory;

    public void Instantiate() {
        playerTerritory = new Coordinate[10, 10];
        enemyTerritory = new Coordinate[10, 10];
    }
}
