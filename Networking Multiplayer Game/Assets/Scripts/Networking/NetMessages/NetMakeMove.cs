using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;

public class NetMakeMove : NetMessage {
    public int unitNW;
    public FixedString64Bytes playerName;
    public int teamId;

    public NetMakeMove() {
        Code = OpCode.MAKE_MOVE;
    }

    public NetMakeMove(DataStreamReader reader) {
        Code = OpCode.MAKE_MOVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer) {
        base.Serialize(ref writer);
        writer.WriteInt(unitNW);
        writer.WriteFixedString64(playerName);
        writer.WriteInt(teamId);
    }

    public override void Deserialize(DataStreamReader reader) {
        base.Deserialize(reader);
        unitNW = reader.ReadInt();
        playerName = reader.ReadFixedString64();
        teamId = reader.ReadInt();
    }

    public override void ReceivedOnClient() {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn) {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
    }
}
