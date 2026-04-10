using System;
using System.Collections.Generic;
using Extensions;
using Jy.NetworkComponents;
using Jy.Packets;
using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterTransformController : NetworkComponent
{
    public class MoveSnapshot
    {
        /// <summary>
        /// 이 스냅샷 정보(위치, 회전)를 생성할 때 사용된 moveinput
        /// </summary>
        public Vector2 moveInput;
        /// <summary>
        /// 이 스냅샷 정보(위치, 회전)을 생성할 때 사용된 deltaTime
        /// </summary>
        public float deltaTime;
        /// <summary>
        /// 시뮬레이션 하기전 위치
        /// </summary>
        public Vector3 beforePosition;
        /// <summary>
        /// 프레임 넘버
        /// </summary>
        public float generatedTime;
        /// <summary>
        /// 계산된 위치
        /// </summary>
        public Vector3 afterPosition;
        /// <summary>
        /// 계산된 회전
        /// </summary>
        public bool used = false;
        public uint frameNumber = 0;
        public static MoveSnapshot Lerp(MoveSnapshot s1, MoveSnapshot s2, float t)
        {
            return new MoveSnapshot
            {
                moveInput = Vector2.Lerp(s1.moveInput, s2.moveInput, t),
                generatedTime = Mathf.Lerp(s1.generatedTime, s2.generatedTime, t),
                deltaTime = Mathf.Lerp(s1.deltaTime, s2.deltaTime, t),
                afterPosition = Vector3.Lerp(s1.afterPosition, s2.afterPosition, t),
                frameNumber = 0,
                used = false
            };
        }
    }

    [SerializeField, Range(0.0001f, 1f)] float movingEpsilon;
    [SerializeField] Vector2 moveInput;
    [SerializeField, Range(1f, 10f)] float movingSpeed;
    [SerializeField] int clientSnapshotCount = 0;
    [SerializeField] Vector3 positionGap;
    [SerializeField, Range(0.00001f, 1f)] float positionTolerance;

    [Header("Debug")]
    [SerializeField] int receivedSnapshotCount = 0;
    [SerializeField] int resimulatedSnapshotCount = 0;
    [SerializeField] int goodSimulatedSnapshotCount = 0;
    [SerializeField] int outOfRangeCount = 0;
    [SerializeField] int lastSimulatedServerFrameNumber = 0;

    List<MoveSnapshot> clientSnapshot = new List<MoveSnapshot>(16);
    List<MoveSnapshot> tempSnapshot = new List<MoveSnapshot>(16);

    public void Update()
    {
        if (IsServer || IsOwner)
        {
            MoveSnapshot newSnapshot = null;
            if (IsClient)
            {
                newSnapshot = new MoveSnapshot();
                newSnapshot.deltaTime = Time.deltaTime;
                newSnapshot.moveInput = moveInput;
                newSnapshot.beforePosition = transform.position;
            }

            Simulate(moveInput, Time.deltaTime);

            if (IsClient)
            {
                //if (positionGap != Vector3.zero)
                //{
                //    Debug.Log($"positionGap: {positionGap}");

                //    transform.position -= positionGap;
                //    positionGap = Vector3.zero;
                //}

                //clientSnapshot.RemoveAll(snapshot => snapshot.used);
                //clientSnapshot.RemoveAll(snapshot => snapshot.time < Time.time - 3f);

                clientSnapshotCount = clientSnapshot.Count;

                newSnapshot.generatedTime = Time.time;
                newSnapshot.afterPosition = transform.position;
                clientSnapshot.Add(newSnapshot);
            }
        }
    }

    private void Simulate(Vector2 moveInput, float deltaTime)
    {
        Vector3 right = transform.GetPlanarRight(transform.up) * moveInput.x;
        Vector3 forward = transform.GetPlanarFoward(transform.up) * moveInput.y;
        Vector3 movingDirection = right + forward;
        movingDirection.Normalize();

        Vector3 velocity = movingDirection * movingSpeed;

        transform.position += velocity * deltaTime;
    }

    [Rpc(SendTo.Everyone)]
    public void SetInitPositionRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    public override void RegisterServerSideListeners()
    {
        base.RegisterServerSideListeners();

        ServerEventBus.Input.onInputReceived += UpdateLookInput;
    }

    public override void UnregisterServerSideListeners()
    {
        base.UnregisterServerSideListeners();

        ServerEventBus.Input.onInputReceived -= UpdateLookInput;
    }

    public override void RegisterClientSideListeners()
    {
        base.RegisterClientSideListeners();

        ClientEventBus.Input.onInput += UpdateLookInput;
        ClientEventBus.Networks.OnFrameSnapshotReceived += OnFrameSnapshotReceived;
    }

    public override void UnregisterClientSideListeners()
    {
        base.UnregisterClientSideListeners();

        ClientEventBus.Input.onInput -= UpdateLookInput;
        ClientEventBus.Networks.OnFrameSnapshotReceived -= OnFrameSnapshotReceived;
    }

    private void OnFrameSnapshotReceived(FrameSnapshot snapshot)
    {
        if (lastSimulatedServerFrameNumber > snapshot.frameNumber)
        {
            Debug.Log($"서버로부터 받은 스냅샷 프레임 번호 : {snapshot.frameNumber}, 마지막으로 시뮬레이션한 서버 프레임 번호 : {lastSimulatedServerFrameNumber}, 무시합니다.");
            return;
        }

        if (!snapshot.netObjectSnapshotById.TryGetValue(NetworkObjectId, out var objectSnapshot))
            return;

        receivedSnapshotCount++;

        var snapshotFromServer = new MoveSnapshot()
        {
            generatedTime = snapshot.creationTime,
            afterPosition = objectSnapshot.position,
            frameNumber = snapshot.frameNumber
        };


        ulong rtt = NetworkManagerExtensions.GetInstance().NetworkConfig.NetworkTransport.GetCurrentRtt(OwnerClientId);
        //float rttInSec = ((float)rtt) / 1000f;
        //float halfRttInSec = rttInSec / 2f;
        float halfRttInSec = 0.05f; //for test
        float targetTime = snapshotFromServer.generatedTime - halfRttInSec;
        Debug.Log($"서버에서 스냅샷 받음, 생성 시간 : {snapshotFromServer.generatedTime}, 위치 : {snapshotFromServer.afterPosition}, 프레임 번호 : {snapshotFromServer.frameNumber}, HalfRTT : {halfRttInSec}");

        int leftIndex = -1;
        int rightIndex = -1;

        MoveSnapshot left = null;
        MoveSnapshot right = null;
        MoveSnapshot interpolated = null;

        try
        {
            Debug.Log($"보간점 찾기 시작 목표 시간:{snapshotFromServer.generatedTime}");

            for (int i = 0; i < clientSnapshot.Count - 1; i++)
            {
                left = clientSnapshot[i];
                right = clientSnapshot[i + 1];

                if (left.generatedTime <= targetTime && targetTime <= right.generatedTime)
                {
                    float rate = (targetTime - left.generatedTime) / (right.generatedTime - left.generatedTime);
                    interpolated = MoveSnapshot.Lerp(left, right, rate);
                    leftIndex = i;
                    rightIndex = i + 1;
                    Debug.Log($"보간점 찾음 left time : {left.generatedTime}, right time : {right.generatedTime}, target time : {targetTime}, rate : {rate}");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            outOfRangeCount++;
            Debug.Log($"범위 초과");
            return;
        }

        if (null == interpolated)
        {
            Debug.Log($"보간 점을 찾지 못함, client snapshot count : {clientSnapshot.Count}");
            return;
        }

        //good simulation!
        if (MathUtility.ApproximatleyEqual(interpolated.afterPosition, snapshotFromServer.afterPosition, positionTolerance))
        {
            Debug.Log("Good Simulation!");
            goodSimulatedSnapshotCount++;
            return;
        }

        Debug.Log($"보간점 위치: {interpolated.afterPosition}, 서버의 스냅샷 위치 : {snapshotFromServer.afterPosition}, 오차 : {(interpolated.afterPosition - snapshotFromServer.afterPosition).magnitude}, 허용 오차 : {positionTolerance}");
        resimulatedSnapshotCount++;

        //resimulation!
        Debug.Log("Resimulation!");

        clientSnapshot.RemoveRange(0, leftIndex);
        Debug.Log($"left item generationtime : {left.generatedTime}, targetTime : {targetTime}");

        float deltaTime = right.generatedTime - interpolated.generatedTime;
        float elapsedTime = interpolated.generatedTime;
        MoveSnapshot currentSnapshot = interpolated;

        tempSnapshot.Clear();

        while (clientSnapshot.Count > 0)
        {
            Debug.Log($"남은 스냅샷 : {clientSnapshot.Count}, deltaTime : {deltaTime}, elapsedTime : {elapsedTime}, currentTime : {currentSnapshot.generatedTime}, curDelta: {currentSnapshot.deltaTime}");
            transform.position = currentSnapshot.beforePosition;
            Simulate(currentSnapshot.moveInput, deltaTime);

            tempSnapshot.Add(new MoveSnapshot()
            {
                generatedTime = elapsedTime + deltaTime,
                afterPosition = transform.position,
                moveInput = currentSnapshot.moveInput,
                deltaTime = deltaTime,
                used = false
            });

            elapsedTime += deltaTime;

            currentSnapshot = clientSnapshot.Dequeue();
            deltaTime = currentSnapshot.deltaTime;
        }

        clientSnapshot.Clear();
        clientSnapshot.AddRange(tempSnapshot);
        lastSimulatedServerFrameNumber = (int)snapshotFromServer.frameNumber;

        Debug.Log($"시뮬래이션 후 위치 : {transform.position}");
        //Debug.Break();
    }

    private void UpdateLookInput(InputPacket packet, ulong senderId)
    {
        if (senderId != NetworkObject.OwnerClientId)
        {
            return;
        }

        moveInput = packet.move;
    }
}