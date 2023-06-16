using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;

public class Client : MonoBehaviour {
    public static Client Instance { get; private set; }

    public NetworkDriver driver;
    private NetworkConnection connection;

    private bool isActive = false;

    public Action connectionDropped;

    private void Awake() {
        Instance = this;
    }

    public void Init(string ip, ushort port) {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);

        connection = driver.Connect(endpoint);

        Debug.Log("Attempting to connect to server on " + endpoint.Address);

        isActive = true;

        RegisterToEvent();
    }

    public void Shutdown() {
        if (isActive) {
            UnregisterFromEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
            Debug.Log("Client disconnected from server");
        }
    }

    private void OnDestroy() {
        Shutdown();
    }

    private void Update() {
        if (!isActive) {
            return;
        }

        driver.ScheduleUpdate().Complete();
        CheckConnection();

        UpdateMessagePump();
    }

    private void CheckConnection() {
        if (!connection.IsCreated && isActive) {
            Debug.Log("Something went wrong, lost connection to server");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }

    private void UpdateMessagePump() {
        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty) {
            switch (cmd) {
                case NetworkEvent.Type.Connect:
                    SendToServer(new NetWelcome());
                    Debug.Log("We're connected!");
                    break;
                case NetworkEvent.Type.Data:
                    NetUtility.OnData(stream, default(NetworkConnection));
                    break;
                case NetworkEvent.Type.Disconnect:
                    Debug.Log("Client disconnected from server");
                    connection = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    Shutdown();
                    break;
            }
        }
    }

    public void SendToServer(NetMessage msg) {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    private void RegisterToEvent() {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }

    private void UnregisterFromEvent() {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm) {
        SendToServer(nm);
    }
}