﻿using System.Net;
using UnityEngine;

using Unity.Networking.Transport;
using Unity.Collections;

using UdpCNetworkDriver = Unity.Networking.Transport.UdpNetworkDriver;

public class ServerBehaviour : MonoBehaviour
{
    public UdpCNetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

        void Start () {
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind(NetworkEndPoint.Parse("0.0.0.0", 9000)) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    void OnDestroy() {
    }

    void Update() {
        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated) {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection)) {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection)) {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated)
                continue;
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) !=
                NetworkEvent.Type.Empty) {
                if (cmd == NetworkEvent.Type.Data) {
                    var readerCtx = default(DataStreamReader.Context);
                    uint number = stream.ReadUInt(ref readerCtx);
                    Debug.Log("Got " + number + " from the Client adding + 2 to it.");
                    number += 2;

                    using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
                        writer.Write(number);
                        m_Driver.Send(NetworkPipeline.Null,m_Connections[i], writer);
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }
}