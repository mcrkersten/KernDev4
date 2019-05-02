using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public enum ClientToServerEvent {
    REQUEST_CONNECTION = 0,
    REQUEST_PLAYERINDEX,
    PING_TO_SERVER,
}

public enum ServerToClientEvent {
    PLAYER_JOINED = 0,
    ASSIGN_PLAYERINDEX,
    DENY_CONNECTION_REQUEST,
    ACCEPT_CONNECTION_REQUEST,
    START_BOATPLACEMENT,
    END_BOATPLACEMENT,
    PING_TO_CLIENT,
}