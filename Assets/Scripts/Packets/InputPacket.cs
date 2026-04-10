using Unity.Netcode;
using UnityEngine;
public class InputPacket : INetworkSerializable
{
    public Vector2 move;
    public Vector2 look;
    public bool firePressed;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref move);
        serializer.SerializeValue(ref look);
        serializer.SerializeValue(ref firePressed);
    }
}
