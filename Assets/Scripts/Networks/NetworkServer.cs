using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : NetworkBehaviour
{
    Queue<IStateEventParameter> eventQueue = new Queue<IStateEventParameter>(16);

    [SerializeField] IngameConfig ingameConfig;
    [SerializeField] Animator fsm;

    //public NetworkWorld NetworkWorld { get => networkWorld; }

    public bool HasEvents()
    {
        return eventQueue.Count > 0;
    }

    public IStateEventParameter DequeueEvent()
    {
        if (eventQueue.Count > 0)
        {
            return eventQueue.Dequeue();
        }
        return null;
    }


    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (IsClient)
        {
            fsm.enabled = false;
            this.enabled = false;
        }

        else if (IsServer)
        {
            fsm.enabled = true;
            this.enabled = true;
        }
    }


    [Rpc(SendTo.Server)]
    public void ReadyRpc(RpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        // 또는 서버 전용 필드가 있는 경우:
        // ulong senderClientId = rpcParams.Server.Receive.SenderClientId;
        Debug.Log($"Received ReadyRpc from client {senderClientId}");

        // 사용 예
        States.PlayerReadyParameter param = new()
        {
            PlayerId = senderClientId
        };

        eventQueue.Enqueue(param);
    }
}
