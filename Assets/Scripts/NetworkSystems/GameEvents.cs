using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

//TODO: Network Time sync events?
public enum GameEvent {
    REQUEST_PLAYERINDEX = 0,    //(client -> server), DONE
    ASSIGN_PLAYERINDEX,         //playerIndex (server -> client), DONE
    PLAYER_JOINED,              //playerIndex (server -> client), DONE
    PLAYER_RECONNECT,
    PING_TO_CLIENT,
    PING_TO_SERVER,
}

//Class that contains all the functions that parse game event packets
//Note that these events may be generated elsewhere (see above list for send directions)
public static class GameEvents {

    public delegate void PacketFunction(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source);

    public static Dictionary<GameEvent, PacketFunction> EventFunctions = new Dictionary<GameEvent, PacketFunction>()
    {
        { GameEvent.REQUEST_PLAYERINDEX, RequestPlayerIndex },
        { GameEvent.ASSIGN_PLAYERINDEX, AssignPlayerIndex },
        { GameEvent.PLAYER_JOINED, PlayerJoined },
        { GameEvent.PLAYER_RECONNECT, PlayerReconnect },
        { GameEvent.PING_TO_CLIENT, PingToClient },
        { GameEvent.PING_TO_SERVER, PingToServer},
    };


    public static void RequestPlayerIndex(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        //bind this connection to a specific player index
        ServerBehaviour server = caller as ServerBehaviour;
        uint eventType = (uint)GameEvent.ASSIGN_PLAYERINDEX;
        uint index = server.AddNextAvailablePlayerIndex(source);
        Debug.Log("[Game Event] Sending player index " + index + " to client");

        //Tell source client what their player index is
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write(eventType);
            writer.Write(index);
            source.Send(server.m_Driver, writer);
        }

        #region Server
        //Send message to all other clients about newly connected player
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)GameEvent.PLAYER_JOINED);
            writer.Write(index);
            server.BroadcastToClientsExcluding(source, writer);
        }

        //Send message per existing client to newly connected player
        server.SendExistingPlayersTo(source);
        #endregion
    }

    public static void AssignPlayerIndex(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        //got player index from server
        ClientBehaviour client = caller as ClientBehaviour;

        uint index = stream.ReadUInt(ref context);

        Debug.Log("[Game Event] Assigned player index " + index + " to my client instance");

        //TODO: Make this a function?
        client.playerIndex = index;
        client.name = "Client" + index;
    }

    public static void PlayerJoined(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        //got remote player joined from server
        ClientBehaviour client = caller as ClientBehaviour;
        uint index = stream.ReadUInt(ref context);
        Debug.Log("Player " + index + 1 + " connected");
    }

    public static void PlayerReconnect(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {

    }

    public static void PingToClient(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {

    }

    public static void PingToServer(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {

    }
}