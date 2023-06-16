using Unity.Networking.Transport;
using UnityEngine;

public class NetQuitGame : NetMessage {
    
    public int teamId;
    public int hasQuit;

    // Making the data
    public NetQuitGame() {
        Code = OpCode.QUIT_GAME;
    }

    // Receiving the data
    public NetQuitGame(DataStreamReader reader) {
        Code = OpCode.QUIT_GAME;
        Deserialize(reader);
    }


    // Pack the data
    public override void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)Code);
        writer.WriteInt(teamId);
        writer.WriteInt(hasQuit);

    }

    // Unpack the data
    public override void Deserialize(DataStreamReader reader) {
        teamId = reader.ReadInt();
        hasQuit = reader.ReadInt();
    }


    // If data received on the client
    public override void ReceivedOnClient() {
        NetUtility.C_QUIT_GAME?.Invoke(this);
    }

    // If data received on the server
    public override void ReceivedOnServer(NetworkConnection cnn) {
        NetUtility.S_QUIT_GAME?.Invoke(this, cnn);
    }
}
