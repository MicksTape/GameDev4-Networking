using System;
using Unity.Networking.Transport;
using UnityEngine;

// Game events
public enum OpCode {
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    QUIT_GAME = 5
}

public static class NetUtility {
    // When data is received, read the content
    public static void OnData(DataStreamReader stream, NetworkConnection cnn, Server server = null) {
        NetMessage msg = null;
        OpCode opCode = (OpCode)stream.ReadByte();
        switch (opCode) {
            case OpCode.KEEP_ALIVE:
                msg = new NetKeepAlive(stream);
                break;
            case OpCode.WELCOME:
                msg = new NetWelcome(stream);
                break;
            case OpCode.START_GAME:
                msg = new NetStartGame(stream);
                break;
            case OpCode.MAKE_MOVE:
                msg = new NetMakeMove(stream);
                break;
            case OpCode.QUIT_GAME:
                msg = new NetQuitGame(stream);
                break;
            default:
                Debug.LogError("Message received had no OpCode");
                break;
        }

        // Check whether data is received from the server or the client
        if (server != null) {
            msg.ReceivedOnServer(cnn);
        } else {
            msg.ReceivedOnClient();
        }
    }

    // Net messages
    public static Action<NetMessage> C_KEEP_ALIVE;
    public static Action<NetMessage> C_WELCOME;
    public static Action<NetMessage> C_START_GAME;
    public static Action<NetMessage> C_MAKE_MOVE;
    public static Action<NetMessage> C_QUIT_GAME;
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_START_GAME;
    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<NetMessage, NetworkConnection> S_QUIT_GAME;
}
