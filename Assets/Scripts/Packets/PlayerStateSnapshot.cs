using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 개별 플레이어의 서버 권위 상태
/// </summary>
public struct PlayerStateSnapshot : INetworkSerializable
{
    public ulong clientId;
    public uint lastProcessedTick;
    public Vector3 position;
    public float rotationY;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref lastProcessedTick);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotationY);
    }
}
