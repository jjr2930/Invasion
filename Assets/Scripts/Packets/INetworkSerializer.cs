using Unity.Collections;

namespace Jy.Packets
{
    public interface INetworkSerializer
    {
        void Serialize(ref DataStreamWriter stream);
        void Deserialize(ref DataStreamReader stream);
    }
}