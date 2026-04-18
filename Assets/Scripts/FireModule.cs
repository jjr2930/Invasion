using System;
using Invasion.Tables.Runtime;
using Jy.NetworkComponents;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class FireModule : NetworkComponent
{
    [SerializeField, Range(0.001f, 1f)]
    float fireDelay;

    [SerializeField] bool firePressed;
    [SerializeField] float nextFiredTime;
    [SerializeField] NetworkCamera networkCamera;
    [SerializeField] PlayerCharacterStatTable stat;
    [SerializeField] int tableKey;
    [SerializeField] LayerMask hitlayerMask;
    [SerializeField] NetworkObject hitPointVisual;

    private void Update()
    {
        if (IsServer)
            FireIfNeed();
    }

    void FireIfNeed()
    {
        if (!firePressed)
        {
            //Debug.Log("Fire button is not pressed for client id: " + OwnerClientId);
            return;
        }

        float currentTime = Time.time;
        if (currentTime < nextFiredTime)
            return;

        //Debug.Log("Fire");

        nextFiredTime = currentTime + fireDelay;

        Assert.IsNotNull(networkCamera, "Owner camera is null for client id: " + OwnerClientId);

        Ray ray = networkCamera.GetFireRay();

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, hitlayerMask.value))
        {
            Debug.Log("Hit object: " + hit.transform.name + " at position: " + hit.point);
            var instance = Instantiate(hitPointVisual, position: hit.point, rotation: Quaternion.identity);
            instance.Spawn();

            //NetworkPlayerCharacter target = hit.transform.GetComponent<NetworkPlayerCharacter>();
            //Assert.IsNotNull(target, "Hit object does not have NetworkPlayerCharacter component. Hit object: " + hit.transform.name);

            //target.Hit(stat.damage);
        }
    }

    public override void RegisterServerSideListeners()
    {
        base.RegisterServerSideListeners();

        ServerEventBus.Spawning.onNetworkCameraSpawned += OnNetworkCameraSpawned;
        ServerEventBus.Input.onInputReceived += OnInputReceived;
    }

    public override void UnregisterServerSideListeners()
    {
        base.UnregisterServerSideListeners();

        ServerEventBus.Spawning.onNetworkCameraSpawned -= OnNetworkCameraSpawned;
        ServerEventBus.Input.onInputReceived -= OnInputReceived;
    }

    private void OnNetworkCameraSpawned(NetworkCamera camera)
    {
        if (camera.OwnerClientId == OwnerClientId)
        {
            networkCamera = camera;
        }
    }

    private void OnInputReceived(InputPacket input, ulong senderId)
    {
        if (senderId != this.OwnerClientId)
            return;

        firePressed = input.firePressed;
    }
}
