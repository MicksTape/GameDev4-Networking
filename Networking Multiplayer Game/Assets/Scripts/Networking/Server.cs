using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;

public class Server : MonoBehaviour {
    #region Singleton implementation
    public static Server Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;

    public Action connectionDropped;

    private void Start() {
        Init(ushort.MaxValue); // Provide a suitable default port value
    }

    // When a server is created
    public void Init(ushort port) {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;

        if (driver.Bind(endpoint) != 0) {
            Debug.Log("Unable to bind on port " + endpoint.Port);
            return;
        } else {
            driver.Listen();
            Debug.Log("Currently listening on port " + endpoint.Port);
        }

        // Max players that can be connected
        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }

    // When the server is shutdown
    public void Shutdown() {
        if (isActive) {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
            Debug.Log("Server has been shutdown");
        }
    }

    private void OnDestroy() {
        Shutdown();
    }

    private void Update() {
        if (!isActive) {
            return;
        }

        KeepAlive();

        driver.ScheduleUpdate().Complete();
        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }

    private void KeepAlive() {
        if (Time.time - lastKeepAlive > keepAliveTickRate) {
            lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }

    // Is there a client not connected to the server but still have the reference, remove the connection
    private void CleanupConnections() {
        for (int i = 0; i < connections.Length; i++) {
            if (!connections[i].IsCreated) {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }
    }

    // Is there a client wanting to enter the server
    private void AcceptNewConnections() {
        NetworkConnection c;
        while ((c = driver.Accept()) != default(NetworkConnection)) {
            connections.Add(c);
            Debug.Log("Accepted a connection");
        }
    }

    // Is a client sending the server a message, and if so, do we have to reply
    private void UpdateMessagePump() {
        DataStreamReader stream;

        for (int i = 0; i < connections.Length; i++) {
            // Create network event
            NetworkEvent.Type cmd;

            // If the network command is not empty
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty) {
                if (cmd == NetworkEvent.Type.Data) {
                    NetUtility.OnData(stream, connections[i], this);
                } else if (cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client disconnected from server");
                    connections[i] = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    Shutdown(); // Only shutdown in a two person game
                }
            }
        }
    }

    // Server specific

    // Send a specific message to a specific player, and only to this player
    public void SendToClient(NetworkConnection connection, NetMessage msg) {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    // Send a message to every single client (in this case both players)
    public void Broadcast(NetMessage msg) {
        for (int i = 0; i < connections.Length; i++) {
            if (connections[i].IsCreated) {
                Debug.Log($"Sending {msg.Code} to: {connections[i].InternalId}");
                SendToClient(connections[i], msg);
            }
        }
    }
}