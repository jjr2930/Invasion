using System;
using System.Collections.Generic;
using System.Reflection;
using Extensions;
using Jy.NetworkComponents;
using Jy.Packets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Rendering;

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
    [SerializeField, Range(0f, 3f)] float targetTimeServerFrameTolerance = 2;
    [SerializeField, Range(0f, 3f)] float targetTimeClientFrameTolerance = 2;
    [SerializeField] IngameConfig ingameConfig;
    [SerializeField, Range(-20, 20)] int timeCorrectionClientFrame = 0;

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

        //Debug.Log($"서버시간 : {NetworkManager.ServerTime.Time}, 로컬시간 : {NetworkManager.LocalTime.Time}, 서버시간과의 차이 : {NetworkManager.ServerTime.Time - NetworkManager.LocalTime.Time}");

        Simulate(moveInput, Time.deltaTime);

        if (this.IsOwnedByLocalPlayer())
        {
            //Debug.Log("유저가 소유중 스냅샷 만들기와, 보정 작업 시작");
            ///clientSnapshot.RemoveAll(s => s.generatedTime < NetworkManager.LocalTime.TimeAsFloat - 1f);

            MoveSnapshot newSnapshot = new MoveSnapshot();
            newSnapshot.deltaTime = Time.deltaTime;
            newSnapshot.moveInput = moveInput;
            newSnapshot.generatedTime = NetworkManager.LocalTime.TimeAsFloat;
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

                //Debug.Log("serverOneFrameDuration : " + serverOneFrameDuration + ", clientOneFrameDuration : " + clientOneFrameDuration);

                //테스트용 RTT, 실제론 네트워크에서 측정된 RTT값을 사용해야함
                UnityTransport transport = (UnityTransport)NetworkManager.NetworkConfig.NetworkTransport;
                long rtt = (long)transport.GetCurrentRtt(NetworkManager.ServerClientId);
                float rttInSec = rtt * 0.001f;
                float halfRtt = rttInSec * 0.5f;
                float positionTolerance = movingSpeed * clientOneFrameDuration * positionToleranceMultiplier;
                float targetTime = 0f;

                for (int i = 0; i < clientSnapshot.Count - 1; i++)
                {
                    MoveSnapshot clientLeftSnapshot = clientSnapshot[i];
                    MoveSnapshot clientRightSnapshot = clientSnapshot[i + 1];
                    targetTime = NetworkManager.ServerTime.TimeAsFloat;
                    targetTime += timeCorrectionClientFrame * clientOneFrameDuration;
                    //targetTime -= halfRtt;

                    // targetTime = serverSnapshot.generatedTime - halfRttInSec;
                    // targetTime -= serverOneFrameDuration * targetTimeServerFrameTolerance;
                    // targetTime -= clientOneFrameDuration * targetTimeClientFrameTolerance;
                    // targetTime += clientOneFrameDuration * (float)timeCorrectionClientFrame;

                    if (clientLeftSnapshot.generatedTime <= targetTime && targetTime <= clientRightSnapshot.generatedTime)
                    {
                        float rate = (targetTime - clientLeftSnapshot.generatedTime) / (float)(clientRightSnapshot.generatedTime - clientLeftSnapshot.generatedTime);
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
                            Debug.Log($"시뮬레이션 빗나감({distance} >= {positionTolerance}), 서버 위치 : {serverSnapshot.afterPosition}, 서버 시간 : {serverSnapshot.generatedTime}, 타겟 시간: {targetTime}, 보간된 위치 : {interpolated.afterPosition}, 보간점 시간 : {interpolated.generatedTime}, 스냅샷 프레임 ; {serverSnapshot.frameNumber}");
                            foundLeftIndex = i;
                            // Debug.Break();
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
                    Debug.Log($"보간점 찾지 못함, framecount : {serverSnapshot.frameNumber}, 서버 에서 생성 시간 {serverSnapshot.generatedTime}, 찾으려는 시간 : {targetTime}, 첫번 째 스냅샷 시간 : {clientSnapshot[0].generatedTime}, 마지막 스냅샷 시간 : {clientSnapshot.Peek().generatedTime}, 현재 로컬시간 : {NetworkManager.LocalTime.TimeAsFloat}, halfRtt : {halfRtt}, rtt : {rttInSec}");
                    // Debug.Break();
                    continue;
                }

                //Debug.Log($"보간점 찾음, index : {foundIndex}, 재시뮬레이션 시작");

                tempSnapshot.Clear();
                MoveSnapshot prev = interpolated;
                //Debug.Log($"재시뮬레이션 시작 : frameNumber:{serverSnapshot.frameNumber}, serverSnapshotCreationTime:{serverSnapshot.generatedTime}, 위치 : {serverSnapshot.afterPosition}");
                for (int i = foundLeftIndex + 1; i < clientSnapshot.Count; ++i)
                {
                    transform.position = serverSnapshot.afterPosition;
                    float delta = clientSnapshot[i].generatedTime - prev.generatedTime;

                    Simulate(clientSnapshot[i].moveInput, delta);

                    MoveSnapshot resimulatedSnapshot = new MoveSnapshot()
                    {
                        moveInput = clientSnapshot[i].moveInput,
                        deltaTime = delta,
                        generatedTime = prev.generatedTime + delta,
                        afterPosition = transform.position
                    };
                    tempSnapshot.Add(resimulatedSnapshot);
                    //Debug.Log($"재시뮬레이션 스텝 delta : {delta}, clientMoveInput : {clientSnapshot[i].moveInput}, 재시뮬레이션 결과 위치 : {resimulatedSnapshot.afterPosition}, 서버 스냅샷 위치 : {serverSnapshot.afterPosition}, deltaTime : {delta}, generatedTime : {resimulatedSnapshot.generatedTime}");

                    prev = resimulatedSnapshot;
                }
                //Debug.Log("시뮬레이션 끝");

                clientSnapshot.Clear();
                clientSnapshot.AddRange(tempSnapshot);
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
            frameNumber = snapshot.frameNumber,
        };

        snapshotFromServer.Add(newServerSnapshot);
        lastReceivedServerSnapshotTime = snapshot.creationTime;

        // var transport = (UnityTransport)NetworkManager.NetworkConfig.NetworkTransport;
        // float rttMs = transport.GetCurrentRtt(NetworkManager.ServerClientId);
        // float halftRtt = rttMs * 0.001f * 0.5f;
        // long clientSideMs = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        // Debug.LogFormat("서버에서 프레임 스냅샷 받음, framenumber : {0}, serverms : {1}, clientside ms : {2}, rttms : {3}, halfRtt : {4}, 클라 서버 차이 : {5}, creationTime : {6}, clientSideServerTime : {7}, clientSideLocalTime : {8}",
        //         newServerSnapshot.frameNumber, snapshot.ms, clientSideMs, rttMs, halftRtt, clientSideMs - snapshot.ms, snapshot.creationTime, NetworkManager.ServerTime.TimeAsFloat, NetworkManager.LocalTime.TimeAsFloat);

    }

    private void UpdateLookInput(InputPacket packet, ulong senderId)
    {
        if (senderId != NetworkObject.OwnerClientId)
        {
            return;
        }

        moveInput = packet.move;
    }

    string ToDate(long ticks)
    {
        DateTime dateTime = new DateTime(ticks);
        return dateTime.ToString("HH:mm:ss.fff");
    }
}