using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenSpaceCoordinatesTest : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;

    private void Update()
    {
        if (playerInput.currentActionMap.name != "UI")
            return;

        Vector2 mousePosition = playerInput.currentActionMap.FindAction("MousePosition").ReadValue<Vector2>();
        Debug.Log($"Mouse Position: {mousePosition}");
    }
    
    
}
