using System;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;


public class NetworkInput : NetworkBehaviour
{
    [SerializeField] PlayerInput spawnedInput;
    [SerializeField, Range(0.0001f, 1f)] float lookThreshold;

    [SerializeField] Vector2 look;
    [SerializeField] Vector2 move;
    [SerializeField] bool firePressed;

    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction attackAction;

    private void Update()
    {
        InputPacket inputPacket = new InputPacket
        {
            move = move,
            look = look,
            firePressed = firePressed
        };

        if(IsClient)
        {
            UpdateInputRpc(inputPacket);

            //UnityEngine.Debug.Log($"Sent input from client {OwnerClientId}: Move={inputPacket.move}, Look={inputPacket.look}, fire: {inputPacket.firePressed}");
            ClientEventBus.Input.onInput?.Invoke(inputPacket, OwnerClientId);
        }
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (!IsOwner)
        {
            spawnedInput.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;

        spawnedInput.enabled = true;

        // currentActionMap에서 액션을 찾아 C# 이벤트 구독
        lookAction = spawnedInput.currentActionMap.FindAction("Look");
        moveAction = spawnedInput.currentActionMap.FindAction("Move");
        attackAction = spawnedInput.currentActionMap.FindAction("Attack");

        Assert.IsNotNull(lookAction, "Look action not found in the current action map.");
        Assert.IsNotNull(moveAction, "Move action not found in the current action map.");
        Assert.IsNotNull(attackAction, "Attack action not found in the current action map.");


        lookAction.performed += OnLook;
        lookAction.canceled += OnLook;

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        attackAction.performed += OnAttack;
        attackAction.canceled += OnAttack;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        Assert.IsNotNull(lookAction, "Look action not found in the current action map.");
        Assert.IsNotNull(moveAction, "Move action not found in the current action map.");
        Assert.IsNotNull(attackAction, "Attack action not found in the current action map.");

        lookAction.performed -= OnLook;
        lookAction.canceled -= OnLook;

        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        attackAction.performed -= OnAttack;
        attackAction.canceled -= OnAttack;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        firePressed = context.ReadValueAsButton();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    [Rpc(SendTo.Server)]
    public void UpdateInputRpc(InputPacket input, RpcParams rpcParam = default)
    {
        //Debug.Log($"Received input from client {rpcParam.Receive.SenderClientId}: Move={input.move}, Look={input.look}, fire: {input.firePressed}");

        ServerEventBus.Input.onInputReceived?.Invoke(input, rpcParam.Receive.SenderClientId);
    }
}
