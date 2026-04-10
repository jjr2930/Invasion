using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

namespace Jy.Jobs
{
    public struct UpdateConnectionJob : IJob
    {
        public NetworkDriver Driver;
        public NativeList<NetworkConnection> Connections;

        public void Execute()
        {
            // Clean up connections.
            for (int i = 0; i < Connections.Length; i++)
            {
                if (!Connections[i].IsCreated)
                {
                    Connections.RemoveAtSwapBack(i);
                    i--;
                }
            }

            // Accept new connections.
            NetworkConnection c;
            while ((c = Driver.Accept()) != default)
            {
                Connections.Add(c);
                Debug.Log("Accepted a connection.");
            }
        }
    }
}
