using Unity.Networking.Transport;
using UnityEngine;

public class NetStartGame : NetMessage {

    // Making the data
    public NetStartGame() {
        Code = OpCode.START_GAME;
    }

    // Receiving the data
    public NetStartGame(DataStreamReader reader) {
        Code = OpCode.START_GAME;
        Deserialize(reader);
    }


    // Pack the data
    public override void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)Code);
    }

    // Unpack the data
    public override void Deserialize(DataStreamReader reader) {

    }


    // If data received on the client
    public override void ReceivedOnClient() {
        NetUtility.C_START_GAME?.Invoke(this);
    }

    // If data received on the server
    public override void ReceivedOnServer(NetworkConnection cnn) {
        NetUtility.S_START_GAME?.Invoke(this, cnn);
    }
}
