using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Jy.Packets
{
    public class FrameSnapshot : INetworkSerializable
    {
        public uint frameNumber;
        public float creationTime;
        public double doubleTime;
        public Dictionary<ulong, NetworkObjectSnapshot> netObjectSnapshotById
            = new Dictionary<ulong, NetworkObjectSnapshot>(32);

        public long ms;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref frameNumber);
            serializer.SerializeValue(ref creationTime);
            serializer.SerializeValue(ref doubleTime);          
            serializer.SerializeValue(ref ms);

            ulong[] keys = null;
            NetworkObjectSnapshot[] values = null;

            int count = netObjectSnapshotById.Count;
            serializer.SerializeValue(ref count);

            if (serializer.IsWriter)
            {
                keys = netObjectSnapshotById.Keys.ToArray();
                values = netObjectSnapshotById.Values.ToArray();
            }
            else
            {
                keys = new ulong[count];
                values = new NetworkObjectSnapshot[count];
            }

            // Serialize both arrays
            serializer.SerializeValue(ref keys);
            serializer.SerializeValue(ref values);

            if (serializer.IsReader)
            {
                netObjectSnapshotById.Clear();
                for (int i = 0; i < count; i++)
                {
                    netObjectSnapshotById.Add(keys[i], values[i]);
                }
            }

        }

        //public void Serialize(ref DataStreamWriter stream)
        //{
        //    stream.Write(frameNumber);
        //    stream.Write((uint)netObjectSnapshotById.Count);

        //    foreach (var element in netObjectSnapshotById)
        //    {
        //        element.Value.Serialize(ref stream);
        //    }
        //}

        //public void Serialize(ref FastBufferWriter writer)
        //{
        //    writer.WriteValue<uint>(frameNumber);
        //    writer.WriteValue<float>(creationTime);
        //    writer.WriteValue<uint>((uint)netObjectSnapshotById.Count);
        //    foreach (var element in netObjectSnapshotById)
        //    {
        //        element.Value.Serialize(ref writer);
        //    }
        //}


        //public void Deserialize(ref DataStreamReader stream)
        //{
        //    stream.Read(out uint dummy);
        //    stream.Read(out frameNumber);
        //    netObjectSnapshotById.Clear();
        //    stream.Read(out int playerCount);
        //    for (int i = 0; i < playerCount; i++)
        //    {
        //        NetworkObjectSnapshot newSnapshot = new NetworkObjectSnapshot();
        //        newSnapshot.Deserialize(ref stream);

        //        //netObjectSnapshotById.Add(i, newSnapshot);
        //    }
        //}

        //public void Deserialize(ref FastBufferReader reader)
        //{
        //    reader.ReadValue<uint>(out frameNumber);
        //    reader.ReadValue<float>(out creationTime);
        //    netObjectSnapshotById.Clear();
        //    reader.ReadValue<uint>(out uint playerCount);
        //    for (int i = 0; i < playerCount; i++)
        //    {
        //        NetworkObjectSnapshot newSnapshot = new NetworkObjectSnapshot();
        //        newSnapshot.Deserialize(ref reader);
        //        netObjectSnapshotById.Add(newSnapshot.networkId, newSnapshot);
        //    }
        //}


        /// <returns></returns>
        public static FrameSnapshot LerpByTime(FrameSnapshot a, FrameSnapshot b, float time)
        {
            Assert.IsTrue(a.creationTime < b.creationTime);
            Assert.IsTrue(a.creationTime < time);
            Assert.IsTrue(b.creationTime > time);

            float length = b.creationTime - a.creationTime;
            float rate = (time - a.creationTime) / length;

            FrameSnapshot lerped = new FrameSnapshot();

            lerped.frameNumber = uint.MaxValue; //it means made from lerp.
            lerped.creationTime = Mathf.Lerp(a.creationTime, b.creationTime, rate);

            foreach (var aPair in a.netObjectSnapshotById)
            {
                lerped.netObjectSnapshotById.Add(aPair.Key, aPair.Value);
            }

            foreach (var bPair in b.netObjectSnapshotById)
            {
                if (!lerped.netObjectSnapshotById.ContainsKey(bPair.Key))
                {
                    lerped.netObjectSnapshotById.Add(bPair.Key, bPair.Value);
                }
            }

            foreach (var lerpPair in lerped.netObjectSnapshotById)
            {
                bool hasA = a.netObjectSnapshotById.TryGetValue(lerpPair.Key, out NetworkObjectSnapshot aNetObjectSnapshot);
                bool hasB = b.netObjectSnapshotById.TryGetValue(lerpPair.Key, out NetworkObjectSnapshot bNetObjectSnapshot);
                Assert.IsTrue(hasA || hasB); //모두 없는 경우는 없어야함

                if (hasA && !hasB)
                    lerped.netObjectSnapshotById[lerpPair.Key] = aNetObjectSnapshot;
                else if (!hasA && hasB)
                    lerped.netObjectSnapshotById[lerpPair.Key] = bNetObjectSnapshot;
                else
                {
                    NetworkObjectSnapshot lerpedNetObject = lerped.netObjectSnapshotById[lerpPair.Key];
                    lerpedNetObject.position = Vector3.Lerp(aNetObjectSnapshot.position, bNetObjectSnapshot.position, rate);
                    lerpedNetObject.rotation = Quaternion.Slerp(aNetObjectSnapshot.rotation, bNetObjectSnapshot.rotation, rate);
                }
            }

            return lerped;
        }

        StringBuilder sb = new StringBuilder(1024);
        public void DebugPrint(string tag)
        {
            sb.Clear();

            sb.AppendLine($"FrameNumber : {frameNumber}, CreationTime : {creationTime}, localtime : {NetworkManagerExtensions.GetInstance().LocalTime.TimeAsFloat}");
            foreach (var pair in netObjectSnapshotById)
            {
                sb.AppendLine($"NetworkId : {pair.Key}, Position : {pair.Value.position}, Rotation : {pair.Value.rotation}");
            }

            sb.Append($"{tag}");
            Debug.Log(sb.ToString());
        }
    }
}