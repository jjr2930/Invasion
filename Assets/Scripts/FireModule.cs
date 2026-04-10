using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class FireModule : NetworkBehaviour
{
    [SerializeField, Range(0.001f, 1f)]
    float fireDelay;

    [SerializeField] bool firePressed;
    [SerializeField] float nextFiredTime;
    [SerializeField] NetworkCamera ownerCamera;
    [SerializeField] PlayerCharacterStat stat;
    [SerializeField] LayerMask hitlayerMask;

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (IsServer)
        {
            RegisterListenersRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            UnregisterListenersRpc();
        }
    }

    private void Update()
    {
        if (IsServer)
            UpdateServerRpc();
    }

    [Rpc(SendTo.Server)]
    void UpdateServerRpc()
    {
        if (!firePressed)
            return;

        float currentTime = Time.time;
        if (currentTime < nextFiredTime)
            return;

        nextFiredTime = currentTime + fireDelay;

        Assert.IsNotNull(ownerCamera, "Owner camera is null for client id: " + OwnerClientId);

        Vector2 clientScreenCenter = ownerCamera.GetScreenCenter();
        Ray ray = ownerCamera.ScreenPointToRay(clientScreenCenter);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, hitlayerMask.value))
        {
            NetworkPlayerCharacter target = hit.transform.GetComponent<NetworkPlayerCharacter>();
            Assert.IsNotNull(target, "Hit object does not have NetworkPlayerCharacter component. Hit object: " + hit.transform.name);

            target.Hit(stat.damage);
        }
    }

    [Rpc(SendTo.Server)]
    private void RegisterListenersRpc()
    {
        ServerEventBus.Spawning.onNetworkCameraSpawned += OnNetworkCameraSpawned;
        ServerEventBus.Input.onInputReceived += OnInputReceived;
    }

    [Rpc(SendTo.Server)]
    private void UnregisterListenersRpc()
    {
        ServerEventBus.Spawning.onNetworkCameraSpawned -= OnNetworkCameraSpawned;
        ServerEventBus.Input.onInputReceived -= OnInputReceived;
    }

    private void OnNetworkCameraSpawned(NetworkCamera camera)
    {
        if (camera.OwnerClientId == OwnerClientId)
        {
            ownerCamera = camera;
        }
    }

    private void OnInputReceived(InputPacket input, ulong senderId)
    {
        if (senderId != this.OwnerClientId)
            return;

        firePressed = input.firePressed;
    }
}
