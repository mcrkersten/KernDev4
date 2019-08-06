using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class ClientBehaviour : MonoBehaviour
{
    private static ClientBehaviour instance = null;
    public static ClientBehaviour Instance
    {
        get {
            if (instance == null) {
                // This is where the magic happens.
                instance = FindObjectOfType(typeof(ClientBehaviour)) as ClientBehaviour;
            }

            // If it is still null, create a new instance
            if (instance == null) {
                GameObject i = new GameObject("Client");
                i.AddComponent(typeof(ClientBehaviour));
                instance = i.GetComponent<ClientBehaviour>();
            }
            return instance;
        }
    }

    //Server
    public ServerBehaviour localServer;
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;

    //Client information
    public int playerID;
    public uint playerIndex;

    //Server status Booleans
    public bool Done;
    private bool ServerDisconnect = false;
    private bool InActiveMatch = false;

    //Countdown Booleans
    private bool Countdown1Start = false;
    private bool Countdown2Start = false;
    private bool PlacingFaseCountdownStart = false;
    private bool EnemyTurnStarted = false;
    private bool PlayerTurnStarted = false;

    //Turn boolean
    public bool isTurn = false;

    //If game is past placing fase and started
    private bool gameStarted = false;
    public bool isForfeit = false;

    //MENU
    public EndGameMenu gameMenu;

    public GameStateMachine gameStateMachine;
    private float reconnectTimer = 3;

    //Delegates/Callers
    public delegate void StartCountdown(int countdownNumber, int CountdownLenght);
    /// <summary>
    /// Starts a countdown visualization in the game, does not trigger any events when the countdown ends.
    /// Used in OnHover.cs
    /// </summary>
    public static event StartCountdown OnStartCountdown;

    /// <summary>
    /// this object is used to determine the state of the game. It get's used in: 
    /// OnHover.cs, ShipPlacing.cs, ClientBehaviour.cs
    /// </summary>

    private void Start() {
        playerID = int.Parse(UserInformation.Instance.userID);
        FireButton.OnFireCoordinate += Fire;
        gameStateMachine = new GameStateMachine();
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
        FireButton.OnFireCoordinate -= Fire;
        m_Driver.Dispose();
    }

    private void Update() {
        m_Driver.ScheduleUpdate().Complete();
        HandleConnectionevents();

        switch (gameStateMachine.CurrentState) {

            case ProcessFase.SearchingFase:
                ConnectToServer();
                break;

            case ProcessFase.CountDownFase1:
                if (!Countdown1Start) {
                    Countdown1Start = true; // Call it once
                    // Caller to timerHandler
                    OnStartCountdown?.Invoke(0, 5);
                }
                break;

            case ProcessFase.PlacingFase:
                if (!PlacingFaseCountdownStart) {
                    PlacingFaseCountdownStart = true;
                    OnStartCountdown?.Invoke(1, 30);
                }
                break;

            case ProcessFase.CountDownFase2:
                if (!Countdown2Start) {
                    Countdown2Start = true; // Call it once
                    // Caller to timerHandler
                    OnStartCountdown?.Invoke(2, 5);
                }
                break;

            case ProcessFase.PlayerTurn:
                if (!PlayerTurnStarted) {
                    PlayerTurnStarted = true;
                    EnemyTurnStarted = false;
                    OnStartCountdown?.Invoke(3, 30);
                }
                
                break;

            case ProcessFase.EnemyTurn:
                if (!EnemyTurnStarted) {
                    EnemyTurnStarted = true;
                    PlayerTurnStarted = false;
                    OnStartCountdown?.Invoke(4, 30);
                }
                break;

            case ProcessFase.GameFase:
                // Play Game
                gameStarted = true;
                break;

            case ProcessFase.GameEndFase:
                
                break;

            case ProcessFase.ClientPause:
                break;
        }
    }

    // Handle 
    private void HandleConnectionevents() {
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out DataStreamReader stream)) !=
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
    private void HandleConnectEvent(DataStreamReader stream) {
        Done = !Done;
        InActiveMatch = true;

        // Send some kind of confirmation menu
        var eventType = (uint)ClientToServerEvent.REQUEST_CONNECTION;

        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write(eventType);
            writer.Write((uint)playerID);

            m_Driver.Send(NetworkPipeline.Null, m_Connection, writer);
            }
        }

    // On New data
    private void HandleDataEvent(DataStreamReader stream) {
        var readerCtx = default(DataStreamReader.Context);
        ServerToClientEvent eventType = (ServerToClientEvent)stream.ReadUInt(ref readerCtx);
        ServerToClientEvents.ServerEventFunctions[eventType](this, stream, ref readerCtx, m_Connection);
    }

    // On Denied Connection
    public void DeniedConnectionEvent() {
        //TODO HANDLE FULL SERVERS
        Debug.Log("Full game");
    }

    // On Accepted Connection
    public void AcceptedConnectionEvent() {
        Debug.Log("Connected");
        var eventType = (uint)ClientToServerEvent.REQUEST_PLAYERINDEX;
        using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
            writer.Write(eventType);
            m_Driver.Send(NetworkPipeline.Null, m_Connection, writer);
        }
    }

    // On disconnect
    private void HandleDisconnectEvent(DataStreamReader stream) {
        //TODO RECONNECT BUTTON
        m_Connection = default(NetworkConnection);
    }

    //If we disconnected from server, but game is forfeit set bool to true so we dont try to reconnect to server
    public void OnBoolForfeit() {
        isForfeit = true;
    }

    // Try to connect to server
    private void ConnectToServer() {
        if (!m_Connection.IsCreated) {
            if (!Done) {
                if (!InActiveMatch) {
                    Debug.Log("No sever found");
                    CreateServer();
                    InActiveMatch = true;
                }
            }
            //Reconnect
            if (ServerDisconnect && !isForfeit) {
                reconnectTimer -= Time.deltaTime;
                if (reconnectTimer < 0) {
                    ReConnect();
                }
            }
            return;
        }
    }

    // If no server has been found, makes one
    private void CreateServer() {
        Debug.Log("Creating Server");
        m_Driver.Dispose();
        localServer.enabled = true;
        FireButton.OnFireCoordinate -= Fire;
        Start();
    }

    private void Fire(Vector2 coordinate) {
        string stringOfNumbers = "";
        //Convert Vector2 to String
        for (int x = 0; x < 2; x++) {
            int temp = (int)coordinate[x];
            stringOfNumbers += temp.ToString();
        }

        byte[] convertedString = Encoding.ASCII.GetBytes(stringOfNumbers);
        using (var writer = new DataStreamWriter(25, Allocator.Temp)) {
            writer.Write((uint)ClientToServerEvent.RECEIVE_PLAYER_TURNDATA);
            writer.Write((uint)playerID);
            writer.Write(convertedString.Length); //To know lenght of Array on recieving end.
            writer.Write(convertedString, convertedString.Length);
            m_Driver.Send(NetworkPipeline.Null, m_Connection, writer);
        }
    }

    // If connection has been lost
    private void ReConnect() {
        Debug.Log("Attempt to Reconnect");
        m_Driver.Dispose();
        Start();
    }
}
