using System.Net;
using System;
using UnityEngine;
using System.Text;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Collections.Generic;
using System.Collections;

using UdpCNetworkDriver = Unity.Networking.Transport.UdpNetworkDriver;

public class ServerBehaviour : MonoBehaviour
{
    private float networkTime;
    /// <summary>
    /// starts the game by changing the Gamestate if there are 2 accepted connections/players.
    /// </summary>
    private bool allPlayersConnected = false;
    private bool countdown1_started = false;
    private bool countdown2_started = false;
    private bool placingFase_started = false;
    private bool gameFase_started = false;
    private uint currentTurnIndex;

    public UdpCNetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;
    private List<uint> connectedPlayerIDs = new List<uint>();
    private GameStateMachine gameStateMachine;
    private List<PlayerData> playerData = new List<PlayerData>();
    private Dictionary<uint, NetworkConnection> playerIndices = new Dictionary<uint, NetworkConnection>();

    //PingStuff
    float pingTime = 1;
    float currentPingTime;


    private void Start () {
        pingTime = currentPingTime;
        gameStateMachine = new GameStateMachine();
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind(NetworkEndPoint.Parse("127.0.0.1", 9000)) != 0)
            Debug.Log("[Server] | Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        StartCoroutine(Clock());
    }

    private void OnDestroy() {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    private void Update() {
        m_Driver.ScheduleUpdate().Complete();
        CleanUpConnections();
        HandleNewConnections();
        HandleConnectionEvents();
        StateMachine();
        PingAllConnections();
    }

    // Statemachine of the server
    private void StateMachine() {

        if (connectedPlayerIDs.Count == 2) {
            allPlayersConnected = true;
        }

        if (allPlayersConnected == true && gameStateMachine.CurrentState == ProcessFase.SearchingFase) {
            ChangeAllClientGameState(Command.FoundPlayers);
        }

        if (gameStateMachine.CurrentState == ProcessFase.CountDownFase1 && !countdown1_started) {
            countdown1_started = true;
            StartCoroutine(CountDown(6, Command.StartPlacingFase));
        }

        if (gameStateMachine.CurrentState == ProcessFase.PlacingFase && !placingFase_started) {
            placingFase_started = true;
            StartCoroutine(CountDown(31, Command.StartGameFaseCountdown));
        }

        if (gameStateMachine.CurrentState == ProcessFase.CountDownFase2 && !countdown2_started) {
            countdown2_started = true;
            StartCoroutine(CountDown(6, Command.StartGameFase));
        }

        if (gameStateMachine.CurrentState == ProcessFase.GameFase && !gameFase_started) {
            gameFase_started = true;
            NextTurn();
        }

        if (gameStateMachine.CurrentState == ProcessFase.GameEndFase) {

        }
    }

    // Handle events
    public void HandleConnectionEvents() {
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out DataStreamReader stream)) !=
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
    private void HandleData(DataStreamReader stream, int connectionIndex) {
        var readerCtx = default(DataStreamReader.Context);

        //Get Data Type (what kind of update is this?)
        ClientToServerEvent eventType = (ClientToServerEvent)stream.ReadUInt(ref readerCtx);
        ClientToServerEvents.ServerEventFunctions[eventType](this, stream, ref readerCtx, m_Connections[connectionIndex]);
    }

