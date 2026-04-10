using Unity.Collections;

namespace Jy.Packets
{
    public abstract class PacketFromClient : INetworkSerializer, IPacket
    {
        static uint sequnceCount;

        public uint lastServerSnapshot;
        public uint thisSequenceNumber;
        public PacketFromClient()
        {
            thisSequenceNumber = sequnceCount++;
        }

        public abstract PacketTypes PacketType { get; }

        public virtual void Deserialize(ref DataStreamReader stream)
        {
            stream.Read(out PacketTypes dummy);
            stream.Read(out uint thisSequenceNumber);
            stream.Read(out lastServerSnapshot);
        }

        public virtual void Serialize(ref DataStreamWriter stream)
        {
            stream.Write((byte)PacketType);
            stream.Write(thisSequenceNumber);
            stream.Write(lastServerSnapshot);
        }
    }
}