using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

namespace Jy.Jobs
{
    public struct UpdateJob : IJobParallelFor
    {
        public NetworkDriver.Concurrent Driver;
        public NativeArray<NetworkConnection> Connections;

        void IJobParallelFor.Execute(int i)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = Driver.PopEventForConnection(Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    uint number = stream.ReadUInt();

                    Debug.Log($"Got {number} from a client, adding 2 to it.");
                    number += 2;

                    Driver.BeginSend(Connections[i], out var writer);
                    writer.WriteUInt(number);
                    Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    Connections[i] = default;
                }
            }
        }
    }
}
