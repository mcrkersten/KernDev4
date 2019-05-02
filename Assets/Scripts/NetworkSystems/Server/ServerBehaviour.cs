﻿using System.Net;
using UnityEngine;

using Unity.Networking.Transport;
using Unity.Collections;
using System.Collections.Generic;
using System.Collections;

using UdpCNetworkDriver = Unity.Networking.Transport.UdpNetworkDriver;

public class ServerBehaviour : MonoBehaviour
{
    private float networkTime;
    private uint timeInSeconds = 60;


    public UdpCNetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;
    private List<uint> connectedPlayerIDs = new List<uint>();

    private Dictionary<uint, PlayerData> playerData = new Dictionary<uint, PlayerData>();
    private Dictionary<uint, NetworkConnection> playerIndices = new Dictionary<uint, NetworkConnection>();

    void Start () {
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind(NetworkEndPoint.Parse("127.0.0.1", 9000)) != 0)
            Debug.Log("[Server] | Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        StartCoroutine(Clock());
    }

    void OnDestroy() {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void Update() {
        m_Driver.ScheduleUpdate().Complete();

        CleanUpConnections();
        HandleNewConnections();
        HandleConnectionEvents();
    }

    #region Networking
    // Handle events
    public void HandleConnectionEvents() {
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) !=
                NetworkEvent.Type.Empty) {
                //handle event
                switch (cmd) {
                    case NetworkEvent.Type.Data:
                        HandleData(stream, i);
                        break;

                    case NetworkEvent.Type.Disconnect:
                        OnDisconnect(i);
                        break;
                }
            }
        }
    }

    // Clean up connections
    public void CleanUpConnections() {
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated) {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }

    // Accept new connections
    public void HandleNewConnections() {
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection)) {
            m_Connections.Add(c);
            Debug.Log("[Server] | New Connection");
        }
    }

    // Handle a client disconnect
    public void OnDisconnect(int connection) {
        Debug.Log("[Server] | Client disconnected from server");

        m_Connections[connection] = default(NetworkConnection);
    }

    // Handle new DataInput
    void HandleData(DataStreamReader stream, int connectionIndex) {
        var readerCtx = default(DataStreamReader.Context);

        //Get Data Type (what kind of update is this?)
        ClientToServerEvent eventType = (ClientToServerEvent)stream.ReadUInt(ref readerCtx);
        ClientToServerEvents.ServerEventFunctions[eventType](this, stream, ref readerCtx, m_Connections[connectionIndex]);
    }

    #region Functions for new connection

    // Define available player index
    internal uint AddNextAvailablePlayerIndex(NetworkConnection c) {
        uint i = 0;
        while (playerIndices.ContainsKey(i)) i++;
        playerIndices[i] = c;

        //initialize PlayerData for this player
        playerData[i] = new PlayerData(i);

        return i;
    }

    // Broadcast to all clients, excluding source client
    internal void BroadcastToClientsExcluding(NetworkConnection source, DataStreamWriter writer) {
        for (int i = 0; i < m_Connections.Length; ++i) {
            //skip broadcast for source client
            if (m_Connections[i] == source)
                continue;

            m_Connections[i].Send(m_Driver, writer);
        }
    }

    // Send all information about connected players to Source client
    internal void SendExistingPlayersTo(NetworkConnection source) {
        for (int i = 0; i < m_Connections.Length; ++i) {
            //skip message for source client
            if (m_Connections[i] == source)
                continue;

            int index = GetIndexForConnection(m_Connections[i]);
            if (index != -1) {
                using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
                    writer.Write((uint)ServerToClientEvent.PLAYER_JOINED);
                    writer.Write((uint)index);
                    source.Send(m_Driver, writer);
                }
            }
        }
    }

    internal uint CheckConnection(NetworkConnection c, uint playerID) {
        uint xx = 0;
        //Check if connection was made before.
        if (connectedPlayerIDs.Contains(playerID)) {
            Debug.Log("[Sever] Reconnected player " + +playerID);
            return 1;
        }
        //If there are less than 2 connections.
        if(connectedPlayerIDs.Count < 2) {
            Debug.Log("[Server] New player " + playerID);
            connectedPlayerIDs.Add(playerID);
            xx = 1;
            return xx;
        }

        //Deny conenction
        xx = 0;
        return xx;
    }

    //return -1 if not found
    int GetIndexForConnection(NetworkConnection c) {
        foreach (KeyValuePair<uint, NetworkConnection> pair in playerIndices) {
            if (c == pair.Value) {
                return (int)pair.Key;
            }
        }
        return -1;
    }
    #endregion

    IEnumerator Clock() {
        while (true) {
            networkTime = Time.time;

            if (Time.frameCount % 60 == 0) {
                //send PING event to all clients
                for (int i = 0; i < m_Connections.Length; ++i) {
                    using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
                        writer.Write((uint)ServerToClientEvent.PING_TO_CLIENT);
                        writer.Write(networkTime);
                        m_Driver.Send(NetworkPipeline.Null, m_Connections[i], writer);
                    }
                }
            }
            yield return null;
        }
    }
    #endregion

    #region GameLogic

    private void StartBoatPlacing() {
        //Server Sends Message to all connected clients
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ServerToClientEvent.START_BOATPLACEMENT);
            writer.Write(timeInSeconds);
            BroadcastToAllClients(writer);
        }
    }

    #endregion

    // Broadcast to all clients, excluding source client
    internal void BroadcastToAllClients(DataStreamWriter writer) {
        for (int i = 0; i < m_Connections.Length; ++i) {
            m_Connections[i].Send(m_Driver, writer);
        }
    }
}

public class PlayerData {
    public uint playerID;
    public Coordinate[,] territory;


    public PlayerData(uint playerID) {
        this.playerID = playerID;
    }

    void OnFireRequest() {
        Debug.Log("[Server] Got Fire Request");
    }
}