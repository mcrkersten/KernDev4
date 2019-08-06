using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class ServerToClientEvents {
    public delegate void PacketFunction(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source);

    public static Dictionary<ServerToClientEvent, PacketFunction> ServerEventFunctions = new Dictionary<ServerToClientEvent, PacketFunction>()
    {
        { ServerToClientEvent.ACCEPT_CONNECTION_REQUEST, AcceptConnectionRequest },
        { ServerToClientEvent.DENY_CONNECTION_REQUEST,  DenyConnectionRequest },
        { ServerToClientEvent.PLAYER_JOINED, PlayerJoined },
        { ServerToClientEvent.ASSIGN_PLAYERINDEX, AssignPlayerIndex },
        { ServerToClientEvent.CHANGE_GAMESTATE, ChangeGameState },
        { ServerToClientEvent.REQUEST_SHIPCOORDINATES, RequestShipCoordinates },
        { ServerToClientEvent.REQUEST_TURNCOORDINATE, RequestTurnCoordinate },
        { ServerToClientEvent.FIRE_ENEMY, EnemyFire },
        { ServerToClientEvent.FIRE_PLAYER, PlayerFire },
        { ServerToClientEvent.FORFEIT, Forfeit },
        { ServerToClientEvent.WIN, Win },
        { ServerToClientEvent.LOSS, Loss },
        { ServerToClientEvent.PING_TO_CLIENT, PingToClient },
    };


    public static void AcceptConnectionRequest(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        client.AcceptedConnectionEvent();
    }

    public static void DenyConnectionRequest(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        client.m_Connection.Disconnect(client.m_Driver);
    }

    //SEND NEW PLAYER TO ALL CLIENTS
    public static void PlayerJoined(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        //got remote player joined from server
        ClientBehaviour client = caller as ClientBehaviour;
        uint index = stream.ReadUInt(ref context);
        Debug.Log("Player " + (index + 1) + " connected");
    }

    //SEND INDEX TO CLIENT
    public static void AssignPlayerIndex(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        //got player index from server
        ClientBehaviour client = caller as ClientBehaviour;
        uint index = stream.ReadUInt(ref context);
        Debug.Log("[Server] Assigned player index " + index + " to my client instance");

        client.playerIndex = index;
        client.name = "Client" + index;
    }

    public static void ChangeGameState(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        uint index = stream.ReadUInt(ref context);
        client.gameStateMachine.ChangeFase((Command)index);
    }

    public static void RequestShipCoordinates(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        Coordinate[,] coordinates = CoordinateManager.Instance.playerTerritory;

        //Convert Enums to string of bytes per row.
        string stringOfNumbers = "";

        for(int x = 0; x < 10; x++) {
            for(int y = 0; y < 10; y++) {
                int temp = (int)coordinates[x, y];
                stringOfNumbers += temp.ToString();
            }
        }

        byte[] convertedString = Encoding.ASCII.GetBytes(stringOfNumbers);

        using (var writer = new DataStreamWriter(150, Allocator.Temp)) {

            writer.Write((uint)ClientToServerEvent.RECEIVE_SHIP_COORDINATES);
            writer.Write((uint)client.playerID);
            writer.Write(convertedString.Length); //To know lenght of Array on other end.
            writer.Write(convertedString, convertedString.Length);
            client.m_Driver.Send(NetworkPipeline.Null, client.m_Connection, writer);
        }
    }

    public static void Forfeit(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        client.OnBoolForfeit();
        client.gameStateMachine.ChangeFase(Command.EndGame);
        client.m_Connection.Disconnect(client.m_Driver);
    }

    public static void RequestTurnCoordinate(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        string stringOfNumbers = "";

        Vector2 coordinate = CoordinateManager.Instance.selectedCoordinate;

        if(coordinate != null) {
            //Convert Vector2 to String
            for (int x = 0; x < 1; x++) {
                int temp = (int)coordinate[x];
                stringOfNumbers += temp.ToString();
            }

            byte[] convertedString = Encoding.ASCII.GetBytes(stringOfNumbers);
            using (var writer = new DataStreamWriter(25, Allocator.Temp)) {
                writer.Write((uint)ClientToServerEvent.RECEIVE_PLAYER_TURNDATA);
                writer.Write((uint)client.playerID);
                writer.Write(convertedString.Length); //To know lenght of Array on recieving end.
                writer.Write(convertedString, convertedString.Length);
                client.m_Driver.Send(NetworkPipeline.Null, client.m_Connection, writer);
            }
        }
        else {
            using (var writer = new DataStreamWriter(16, Allocator.Temp)) {
                writer.Write((uint)ClientToServerEvent.RECIEVE_NULL_PLAYER_TURNDATA);
                writer.Write((uint)client.playerID);
                client.m_Driver.Send(NetworkPipeline.Null, client.m_Connection, writer);
            }
        }
    }

    //Update Enemy Teritory
    public static void EnemyFire(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        Coordinate state = (Coordinate)stream.ReadUInt(ref context);
        int streamLenght = stream.ReadInt(ref context);
        byte[] bytes = stream.ReadBytesAsArray(ref context, streamLenght);

        //Convert recieved ByteArray to Vector2
        char[] convertedBytes = Conversions.BytesToCharArray(bytes);
        Vector2 coordinate = new Vector2((int)char.GetNumericValue(convertedBytes[0]), (int)char.GetNumericValue(convertedBytes[1]));

        //Enemy fires on player
        CoordinateManager.Instance.UpdatePlayerTerritory(coordinate, state);
    }

    //Update Player Teritory
    public static void PlayerFire(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        Coordinate state = (Coordinate)stream.ReadUInt(ref context);
        int streamLenght = stream.ReadInt(ref context);
        byte[] bytes = stream.ReadBytesAsArray(ref context, streamLenght);

        //Convert recieved ByteArray to Vector2
        char[] convertedBytes = Conversions.BytesToCharArray(bytes);
        Vector2 coordinate = new Vector2((int)char.GetNumericValue(convertedBytes[0]), (int)char.GetNumericValue(convertedBytes[1]));

        //Player fires on enemy
        CoordinateManager.Instance.UpdateEnemyTerritory(coordinate, state);
    }

    public static void Win(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        client.gameStateMachine.ChangeFase(Command.EndGame);
        client.gameMenu.OpenWinMenu();
    }

    public static void Loss(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        client.gameStateMachine.ChangeFase(Command.EndGame);
        client.gameMenu.OpenLossMenu();
    }

    public static void PingToClient(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        ClientBehaviour client = caller as ClientBehaviour;
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ClientToServerEvent.PING_TO_SERVER);
            client.m_Driver.Send(NetworkPipeline.Null, client.m_Connection, writer);
        }
    }
}
