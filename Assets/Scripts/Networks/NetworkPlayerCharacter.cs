using System;
using Invasion;
using Invasion.Tables.Runtime;
using Jy.NetworkComponents;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class NetworkPlayerCharacter : NetworkComponent
{
    [Serializable]
    public class MaterialContainer
    {
        public Material localMaterial;
        public Material remoteMaterial;
    }


    [Header("References")]
    [SerializeField] MaterialContainer logicMaterial;
    [SerializeField] MaterialContainer visualMaterial;

    [SerializeField] MeshRenderer meshRenderer;

    [SerializeField] CharacterController cc;
    [SerializeField] PlayerCharacterStatTable statTable;
    [SerializeField] int statTableKey;
    [SerializeField] PlayerCharacterStat stat;
    [SerializeField] GameObject visualPrefab;
    [SerializeField] Transform visualInstance;
    [SerializeField] IngameConfig config;

    [Header("Thresholds")]
    [SerializeField, Range(0.0001f, 1f)]
    float lookThreshold = 0.1f;
    [SerializeField, Range(0.0001f, 10f)]
    float rotateSpeed = 1f;

    [Header("Current stat")]
    [SerializeField] int currentHealth;

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

        if (!IsServer)
            return;

        transform.Rotate(transform.up, currentInput.look.x * Time.deltaTime * rotateSpeed);

        Vector3 moveDirection = new Vector3(currentInput.move.x, 0f, currentInput.move.y);
        moveDirection = this.transform.rotation * moveDirection;
        Vector3 nextVelocity = moveDirection * config.moveSpeed;
        cc.Move(nextVelocity * Time.deltaTime);
    }

    public void LateUpdate()
    {
        if (!IsClient)
            return;

        Vector3 nextPosition = positionLerper.Lerp(visualInstance.position, this.transform.position);
        Quaternion nextRotation = rotationLerper.Lerp(visualInstance.rotation, this.transform.rotation);

        visualInstance.SetPositionAndRotation(nextPosition, nextRotation);
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        currentInput = new InputPacket();

        Material selectedVisualMaterial = null;
        if (this.IsOwnedByLocalPlayer())
        {
            meshRenderer.material = logicMaterial.localMaterial;
            ClientEventBus.Networks.OnLocalPlayerCharacterSpawned?.Invoke(this);

            selectedVisualMaterial = visualMaterial.localMaterial;
        }
        else
        {
            meshRenderer.material = logicMaterial.remoteMaterial;

            selectedVisualMaterial = visualMaterial.remoteMaterial;
        }

        if (IsClient)
        {
            visualInstance = Instantiate(visualPrefab, this.transform.position, this.transform.rotation).transform;
            visualInstance.name = $"NetworkPlayer Visual {OwnerClientId}";

            var visualRenderer = visualInstance.GetComponent<Renderer>();
            visualRenderer.material = selectedVisualMaterial;
        }

        currentHealth = stat.health;
        gameObject.name = $"NetworkPlayer Logic {OwnerClientId}";
    }

    public override void RegisterServerSideListeners()
    {
        base.RegisterServerSideListeners();

        ServerEventBus.Input.onInputReceived += OnInputReceived;
    }

    public override void UnregisterServerSideListeners()
    {
        base.UnregisterServerSideListeners();

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
        Debug.Log($"Player {OwnerClientId} hit with damage {damage}. Current health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Handle death, e.g., respawn or disable character
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        IAttackable attackable = other.GetComponent<IAttackable>();
        Assert.IsNotNull(attackable, $"Collided object {other.gameObject.name} does not implement IAttackable.");

        int damage = attackable.GetDamage();
        Hit(damage);
    }

    public void SetTableKey(int key)
    {
        statTableKey = key;
        stat = statTable.Get<PlayerCharacterStat>(statTableKey);
    }
}
