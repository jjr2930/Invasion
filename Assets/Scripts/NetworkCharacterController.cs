using System;
using UnityEngine;

namespace Jy.Assets.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class NetworkCharacterController : MonoBehaviour
    {
        [SerializeField] CharacterController cc;
        [SerializeField] Vector2 movingInput;
        [SerializeField] float movingInputEpsilon;

        [SerializeField] Vector2 rotationInput;

        [SerializeField, Range(0.1f, 10f)] float movingSpeed = 5f;

        private void Reset()
        {
            cc = GetComponent<CharacterController>();
        }

        public void Update()
        {
            UpdateMoving();
            UpdateRotation();
        }

        private void UpdateMoving()
        {
            if (movingInput.sqrMagnitude <= movingInputEpsilon * movingInputEpsilon)
            {
                cc.Move(Physics.gravity * Time.deltaTime);
                return;
            }

            Vector3 right = transform.right * movingInput.x;
            Vector3 forward = transform.forward * movingInput.y;
            Vector3 movingDirection = right + forward;
            movingDirection.Normalize();

            Vector3 velocity = forward * movingSpeed + Physics.gravity * Time.deltaTime;

            cc.Move(velocity);
        }

        private void UpdateRotation()
        {
            throw new NotImplementedException();
        }
    }
}
