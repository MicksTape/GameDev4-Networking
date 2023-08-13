using Unity.Networking.Transport;
using UnityEngine;
using Unity.Collections;

public class NetStartGame : NetMessage {

    public FixedString64Bytes SessionId { set; get; }

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
        writer.WriteFixedString64(SessionId);
    }

    // Unpack the data
    public override void Deserialize(DataStreamReader reader) {
        SessionId = reader.ReadFixedString64();
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
