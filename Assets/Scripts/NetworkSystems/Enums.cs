using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public enum ClientToServerEvent {
    REQUEST_CONNECTION = 0,
    REQUEST_PLAYERINDEX,
    RECEIVE_SHIP_COORDINATES,
    PING_TO_SERVER,
}

public enum ServerToClientEvent {
    PLAYER_JOINED = 0,
    ASSIGN_PLAYERINDEX,
    DENY_CONNECTION_REQUEST,
    ACCEPT_CONNECTION_REQUEST,
    CHANGE_GAMESTATE,
    REQUEST_SHIPCOORDINATES,
    FORFEIT,
    WIN,
    LOST,

    PING_TO_CLIENT,
}