    // Change the gamestate machine off all connected clients and server
    private void ChangeAllClientGameState(Command command) {
        //Server Sends Message to all connected clients
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ServerToClientEvent.CHANGE_GAMESTATE);
            writer.Write((uint)command);
            BroadcastToAllClients(writer);
        }
        gameStateMachine.ChangeFase(command);
    }

    // Change gamestate of a single client
    private void ChangeSingleClientGameState(Command command, int clientIndex) {
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ServerToClientEvent.CHANGE_GAMESTATE);
            writer.Write((uint)command);
            m_Connections[clientIndex].Send(m_Driver, writer);
        }
    }

    // Broadcast to all clients
    internal void BroadcastToAllClients(DataStreamWriter writer) {
        for (int i = 0; i < m_Connections.Length; ++i) {
            m_Connections[i].Send(m_Driver, writer);
        }
    }

    // Handle recieved playerShip-coordinates
    public void SetPlayerCoordinates(byte[] bytes, uint id) {
        char[] receivedString = Conversions.BytesToCharArray(bytes);
        Coordinate[,] newCoordinates = new Coordinate[10,10];

        int shipParts = 0;
        int parsedInt;
        int xxx = 0;

        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 10; y++) {
                //Convert string[index] to int
                parsedInt = (int)char.GetNumericValue(receivedString[xxx]);
                //To know how many ship-blocks have been placed.
                if (parsedInt == 1) {
                    shipParts++;
                }

                newCoordinates[x, y] = (Coordinate)parsedInt;
                xxx++;
            }
        }
        
        foreach (PlayerData data in playerData) {
            if (data.playerID == id) {
                if (shipParts < 0) {
                    //Send forfeit message to all players.
                    using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
                        writer.Write((uint)ServerToClientEvent.FORFEIT);
                        BroadcastToAllClients(writer);
                    }
                    return;
                }
                data.territory = newCoordinates;
            }
        }
    }

    // Handle recieved playerFire-coordinate
    public void FireOnPlayerCoordinate(byte[] bytes, uint id, NetworkConnection fireringClient) {
        Coordinate hit = Coordinate.ship;
        char[] receivedString = Conversions.BytesToCharArray(bytes);
        PlayerData enemy = null;

        foreach (PlayerData data in playerData) {
            if (data.playerID == id) {
                continue;
            }
            else {
                if (data.territory[(int)char.GetNumericValue(receivedString[0]), (int)char.GetNumericValue(receivedString[1])] == Coordinate.ship) {
                    enemy = data;
                    data.territory[(int)char.GetNumericValue(receivedString[0]), (int)char.GetNumericValue(receivedString[1])] = Coordinate.hit;
                    hit = Coordinate.hit;
                    data.destroyedTargets += 1;
                }
                else {
                    data.territory[(int)char.GetNumericValue(receivedString[0]), (int)char.GetNumericValue(receivedString[1])] = Coordinate.miss;
                    hit = Coordinate.miss;
                }
            }
        }

        foreach (PlayerData data in playerData) {
            if (data.playerID == id) {
                using (var writer = new DataStreamWriter(16, Allocator.Temp)) {
                    writer.Write((uint)ServerToClientEvent.FIRE_PLAYER);
                    writer.Write((uint)hit);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                    fireringClient.Send(m_Driver, writer);
                }
            }
            else {
                using (var writer = new DataStreamWriter(16, Allocator.Temp)) {
                    writer.Write((uint)ServerToClientEvent.FIRE_ENEMY);
                    writer.Write((uint)hit);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                    BroadcastToClientsExcluding(fireringClient, writer);
                }
            }
        }

        if (enemy != null && enemy.destroyedTargets >= 13) {
            EndGame(enemy);
            return;
        }
        Debug.Log("Turn");
        NextTurn();
    }

    // Define available player index
    internal uint AddNextAvailablePlayerIndex(NetworkConnection c) {
        uint i = 0;
        while (playerIndices.ContainsKey(i)) i++;
        playerIndices[i] = c;

        return i;
    }

    // Broadcast new connection to all clients, excluding the new client
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

    // Check the connection
    internal uint CheckConnection(NetworkConnection c, uint playerID) {
        uint xx = 0;
        //Check if connection was made before.
        if (connectedPlayerIDs.Contains(playerID)) {
            Debug.Log("[Sever] Reconnected player " + playerID);
            xx = 1;
            return xx;
        }
        //If there are less than 2 connections.
        if(connectedPlayerIDs.Count < 2) {
            Debug.Log("[Server] New player " + playerID);
            connectedPlayerIDs.Add(playerID);
            xx = 1;

            //initialize PlayerData for this player
            playerData.Add(new PlayerData(playerID, c));
            return xx;
        }

        //Deny conenction
        xx = 0;
        return xx;
    }

    //return -1 if not found
    private int GetIndexForConnection(NetworkConnection c) {
        foreach (KeyValuePair<uint, NetworkConnection> pair in playerIndices) {
            if (c == pair.Value) {
                return (int)pair.Key;
            }
        }
        return -1;
    }

    private IEnumerator Clock() {
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

    private IEnumerator CountDown(int time, Command command)
    {
        float normalizedTime = time;
        while (normalizedTime >= 0) {
            normalizedTime -= Time.deltaTime;
            yield return null;
        }
        ChangeAllClientGameState(command);
        if(command == Command.StartGameFaseCountdown) {
            using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
                writer.Write((uint)ServerToClientEvent.REQUEST_SHIPCOORDINATES);
                BroadcastToAllClients(writer);
            }
        }
    }

    private void NextTurn() {
        currentTurnIndex = ++currentTurnIndex % (uint)playerData.Count;
        for (int x = 0; x < playerData.Count; x++) {
            if (x == currentTurnIndex) {
                ChangeSingleClientGameState(Command.ChangeTurnEnemy, x);
            }
            else {
                ChangeSingleClientGameState(Command.ChangeTurnPlayer, x);
            }
        }
    }

    private void EndGame(PlayerData playerThatLoses) {
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ServerToClientEvent.LOSS);
            playerThatLoses.connection.Send(m_Driver, writer);
        }
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ServerToClientEvent.WIN);
            BroadcastToClientsExcluding(playerThatLoses.connection, writer);
        }
    }

    private void PingAllConnections() {
        currentPingTime -= Time.deltaTime;
        if(currentPingTime < 0) {
            currentPingTime = pingTime;

            using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
                writer.Write((uint)ServerToClientEvent.PING_TO_CLIENT);
                BroadcastToAllClients(writer);
            }
        }
    }
}

public class PlayerData {
    public NetworkConnection connection;
    public uint playerID;
    public Coordinate[,] territory;
    public int destroyedTargets = 0;


    public PlayerData(uint playerID, NetworkConnection connection) {
        this.playerID = playerID;
        this.connection = connection;
    }
}
