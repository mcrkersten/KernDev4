using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class ClientToServerEvents {
    public delegate void PacketFunction(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source);

    public static Dictionary<ClientToServerEvent, PacketFunction> ServerEventFunctions = new Dictionary<ClientToServerEvent, PacketFunction>()
    {
        { ClientToServerEvent.REQUEST_CONNECTION, RequestConnection },
        { ClientToServerEvent.REQUEST_PLAYERINDEX, RequestPlayerIndex },
        { ClientToServerEvent.RECEIVE_SHIP_COORDINATES, ReceiveShipCoordinates},
        { ClientToServerEvent.RECEIVE_PLAYER_TURNDATA, PlayerTurnData },
        { ClientToServerEvent.RECIEVE_NULL_PLAYER_TURNDATA, PlayerNullTurnData },
        { ClientToServerEvent.PING_TO_SERVER, PingToServer },
    };


    public static void RequestConnection(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ServerBehaviour server = caller as ServerBehaviour;
        Debug.Log("[Server] New Connect request");
        uint playerID = stream.ReadUInt(ref context);
        uint canConnect = server.CheckConnection(source, playerID);

        if (canConnect == 1) {
            uint eventType = (uint)ServerToClientEvent.ACCEPT_CONNECTION_REQUEST;
            using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
                writer.Write(eventType);
                source.Send(server.m_Driver, writer);
            }
        }
        else {
            uint eventType = (uint)ServerToClientEvent.DENY_CONNECTION_REQUEST;
            using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
                writer.Write(eventType);
                source.Send(server.m_Driver, writer);
            }
        }
    }

    public static void RequestPlayerIndex(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        //bind this connection to a specific player index
        ServerBehaviour server = caller as ServerBehaviour;

        //Server handles request and sends result to client
        uint eventType = (uint)ServerToClientEvent.ASSIGN_PLAYERINDEX;
        uint index = server.AddNextAvailablePlayerIndex(source);
        Debug.Log("[Server] Sending player index " + index + " to client");
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write(eventType);
            writer.Write(index);
            source.Send(server.m_Driver, writer);
        }

        //Server Sends Message to all connected clients
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ServerToClientEvent.PLAYER_JOINED);
            writer.Write(index);
            server.BroadcastToClientsExcluding(source, writer);
        }

        //Send all players to newly connected player.
        server.SendExistingPlayersTo(source);
    }

    public static void ReceiveShipCoordinates(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ServerBehaviour server = caller as ServerBehaviour;
        uint playerID = stream.ReadUInt(ref context);
        int stringLength = stream.ReadInt(ref context);
        byte[] convertedString = stream.ReadBytesAsArray(ref context, stringLength);

        server.SetPlayerCoordinates(convertedString, playerID);
    }

    public static void PlayerTurnData(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ServerBehaviour server = caller as ServerBehaviour;
        uint PlayerID = stream.ReadUInt(ref context);
        int stringLength = stream.ReadInt(ref context);
        byte[] convertedString = stream.ReadBytesAsArray(ref context, stringLength);

        //Test if coordinate is fired on a ship or is a miss and send result to all clients. 
        server.FireOnPlayerCoordinate(convertedString, PlayerID, source);
    }

    public static void PlayerNullTurnData(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ServerBehaviour server = caller as ServerBehaviour;
        uint PlayerID = stream.ReadUInt(ref context);
    }

    public static void PingToServer(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {

    }
}
