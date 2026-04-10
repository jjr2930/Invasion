using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Jy.Packets
{
    public class NetworkObjectSnapshot : INetworkSerializer, INetworkSerializable
    {
        public ulong networkId;
        public Vector3 position;
        public Quaternion rotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref networkId);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
        }

        public void Deserialize(ref DataStreamReader stream)
        {
            stream.Read(out networkId);
            stream.Read(out position);
            stream.Read(out rotation);
        }

        public void Serialize(ref DataStreamWriter stream)
        {
            stream.Write(networkId);
            stream.Write(position);
            stream.Write(rotation);
        }
    }
}