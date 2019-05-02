﻿using System.Collections;
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
    }

    public static void PingToServer(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {

    }
}