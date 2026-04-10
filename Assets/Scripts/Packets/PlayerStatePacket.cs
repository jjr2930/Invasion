using Unity.Netcode;
using UnityEngine;

public struct PlayerStatePacket : INetworkSerializable
{
    public uint lastProcessedTick;
    public Vector3 position;
    public float rotationY;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref lastProcessedTick);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotationY);
    }
}
