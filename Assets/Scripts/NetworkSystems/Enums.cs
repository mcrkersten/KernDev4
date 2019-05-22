using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public enum ClientToServerEvent {
    REQUEST_CONNECTION = 0,
    REQUEST_PLAYERINDEX,
    RECEIVE_SHIP_COORDINATES,
    RECEIVE_PLAYER_TURNDATA,
    RECIEVE_NULL_PLAYER_TURNDATA,
    PING_TO_SERVER,
}

public enum ServerToClientEvent {
    PLAYER_JOINED = 0,
    ASSIGN_PLAYERINDEX,
    DENY_CONNECTION_REQUEST,
    ACCEPT_CONNECTION_REQUEST,
    CHANGE_GAMESTATE,
    REQUEST_SHIPCOORDINATES,

    REQUEST_TURNCOORDINATE,

    FIRE_ENEMY,
    FIRE_PLAYER,

    FORFEIT,

    WIN,
    LOST,

    PING_TO_CLIENT,
}