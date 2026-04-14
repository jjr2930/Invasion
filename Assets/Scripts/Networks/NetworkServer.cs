using System;
using System.Collections;
using System.Collections.Generic;
using Jy.Packets;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : NetworkBehaviour
{
    Queue<IStateEventParameter> eventQueue = new Queue<IStateEventParameter>(16);

    [SerializeField] IngameConfig ingameConfig;
    [SerializeField] Animator fsm;
    [SerializeField] uint frameNumber;
    FrameSnapshotContainer frameSnapshotContainer = new FrameSnapshotContainer();
    [SerializeField] NetworkWorld networkWorld;

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
            frameNumber = 0;
            fsm.enabled = true;
            this.enabled = true;
            Application.targetFrameRate = (int)NetworkManager.NetworkTickSystem.TickRate;
        }

        //if(null == networkWorld)
        //{
        //    networkWorld = new NetworkWorld();
        //    Assert.IsNotNull(networkWorld, "NetworkWorld not found in the scene. Please ensure a NetworkWorld is present.");
        //}
    }

    //float startTime;
    //int frameCount = 0;
    public void Update()
    {
        if (!IsServer)
            return;

        if (!IsSpawned)
            return;

        frameNumber++;
        var currentWorldSnapshot = MakeCurrentWorldSnapshot();
        currentWorldSnapshot.DebugPrint(GetEntityId().ToString());
        //SendSnapshotRpc(currentWorldSnapshot);

        if(currentWorldSnapshot.creationTime > NetworkManagerExtensions.GetInstance().ServerTime.TimeAsFloat)
        {
            Debug.Log($"생성된 서버 스냅샷이 너무 미래에 생성된 스냅샷임, 서버 시간 : {currentWorldSnapshot.creationTime}, 현재 서버 시간 : {NetworkManagerExtensions.GetInstance().ServerTime.TimeAsFloat}");
        }

        StartCoroutine(SendPacketWithDelay(currentWorldSnapshot));

        frameSnapshotContainer.Add(currentWorldSnapshot);
    }

    IEnumerator SendPacketWithDelay(FrameSnapshot snapshot)
    {
        yield return new WaitForSeconds(this.ingameConfig.customRtt * 0.5f); // rtt 절반
        // 패킷 전송 로직 작성

        SendSnapshotRpc(snapshot);
    }

    [Rpc(SendTo.NotServer)]
    private void SendSnapshotRpc(FrameSnapshot frameSnapshot, RpcParams rpcParam = default)
    {

        //Debug.Log("Recieved snapshot rpc on client with frame number " + frameSnapshot.frameNumber);
        ClientEventBus.Networks.OnFrameSnapshotReceived?.Invoke(frameSnapshot);
    }

    private Jy.Packets.FrameSnapshot MakeCurrentWorldSnapshot()
    {
        //using (new TimeCheckingScope("Snapshot generation time : {0:F10}sec"))
        {
            FrameSnapshot snapshot = new FrameSnapshot();
            snapshot.frameNumber = frameNumber;
            snapshot.creationTime = NetworkManagerExtensions.GetInstance().ServerTime.TimeAsFloat;
            snapshot.doubleTime = NetworkManager.ServerTime.Time;

            //느릴거 같은데?
            foreach (KeyValuePair<ulong, NetworkObject> pair in NetworkManagerExtensions.GetInstance().SpawnManager.SpawnedObjects)
            {
                Transform transform = pair.Value.GetComponent<Transform>();

                snapshot.netObjectSnapshotById.Add(pair.Key, new NetworkObjectSnapshot()
                {
                    networkId = pair.Key,
                    position = transform.position,
                    rotation = transform.rotation,
                });
            }
            return snapshot;
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
