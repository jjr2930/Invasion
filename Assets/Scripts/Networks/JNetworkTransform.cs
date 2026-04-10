using System.Collections.Generic;
using Jy.NetworkComponents;
using Jy.Packets;
using UnityEngine;

public class JNetworkTransform : NetworkComponent
{
    public class Snapshot
    {
        public uint framNumber;
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public bool used = false;

        public Snapshot Lerp(Snapshot s1, Snapshot s2, float t)
        {
            return new Snapshot
            {
                framNumber = (uint)Mathf.Lerp(s1.framNumber, s2.framNumber, t),
                time = Mathf.Lerp(s1.time, s2.time, t),
                position = Vector3.Lerp(s1.position, s2.position, t),
                rotation = Quaternion.Slerp(s1.rotation, s2.rotation, t),
                used = false
            };
        }
    }

    [SerializeField, Range(0.00001f, 1f)] float positionEpsilon = 0.01f;
    [SerializeField, Range(0.00001f, 1f)] float rotationEpsilon = 0.01f;
    [SerializeField] int serverSnapshotCount = 0;
    [SerializeField] int clientSnapshotCount = 0;

    public Vector3 positionGap = Vector3.zero;

    List<Snapshot> clientSnapshot = new List<Snapshot>(16);

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (IsClient)
            ClientEventBus.Networks.OnFrameSnapshotReceived += OnFrameSnapshotReceived;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsClient)
            ClientEventBus.Networks.OnFrameSnapshotReceived -= OnFrameSnapshotReceived;
    }

    public void Update()
    {
        if (IsServer)
            return;

        if (!IsOwner)
            return;

        clientSnapshot.RemoveAll(snapshot => snapshot.used);
        clientSnapshot.RemoveAll(snapshot => snapshot.time < Time.time - 1f);

        clientSnapshotCount = clientSnapshot.Count;
        clientSnapshot.Add(new Snapshot
        {
            framNumber = 0, //idont want to know the frame number in client, so just set it to 0
            time = Time.time,
            position = transform.position,
            rotation = transform.rotation
        });

        //현재시간 보다 1초 앞에 있는 clientsnapshot은 지운다.
        //float thresholdTime = Time.time - 1f;
        //for (int i = clientSnapshot.Count - 1; i >= 0; --i)
        //{
        //    if (clientSnapshot[i].time < thresholdTime)
        //        clientSnapshot.RemoveAt(i);
        //}

        //int leftIndex = 0;
        //int rightIndex = 0;
        //Snapshot left;
        //Snapshot right;

        //Vector3 elapsedPositionGap = Vector3.zero;
        //Quaternion elapsedRotationGap = Quaternion.identity;

        ////1. clientSnapshot[0].time 1초 뒤의 서버 스냅샷들은 지운다
        //float time = clientSnapshot[0].time;
        //for (int i = serverSnapshot.Count - 1; i >= 0; --i)
        //{
        //    if (serverSnapshot[i].time < time)
        //    {
        //        serverSnapshot.RemoveAt(i);
        //    }
        //}


        //for (int i = 0; i < clientSnapshot.Count; ++i)
        //{
        //    for (int j = 0; j < serverSnapshot.Count - 1; ++j)
        //    {
        //        Snapshot tempLeft = serverSnapshot[j];
        //        Snapshot tempRight = serverSnapshot[j + 1];
        //        ulong rtt = NetworkManagerExtensions.GetInstance().NetworkConfig.NetworkTransport.GetCurrentRtt(OwnerClientId);
        //        ulong halfRtt = rtt / 2;
        //        float targetTime = clientSnapshot[i].time + halfRtt;
        //        if (tempLeft.time <= targetTime && tempRight.time >= targetTime)
        //        {
        //            //Debug.Log("Find");
        //            leftIndex = j;
        //            rightIndex = j + 1;
        //            left = tempLeft;
        //            right = tempRight;

        //            float t = (clientSnapshot[i].time - left.time) / (right.time - left.time);
        //            Snapshot interpolatedSnapshot = left.Lerp(left, right, t);

        //            elapsedPositionGap += interpolatedSnapshot.position - clientSnapshot[i].position;
        //            //elapsedRotationGap *= interpolatedSnapshot.rotation * Quaternion.Inverse(clientSnapshot[i].rotation);

        //            serverSnapshot[j].used = true;
        //            clientSnapshot[i].used = true;
        //            break;
        //        }
        //    }
        //}

        //if (elapsedPositionGap.magnitude > positionEpsilon)
        //{
        //    //Debug.Log($"position gap: {elapsedPositionGap}");
        //    transform.position -= elapsedPositionGap;
        //}

        //if (Quaternion.Angle(Quaternion.identity, elapsedRotationGap) > rotationEpsilon)
        //{
        //    Debug.Log($"Rotation gap: {Quaternion.Angle(Quaternion.identity, elapsedRotationGap)}");
        //    //transform.rotation = elapsedRotationGap * transform.rotation;
        //}

        ////remove used snapshot
        //int removedServerSnapshot = serverSnapshot.RemoveAll(s => s.used);
        //int removedClientSnapshot = clientSnapshot.RemoveAll(s => s.used);

        //if (removedServerSnapshot > 0)
        //{
        //    Debug.Log($"Removed {removedServerSnapshot} server snapshot");
        //}

        //if (removedClientSnapshot > 0)
        //{
        //    Debug.Log($"Removed {removedClientSnapshot} client snapshot");
        //}

        //serverSnapshotCount = serverSnapshot.Count;
        //clientSnapshotCount = clientSnapshot.Count;
    }

    public void LateUpdate()
    {
        if (IsServer)
            return;
        if (!IsOwner)
            return;

        if (positionGap.magnitude > positionEpsilon)
        {
            Debug.Log($"LateUpdate position gap: {positionGap}");
            transform.position += positionGap;
            positionGap = Vector3.zero;
        }
    }

    private void OnFrameSnapshotReceived(FrameSnapshot snapshot)
    {
        //Debug.Log($"Receive snapshot: {snapshot.frameNumber}, server time: {snapshot.creationTime}, clientTime : {Time.time}");
        if (!snapshot.netObjectSnapshotById.TryGetValue(NetworkObjectId, out var objectSnapshot))
            return;

        var newSnapshot = new Snapshot()
        {
            framNumber = snapshot.frameNumber,
            time = snapshot.creationTime,
            position = objectSnapshot.position,
            rotation = objectSnapshot.rotation
        };

        ulong rtt = NetworkManagerExtensions.GetInstance().NetworkConfig.NetworkTransport.GetCurrentRtt(OwnerClientId);
        ulong halfRtt = rtt / 2;
        float targetTime = newSnapshot.time - halfRtt;

        //clientSnapshot.RemoveAll(x => x.time < targetTime - 1f);

        int leftIndex = 0;
        int rightIndex = 0;
        Snapshot left;
        Snapshot right;
        Vector3 elapsedPositionGap = Vector3.zero;
        Vector3 nextPosition = transform.position;
        positionGap = Vector3.zero;
        for (int i = 0; i < clientSnapshot.Count - 1; ++i)
        {
            Snapshot tempLeft = clientSnapshot[i];
            Snapshot tempRight = clientSnapshot[i + 1];
            if (tempLeft.time <= targetTime && tempRight.time >= targetTime)
            {
                //Debug.Log("Find");
                leftIndex = i;
                rightIndex = i + 1;
                left = tempLeft;
                right = tempRight;

                float t = (newSnapshot.time - left.time) / (right.time - left.time);
                Snapshot interpolatedSnapshot = left.Lerp(left, right, t);

                positionGap += newSnapshot.position - interpolatedSnapshot.position;
                //elapsedRotationGap *= interpolatedSnapshot.rotation * Quaternion.Inverse(clientSnapshot[i].rotation);

                //transform.position += gap;

                clientSnapshot[i].used = true;

                Debug.Log($"targetTime : {targetTime}, serverSanpshot: {newSnapshot.position}, lerped:{interpolatedSnapshot.position}, gap: {positionGap}");
                break;
            }
        }

        //clientSnapshot.RemoveAll(x => x.used);

    }
}