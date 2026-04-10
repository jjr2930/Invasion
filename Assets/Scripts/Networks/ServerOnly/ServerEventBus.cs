using System;
using UnityEngine;

public static class ServerEventBus
{
    public static class Spawning
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            onNetworkCameraSpawned = null;
        }

        public static Action<NetworkCamera> onNetworkCameraSpawned;
    }
    public static class Input
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            onInputReceived = null;
        }

        public static Action<InputPacket, ulong> onInputReceived;
    }
}
