using Unity.Networking.Transport;
using UnityEngine;
using Unity.Collections;

public class NetWelcome : NetMessage {
    
    public int AssignedTeam { set; get; }

    // Making the data
    public NetWelcome() {
        Code = OpCode.WELCOME;
    }

    // Receiving the data
    public NetWelcome(DataStreamReader reader) {
        Code = OpCode.WELCOME;
        Deserialize(reader);
    }


    // Pack the data
    public override void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)Code);
        writer.WriteInt(AssignedTeam);
    }

    // Unpack the data
    public override void Deserialize(DataStreamReader reader) {
        // We already read the byte in the NetUtility::OnData
        AssignedTeam = reader.ReadInt();
    }


    // If data received on the client
    public override void ReceivedOnClient() {
        NetUtility.C_WELCOME?.Invoke(this);
    }

    // If data received on the server
    public override void ReceivedOnServer(NetworkConnection cnn) {
        NetUtility.S_WELCOME?.Invoke(this, cnn);
    }
}
