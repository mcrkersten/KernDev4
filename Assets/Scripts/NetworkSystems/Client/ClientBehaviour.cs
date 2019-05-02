using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class ClientBehaviour : MonoBehaviour
{
    public ServerBehaviour server;
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;

    public int playerID;
    public uint playerIndex;

    public bool Done;
    private bool ServerDisconnect = false;
    private bool WantedDisconnect = false;
    private bool InActiveMatch = false;

    float reconnectTimer = 3;

    void Start() {

        //Connection Parameters
        NetworkConfigParameter parameter = new NetworkConfigParameter {

            maxConnectAttempts = 10,

            //Time between connection attempts in Millies (1 sec)
            connectTimeoutMS = 1000,

            //If we have not heard from the server for the last 10 secs, assume disconnect
            disconnectTimeoutMS = 10000,
        };

        m_Driver = new UdpNetworkDriver(parameter);
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.Parse("127.0.0.1", 9000);
        m_Connection = m_Driver.Connect(endpoint);

        if (!ServerDisconnect) {
            Debug.Log("Connecting");
        }
    }


    public void OnDestroy() {
        m_Driver.Dispose();
    }

    void Update() {
        m_Driver.ScheduleUpdate().Complete();

        HandleConnectionevents();

        if (!m_Connection.IsCreated) {
            if (!Done) {
                if (!InActiveMatch) {
                    Debug.Log("No sever found");
                    CreateServer();
                    InActiveMatch = true;
                }
            }
            //Reconnect
            if (ServerDisconnect) {
                reconnectTimer -= Time.deltaTime;
                if (reconnectTimer < 0) {
                    ReConnect();
                }
            }
            return;
        }
    }

    private void HandleConnectionevents() {
        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) !=
            NetworkEvent.Type.Empty) {

            switch (cmd) {
                case NetworkEvent.Type.Connect:
                    HandleConnectEvent(stream);
                    break;

                case NetworkEvent.Type.Data:
                    HandleDataEvent(stream);
                    break;

                case NetworkEvent.Type.Disconnect:
                    HandleDisconnectEvent(stream);
                    break;
            }
        }
    }

    // On Connection
    void HandleConnectEvent(DataStreamReader stream) {
        Done = !Done;
        InActiveMatch = true;

        //Send some kind of confirmation menu
        var eventType = (uint)ClientToServerEvent.REQUEST_CONNECTION;

        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write(eventType);
            writer.Write((uint)playerID);

            m_Driver.Send(NetworkPipeline.Null, m_Connection, writer);
            }
        }

    // On New data
    void HandleDataEvent(DataStreamReader stream) {
        var readerCtx = default(DataStreamReader.Context);
        ServerToClientEvent eventType = (ServerToClientEvent)stream.ReadUInt(ref readerCtx);

        ServerToClientEvents.ServerEventFunctions[eventType](this, stream, ref readerCtx, m_Connection);
    }

    // On Denied Connection
    public void DeniedConnectionEvent() {
        //TODO HANDLE FULL SERVERS
        Debug.Log("Full game");
    }

    public void AcceptedConnectionEvent() {
        Debug.Log("Connected");
        var eventType = (uint)ClientToServerEvent.REQUEST_PLAYERINDEX;
        using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
            writer.Write(eventType);
            m_Driver.Send(NetworkPipeline.Null, m_Connection, writer);
        }
    }

    // On disconnect
    void HandleDisconnectEvent(DataStreamReader stream) {
        //TODO RECONNECT BUTTON
        m_Connection = default(NetworkConnection);
    }

    private void CreateServer() {
        Debug.Log("Creating Server");
        m_Driver.Dispose();
        server.enabled = true;
        Start();
    }

    private void ReConnect() {
        Debug.Log("Attempt to Reconnect");
        m_Driver.Dispose();
        Start();
    }
}
