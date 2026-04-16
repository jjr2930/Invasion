using System;
using Jy.NetworkComponents;
using MinMax;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

[DefaultExecutionOrder(ExecutionOrders.NetworkCamera)]
public class NetworkCamera : NetworkComponent
{
    [Header("Camera static options")]
    [SerializeField] float fov;
    [SerializeField] float aspectRatio;
    [SerializeField] Vector2 screenSize;
    [SerializeField] float near;
    [SerializeField] float far;

    [Header("References")]
    [SerializeField] IngameConfig ingameConfig;
    [SerializeField] Unity.Netcode.NetworkObject followTarget;

    [Header("Camera dynamic options")]
    [SerializeField] MinMaxFloat elevationMinMax;
    [SerializeField, Range(0.1f, 10f)]
    float cameraDistance = 5f;

    [SerializeField] Vector3 lookOffset;
    [SerializeField] Vector3 positionOffset;

    [SerializeField, Range(0.0001f, 10f)] float lookSensitivity = 1f;

    [Header("internal value")]
    [SerializeField] float elevationAngle;

    private void Awake()
    {
        fov = ingameConfig.fov;
        aspectRatio = ingameConfig.aspectRatio;
        screenSize = ingameConfig.resolution;
        near = ingameConfig.near;
        far = ingameConfig.far;
    }

    private void Update()
    {
        Vector2 clientScreenCenter = this.GetScreenCenter();
        Ray ray = this.ScreenPointToRay(clientScreenCenter);


        //Debug.DrawLine(ray.origin, ray.GetPoint(10f), Color.red);
        //Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.green);
    }


    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Vector2 clientScreenCenter = this.GetScreenCenter();
        Ray ray = this.ScreenPointToRay(clientScreenCenter);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ray.origin, ray.GetPoint(10f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 10);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);

        Debug.Log($"Camera position: {transform.position}, forward: {transform.forward}, ray origin: {ray.origin}, ray direction: {ray.direction}");
    }

    private void LateUpdate()
    {
        if (null == followTarget)
            return;

        Vector3 up = Vector3.up;
        Vector3 planarTargetForward = followTarget.transform.GetPlanarFoward(up);
        Vector3 planarTargetRight = followTarget.transform.GetPlanarRight(up);

        Quaternion elevationRotation = Quaternion.AngleAxis(elevationAngle, followTarget.transform.GetPlanarRight(Vector3.up));

        //3인칭 카메라
        Vector3 offset = elevationRotation * (-planarTargetForward * cameraDistance);
        Vector3 nextPosition = followTarget.transform.position + offset + positionOffset;
        Vector3 nextLookPosition = followTarget.transform.position + lookOffset;
        Vector3 lookForward = nextLookPosition - nextPosition;
        if (lookForward.sqrMagnitude <= 0.00001f)
        {
            Debug.LogWarning("Look forward is too small, skip update rotation.");
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(lookForward, Vector3.up);
        this.transform.SetPositionAndRotation(nextPosition, lookRotation);
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (IsClient)
        {
            SetScreenSizeRpc(new Vector2(Screen.width, Screen.height));
            ClientEventBus.Networks.OnNetworkCameraSpawned?.Invoke(this);
        }

        this.gameObject.name = $"NetworkCamera_{this.OwnerClientId}";
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
    }

    public override void UnregisterClientSideListeners()
    {
        base.UnregisterClientSideListeners();
        ClientEventBus.Input.onInput -= UpdateLookInput;
    }
    public void RegisterServerSideInputListenerRpc()
    {
        Debug.Log($"Registering input listener for camera {this.NetworkObjectId} owned by client {this.OwnerClientId}");
        ServerEventBus.Input.onInputReceived += UpdateLookInput;
    }

    public void UnregisterServerSideInputListenerRpc()
    {
        Debug.Log($"Unregistering input listener for camera {this.NetworkObjectId} owned by client {this.OwnerClientId}");
        ServerEventBus.Input.onInputReceived -= UpdateLookInput;
    }

    public void UpdateLookInput(InputPacket input, ulong senderId)
    {
        if (senderId != this.OwnerClientId)
        {
            return;
        }
        var delta = input.look.y * Time.deltaTime * lookSensitivity;
        elevationAngle += delta;
        elevationAngle = elevationMinMax.Clamp(elevationAngle);
    }


    [Rpc(SendTo.Server)]
    public void SetScreenSizeRpc(Vector2 size)
    {
        screenSize = size;
        aspectRatio = screenSize.x / screenSize.y;
    }

    public Vector2 GetScreenCenter()
    {
        return new Vector2(screenSize.x, screenSize.y) * 0.5f;
    }

    /// <summary>
    /// only support perspective
    /// </summary>
    /// <param name="screenPoint"></param>
    /// <returns></returns>
    public Ray ScreenPointToRay(Vector2 screenPoint)
    {
        // -------------------------
        // 1. Screen → NDC
        // -------------------------
        float x = (screenPoint.x / screenSize.x) * 2f - 1f;
        float y = (screenPoint.y / screenSize.y) * 2f - 1f;

        // Clip space (near plane)
        Vector4 clip = new Vector4(x, y, -1f, 1f);

        // -------------------------
        // 2. Projection Matrix
        // -------------------------
        Matrix4x4 proj = Matrix4x4.Perspective(fov, aspectRatio, near, far);
        Matrix4x4 invProj = proj.inverse;

        // Clip → View
        Vector4 view = invProj * clip;
        view /= view.w;

        // -------------------------
        // 3. Camera Transform
        // -------------------------
        Matrix4x4 camToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        // View → World (direction)
        Vector3 dir = camToWorld.MultiplyVector(view).normalized;

        // -------------------------
        // 4. Ray 생성
        // -------------------------
        return new Ray(transform.position, -dir);
    }

    [Rpc(SendTo.Everyone)]
    public void SetFollowTargetRpc(ulong targetObjectId)
    {
        NetworkManagerExtensions.GetInstance().SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObj);
        Assert.IsNotNull(targetObj, $"Failed to find target object with NetworkObjectId {targetObjectId} to follow.");

        followTarget = targetObj;
    }
}
