﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Networking.Transport; 
using Unity.Collections;

using UdpCNetworkDriver = Unity.Networking.Transport.UdpNetworkDriver;

public class Server : MonoBehaviour
{
    public UdpCNetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind( NetworkEndPoint.Parse("0.0.0.0", 9000 ) ) != 0)
            Debug.Log("Failed to bind to port ...");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) !=
                NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var readerCtx = default(DataStreamReader.Context);
                    uint number = stream.ReadUInt(ref readerCtx);
                    Debug.Log("Got " + number + " from the Client adding + 2 to it.");
                     number +=2;

                    using (var writer = new DataStreamWriter(4, Allocator.Temp))
                    {
                        writer.Write(number);
                        //other option
                        //m_Connections[i].Send(m_Driver, writer);
                        m_Driver.Send(NetworkPipeline.Null, m_Connections[i], writer);
                    }
                }
            }
        }
    }
}
