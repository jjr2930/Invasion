using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Jy.Packets
{
    public class DeltaNetworkObjectSnapshot : INetworkSerializer
    {
        public int networkid;
        public Vector3? position = null;
        public Quaternion? rotation = null;
        public void Deserialize(ref DataStreamReader stream)
        {
            stream.Read(out networkid);
            stream.Read(out bool hasPosition);
            if (hasPosition)
            {
                stream.Read(out Vector3 readPosition);
                position = readPosition;
            }

            stream.Read(out bool hasRotation);
            if (hasRotation)
            {
                stream.Read(out Quaternion readRotation);
                rotation = readRotation;
            }
        }

        public void Serialize(ref DataStreamWriter stream)
        {
            stream.Write(networkid);

            stream.Write(position.HasValue);
            if (position.HasValue)
                stream.Write(position.Value);

            stream.Write(rotation.HasValue);
            if (rotation.HasValue)
                stream.Write(rotation.Value);
        }

        public static DeltaNetworkObjectSnapshot Create(NetworkObjectSnapshot a, NetworkObjectSnapshot b)
        {
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.IsTrue(a.networkId == b.networkId);

            DeltaNetworkObjectSnapshot delta = new DeltaNetworkObjectSnapshot();
            //delta.networkid = a.networkId;
            if (delta.position != b.position)
                delta.position = b.position;

            if (delta.rotation != b.rotation)
                delta.rotation = b.rotation;

            return delta;
        }
    }
}