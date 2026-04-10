using System;
using Jy.Packets;
using UnityEngine;

public static class ClientEventBus
{
    public static class Networks
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            OnLocalPlayerCharacterSpawned = null;
            OnNetworkCameraSpawned = null;
        }

        public static Action<NetworkPlayerCharacter> OnLocalPlayerCharacterSpawned;
        public static Action<NetworkCamera> OnNetworkCameraSpawned;
        public static Action<FrameSnapshot> OnFrameSnapshotReceived;

    }

    public static class Input
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            onInput = null;
        }
        
        public static Action<InputPacket,ulong> onInput;
    }
}
