using System;
using System.Collections.Generic;
using Extensions;
using Jy.NetworkComponents;
using Jy.Packets;
using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterTransformController : NetworkComponent
{
    [Serializable]
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
    [SerializeField, Range(0.0f, 10f)] float positionToleranceMultiplier = 2;
    [SerializeField, Range(0.0001f, 3f)] float targetTimeServerFrameTolerance = 2;
    [SerializeField, Range(0.0001f, 3f)] float targetTimeClientFrameTolerance = 2;
    [SerializeField] IngameConfig ingameConfig;
    [SerializeField, Range(-1f, 1f)] float timeCorrection = 0f;

    [Header("Debug")]
    [SerializeField] int receivedSnapshotCount = 0;
    [SerializeField] int resimulatedSnapshotCount = 0;
    [SerializeField] int goodSimulatedSnapshotCount = 0;
    [SerializeField] int outOfRangeCount = 0;
    [SerializeField] uint lastSimulatedServerFrameNumber = 0;
    [SerializeField] float lastReceivedServerSnapshotTime = 0f;

    [SerializeField] List<MoveSnapshot> clientSnapshot = new List<MoveSnapshot>(16);
    [SerializeField] List<MoveSnapshot> snapshotFromServer = new List<MoveSnapshot>(16);

    List<MoveSnapshot> tempSnapshot = new List<MoveSnapshot>(16);

    public void Update()
    {
        if (!IsServer && !IsOwner)
            return;

        Simulate(moveInput, Time.deltaTime);

        if (this.IsOwnedByLocalPlayer())
        {
            //Debug.Log("유저가 소유중 스냅샷 만들기와, 보정 작업 시작");
            clientSnapshot.RemoveAll(s => s.generatedTime < NetworkManagerExtensions.GetInstance().ServerTime.TimeAsFloat - 1f);

            MoveSnapshot newSnapshot = new MoveSnapshot();
            newSnapshot.deltaTime = Time.deltaTime;
            newSnapshot.moveInput = moveInput;
            newSnapshot.beforePosition = transform.position;
            newSnapshot.generatedTime = NetworkManagerExtensions.GetInstance().ServerTime.TimeAsFloat;
            newSnapshot.afterPosition = transform.position;

            clientSnapshot.Add(newSnapshot);
            clientSnapshotCount = clientSnapshot.Count;


            while (snapshotFromServer.Count > 0)
            {
                int foundLeftIndex = -1;
                MoveSnapshot serverSnapshot = snapshotFromServer.Dequeue();
                if (serverSnapshot.frameNumber < lastSimulatedServerFrameNumber)
                    continue;

                lastSimulatedServerFrameNumber = serverSnapshot.frameNumber;
                MoveSnapshot interpolated = null;

                if (clientSnapshot.Count < 2)
                {
                    Debug.Log($"보간점 찾기 위한 스냅샷 부족, clientSnapshotCount : {clientSnapshot.Count}");
                    continue;
                }

                bool goodSimulation = false;
                float serverOneFrameDuration = 1f / NetworkManager.NetworkTickSystem.TickRate;
                float clientOneFrameDuration = 1f / Application.targetFrameRate;
                float clientHalfFrameDuration = clientOneFrameDuration * 0.5f;

                //테스트용 RTT, 실제론 네트워크에서 측정된 RTT값을 사용해야함
                float halfRtt = ingameConfig.customRtt * 0.5f;
                float positionTolerance = movingSpeed * clientOneFrameDuration * positionToleranceMultiplier;
                float targetTime = 0;

                for (int i = 0; i < clientSnapshot.Count - 1; i++)
                {
                    MoveSnapshot clientLeftSnapshot = clientSnapshot[i];
                    MoveSnapshot clientRightSnapshot = clientSnapshot[i + 1];
                    targetTime = serverSnapshot.generatedTime - halfRtt;
                    targetTime -= serverOneFrameDuration * targetTimeServerFrameTolerance;
                    targetTime -= clientOneFrameDuration * targetTimeClientFrameTolerance;
                    targetTime += timeCorrection;

                    if (clientLeftSnapshot.generatedTime <= targetTime && targetTime <= clientRightSnapshot.generatedTime)
                    {
                        float rate = (targetTime - clientLeftSnapshot.generatedTime) / (clientRightSnapshot.generatedTime - clientLeftSnapshot.generatedTime);
                        interpolated = MoveSnapshot.Lerp(clientLeftSnapshot, clientRightSnapshot, rate);
                        if (MathUtility.ApproximatleyEqual(interpolated.afterPosition, serverSnapshot.afterPosition, positionTolerance))
                        {
                            Debug.Log($"시뮬레이션 적중({positionTolerance})");
                            goodSimulatedSnapshotCount++;
                            goodSimulation = true;
                            break;
                        }
                        else
                        {
                            float distance = Vector3.Magnitude(interpolated.afterPosition - serverSnapshot.afterPosition);
                            Debug.Log($"시뮬레이션 빗나감({distance} >= {positionTolerance}), 서버 위치 : {serverSnapshot.afterPosition}, 서버 시간 : {serverSnapshot.generatedTime}, 타겟 시간: {targetTime}, 보간된 위치 : {interpolated.afterPosition}, 보간점 시간 : {interpolated.generatedTime}, 위치 차이 : {(serverSnapshot.afterPosition - interpolated.afterPosition).magnitude}");
                            foundLeftIndex = i;
                            break;
                        }
                    }
                }

                if (goodSimulation)
                {
                    //Debug.Log("서버에서 보낸 스냅샷과 클라이언트 시뮬레이션 결과가 유사함, 재시뮬레이션 필요 없음");
                    continue;
                }

                if (null == interpolated)
                {
                    Debug.Log($"보간점 찾지 못함, 서버 시간 {serverSnapshot.generatedTime}, 찾으려는 시간 : {targetTime}, 첫번 째 스냅샷 시간 : {clientSnapshot[0].generatedTime}, 마지막 스냅샷 시간 : {clientSnapshot.Peek().generatedTime}, 현재 서버시간 : {NetworkManager.ServerTime.TimeAsFloat}");
                    continue;
                }

                //Debug.Log($"보간점 찾음, index : {foundIndex}, 재시뮬레이션 시작");

                tempSnapshot.Clear();
                MoveSnapshot prev = interpolated;
                for (int i = foundLeftIndex + 1; i < clientSnapshot.Count; ++i)
                {
                    transform.position = serverSnapshot.afterPosition;
                    var beforePosition = transform.position;
                    float delta = clientSnapshot[i].generatedTime - prev.generatedTime;

                    Simulate(clientSnapshot[i].moveInput, delta);

                    MoveSnapshot resimulatedSnapshot = new MoveSnapshot()
                    {
                        moveInput = clientSnapshot[i].moveInput,
                        deltaTime = delta,
                        beforePosition = beforePosition,
                        generatedTime = prev.generatedTime + delta,
                        afterPosition = transform.position
                    };
                    tempSnapshot.Add(resimulatedSnapshot);

                    prev = resimulatedSnapshot;
                }

                clientSnapshot.Clear();
                clientSnapshot.AddRange(tempSnapshot);
                if (tempSnapshot.Count == 0)
                {
                    Debug.Log($"재시뮬레이션할 스냅샷이 없음");
                    continue;
                }
                else
                {
                    Debug.Log($"서버 시간: {serverSnapshot.generatedTime}, 서버 위치: {serverSnapshot.afterPosition}, 재시뮬레이션된 최종 위치: {transform.position}, 재시뮬레이션 마지막 시간 : {tempSnapshot.Peek().generatedTime}, 지금 서버시간 : {NetworkManager.ServerTime.TimeAsFloat}");
                    //Debug.Break();
                }
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
        if (!snapshot.netObjectSnapshotById.TryGetValue(NetworkObjectId, out var objectSnapshot))
            return;

        var newServerSnapshot = new MoveSnapshot()
        {
            generatedTime = snapshot.creationTime,
            afterPosition = objectSnapshot.position,
            frameNumber = snapshot.frameNumber
        };

        // if(snapshot.creationTime > NetworkManagerExtensions.GetInstance().ServerTime.TimeAsFloat)
        // {
        //     Debug.Log($"받은 서버 스냅샷이 너무 미래에 생성된 스냅샷임, 서버 시간 : {snapshot.creationTime}, 현재 서버 시간 : {NetworkManagerExtensions.GetInstance().ServerTime.TimeAsFloat}");
        // }

        if(snapshot.doubleTime > NetworkManager.ServerTime.Time)
        {
            Debug.Log($"받은 서버 스냅샷이 너무 미래에 생성된 스냅샷임, 서버 doubleTime : {snapshot.doubleTime}, 현재 doubleTime : {NetworkManager.ServerTime.Time}");
        }

        snapshotFromServer.Add(newServerSnapshot);
        lastReceivedServerSnapshotTime = snapshot.creationTime;

        //Debug.Log($"서버에서 프레임 스냅샷 받음, 서버 시간 : {snapshot.creationTime}, 서버 위치 : {objectSnapshot.position}, 스냅샷 큐에 추가, 현재 큐 길이 : {snapshotFromServer.Count}");
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