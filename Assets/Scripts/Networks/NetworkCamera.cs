using System;
using Jy.NetworkComponents;
using MinMax;
using Unity.Netcode;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Assertions;

[DefaultExecutionOrder(ExecutionOrders.NetworkCamera)]
public class NetworkCamera : NetworkComponent
{
    [Header("References")]
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Ray ray = new Ray(this.transform.position, this.transform.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ray.origin, ray.direction * 10f);
    }

    private void LateUpdate()
    {
        if(IsClient)
            return;

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
            ClientEventBus.Networks.OnNetworkCameraSpawned?.Invoke(this);

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

    public void UpdateLookInput(InputPacket input, ulong senderId)
    {
        if(IsClient)
            return;

        if (senderId != this.OwnerClientId)
        {
            return;
        }
        var delta = input.look.y * Time.deltaTime * lookSensitivity;
        elevationAngle += delta;
        elevationAngle = elevationMinMax.Clamp(elevationAngle);
    }

    [Rpc(SendTo.Everyone)]
    public void SetFollowTargetRpc(ulong targetObjectId)
    {
        NetworkManagerExtensions.GetInstance().SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObj);
        Assert.IsNotNull(targetObj, $"Failed to find target object with NetworkObjectId {targetObjectId} to follow.");

        followTarget = targetObj;
    }

    public Ray GetFireRay()
    {
        return new Ray(transform.position, transform.forward);
    }
}
