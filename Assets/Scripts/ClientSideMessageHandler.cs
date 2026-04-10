using System;
using Jy.Packets;
using Unity.Netcode;
using UnityEngine;

public class ClientSideMessageHandler : MonoBehaviour
{
    public Action<FrameSnapshot> OnFrameSnapshotReceived;
    void Start()
    {
        NetworkManagerExtensions.GetInstance().CustomMessagingManager.OnUnnamedMessage += HandleUnnamedMessage;
    }

    void OnDestroy()
    {
        if (null == NetworkManagerExtensions.GetInstance())
            return;

        NetworkManagerExtensions.GetInstance().CustomMessagingManager.OnUnnamedMessage -= HandleUnnamedMessage;
    }

    private void HandleUnnamedMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValue<uint>(out uint t);
        PacketTypes pt = (PacketTypes)t;
        switch (pt)
        {
            case PacketTypes.Error:
                break;
            case PacketTypes.Frame:
                {
                    FrameSnapshot fp = new FrameSnapshot();
                    //fp.Deserialize(ref reader);
                    Debug.Log($"Received frame packet with frame number {fp.frameNumber}");
                    
                    OnFrameSnapshotReceived?.Invoke(fp);
                }
                break;
            case PacketTypes.DeltaFrame:
                break;
            case PacketTypes.Input:
                break;
            default:
                break;
        }
    }
}
