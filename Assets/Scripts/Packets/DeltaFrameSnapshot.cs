using System.Collections.Generic;
using Unity.Collections;

namespace Jy.Packets
{
    public class DeltaFrameSnapshot : INetworkSerializer, IPacket
    {
        public uint frameNumber1;
        public uint frameNumber2;

        public List<DeltaNetworkObjectSnapshot> deltaNetObjectSnapshots = new List<DeltaNetworkObjectSnapshot>(16);
        public List<int> removedNetObjects = new List<int>(16);

        public PacketTypes PacketType => PacketTypes.DeltaFrame;

        public void Serialize(ref DataStreamWriter stream)
        {
            stream.Write((byte)PacketType);
            stream.Write(frameNumber1);
            stream.Write(frameNumber2);
            stream.Write(deltaNetObjectSnapshots.Count);
            for (int i = 0; i < deltaNetObjectSnapshots.Count; i++)
            {
                deltaNetObjectSnapshots[i].Serialize(ref stream);
            }

            stream.Write(removedNetObjects.Count);
            for (int i = 0; i < removedNetObjects.Count; i++)
            {
                stream.Write(removedNetObjects[i]);
            }
        }

        public void Deserialize(ref DataStreamReader stream)
        {
            stream.Read(out PacketTypes dummy);
            stream.Read(out frameNumber1);
            stream.Read(out frameNumber2);
            stream.Read(out int deltaCount);
            deltaNetObjectSnapshots.Clear();
            for (int i = 0; i < deltaCount; ++i)
            {
                DeltaNetworkObjectSnapshot newDelta = new DeltaNetworkObjectSnapshot();
                newDelta.Deserialize(ref stream);
            }

            stream.Read(out deltaCount);
            removedNetObjects.Clear();
            for (int i = 0; i < deltaCount; ++i)
            {
                stream.Read(out int id);
                removedNetObjects.Add(id);
            }
        }

        public static DeltaFrameSnapshot Create(FrameSnapshot a, FrameSnapshot b)
        {
            DeltaFrameSnapshot delta = new DeltaFrameSnapshot();
            delta.frameNumber1 = a.frameNumber;
            delta.frameNumber2 = b.frameNumber;

            foreach (var aPair in a.netObjectSnapshotById)
            {
                if (!b.netObjectSnapshotById.ContainsKey(aPair.Key))
                {
                    //delta.removedNetObjects.Add(aPair.Key);
                }
                else
                {
                    DeltaNetworkObjectSnapshot deltaNetObject = DeltaNetworkObjectSnapshot.Create(aPair.Value, b.netObjectSnapshotById[aPair.Key]);
                    delta.deltaNetObjectSnapshots.Add(deltaNetObject);
                }
            }

            return delta;
        }
    }
}