using Jy.NetworkComponents;
using UnityEngine;

public class NetworkCharacterController : NetworkComponent
{
    [SerializeField] CharacterController characterController;

    protected override void Reset()
    {
        base.Reset();
        characterController = GetComponent<CharacterController>();
    }

    public override void RegisterServerSideListeners()
    {
    }
}