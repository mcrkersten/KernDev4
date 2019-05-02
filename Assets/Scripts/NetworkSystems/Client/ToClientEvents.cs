using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class ServerToClientEvents {
    public delegate void PacketFunction(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source);

    public static Dictionary<ServerToClientEvent, PacketFunction> ServerEventFunctions = new Dictionary<ServerToClientEvent, PacketFunction>()
    {
        { ServerToClientEvent.ACCEPT_CONNECTION_REQUEST, AcceptConnectionRequest },
        { ServerToClientEvent.DENY_CONNECTION_REQUEST,  DenyConnectionRequest},
        { ServerToClientEvent.PLAYER_JOINED, PlayerJoined },
        { ServerToClientEvent.ASSIGN_PLAYERINDEX, AssignPlayerIndex},
        { ServerToClientEvent.START_BOATPLACEMENT, StartBoatPlacement },
        { ServerToClientEvent.PING_TO_CLIENT, PingToClient },
    };


    public static void AcceptConnectionRequest(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        client.AcceptedConnectionEvent();
    }

    public static void DenyConnectionRequest(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        client.m_Connection.Disconnect(client.m_Driver);
    }

    //SEND NEW PLAYER TO ALL CLIENTS
    public static void PlayerJoined(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        //got remote player joined from server
        ClientBehaviour client = caller as ClientBehaviour;
        uint index = stream.ReadUInt(ref context);
        Debug.Log("Player " + index + 1 + " connected");
    }

    //SEND INDEX TO CLIENT
    public static void AssignPlayerIndex(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        //got player index from server
        ClientBehaviour client = caller as ClientBehaviour;
        uint index = stream.ReadUInt(ref context);
        Debug.Log("[Server] Assigned player index " + index + " to my client instance");

        client.playerIndex = index;
        client.name = "Client" + index;
    }

    public static void StartBoatPlacement(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {

    }

    public static void PingToClient(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {

    }
}
