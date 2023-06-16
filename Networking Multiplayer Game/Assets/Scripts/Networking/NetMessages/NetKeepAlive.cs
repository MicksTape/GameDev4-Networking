using Unity.Networking.Transport;
using UnityEngine;

public class NetKeepAlive : NetMessage {
    
    // Making the data
    public NetKeepAlive() {
        Code = OpCode.KEEP_ALIVE;
    }

    // Receiving the data
    public NetKeepAlive(DataStreamReader reader) {
        Code = OpCode.KEEP_ALIVE;
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
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }

    // If data received on the server
    public override void ReceivedOnServer(NetworkConnection cnn) {
        NetUtility.S_KEEP_ALIVE?.Invoke(this, cnn);
    }
}
