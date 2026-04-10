using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class NetworkPlayerCharacter : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] Material localMaterial;
    [SerializeField] Material remoteMaterial;
    [SerializeField] MeshRenderer meshRenderer;

    [SerializeField] CharacterController cc;
    [SerializeField] PlayerCharacterStat stat;
    [SerializeField] IngameConfig config;

    [Header("Thresholds")]
    [SerializeField, Range(0.0001f, 1f)]
    float lookThreshold = 0.1f;
    [SerializeField, Range(0.0001f, 10f)]
    float lookSensitivity = 1f;

    [Header("Current stat")]
    [SerializeField] int currentHealth;
    [SerializeField] float moveSpeed;

    [Header("Interpolation")]
    [SerializeField] PositionLerper positionLerper;
    [SerializeField] RotationLerper rotationLerper;
    [SerializeField] Vector3 lastPosition;
    [SerializeField] Quaternion lastRotation;

    InputPacket currentInput = new InputPacket();

    public void Update()
    {
        if (!IsSpawned)
            return;

        //Debug.Log($"current delta time : {Time.deltaTime}");

        if (IsOwner || IsServer)
        {

            //if (Mathf.Abs(currentInput.look.x) >= lookThreshold)
            //{
            //    transform.Rotate(Vector3.up, currentInput.look.x * Time.deltaTime * lookSensitivity);
            //}

            Vector3 moveDirection = new Vector3(currentInput.move.x, 0f, currentInput.move.y);
            //Debug.Log($"Received input from client {senderId}, move: {moveDirection}, look: {look}");
            moveDirection = this.transform.rotation * moveDirection;
            Vector3 nextVelocity = moveDirection * config.moveSpeed;
            cc.Move(nextVelocity * Time.deltaTime);
        }
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        currentInput = new InputPacket();

        if (this.IsOwnedByLocalPlayer())
        {
            meshRenderer.material = localMaterial;
            ClientEventBus.Networks.OnLocalPlayerCharacterSpawned?.Invoke(this);
        }
        else
        {
            meshRenderer.material = remoteMaterial;
        }

        gameObject.name = $"NetworkPlayerCharacter_{OwnerClientId}";

        if (IsServer)
            RegisterServcerSideListeners();

        if (IsClient)
            RegisterClientSideListeners();
    }


    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
            UnregisterServerSideListeners();

        if (IsClient)
            UnregisterClientSideListeners();
    }

    private void RegisterClientSideListeners()
    {
        ClientEventBus.Input.onInput += OnInputReceived;
    }

    private void UnregisterClientSideListeners()
    {
        ClientEventBus.Input.onInput -= OnInputReceived;
    }

    public void RegisterServcerSideListeners()
    {
        ServerEventBus.Input.onInputReceived += OnInputReceived;
    }

    public void UnregisterServerSideListeners()
    {
        ServerEventBus.Input.onInputReceived -= OnInputReceived;
    }

    public void OnInputReceived(InputPacket input, ulong senderId)
    {
        if (senderId != this.OwnerClientId)
        {
            return;
        }

        //Debug.Log("Received input from client " + senderId + ": move: " + input.move + ", look: " + input.look);
        currentInput = input;
    }


    [Rpc(SendTo.Everyone)]
    public void SetInitialPositionRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        if (cc != null)
        {
            cc.enabled = false;
            cc.enabled = true;
        }
    }

    public void Hit(int damage)
    {
        Assert.IsTrue(damage >= 0, "Damage cannot be negative. Damage value: " + damage);
        Assert.IsTrue(IsServer, "Hit method can only be called on the server.");

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Handle death, e.g., respawn or disable character
        }
    }
}
