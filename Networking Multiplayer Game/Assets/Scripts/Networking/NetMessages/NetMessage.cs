using Unity.Networking.Transport;

// When sending data from client to server or server to client, we're sending sockets with pieces of information. This socket can be read and written.
public class NetMessage {
    
    public OpCode Code { set; get; }


    public virtual void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)Code);
    }


    public virtual void Deserialize(DataStreamReader reader) {

    }


    public virtual void ReceivedOnClient() {

    }


    public virtual void ReceivedOnServer(NetworkConnection cnn) {

    }

}
