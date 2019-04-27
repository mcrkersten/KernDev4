using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class ClientBehaviour : MonoBehaviour
{
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;
    public bool ServerDisconnect;
    float timer = 3f;

    void Start() {
        m_Driver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.Parse("127.0.0.1", 9000);
        m_Connection = m_Driver.Connect(endpoint);
    }
    public void OnDestroy() {
        m_Driver.Dispose();
    }

    void Update() {
        timer -= Time.deltaTime;
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated) {
            if (!Done)
                Debug.Log("Error during connection");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) !=
            NetworkEvent.Type.Empty) {
            if (cmd == NetworkEvent.Type.Connect) {
                Debug.Log("Connected to server");

                var value = 1;
                using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
                    writer.Write(value);
                    m_Connection.Send(m_Driver, writer);
                }
            }
            else if (cmd == NetworkEvent.Type.Data) {
                var readerCtx = default(DataStreamReader.Context);
                uint value = stream.ReadUInt(ref readerCtx);
                Debug.Log("Got the value = " + value + " back");
                Done = true;

            }
            //Disconnect from ServerSide;
            else if (cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Disconnected");
                m_Connection = default(NetworkConnection);
            }
        }

        //Disconnect from Server with timer;
        if (timer < 0) {
            m_Connection.Disconnect(m_Driver);
            Debug.Log("Disconnected");
            m_Connection = default(NetworkConnection);
        }
    }
}
