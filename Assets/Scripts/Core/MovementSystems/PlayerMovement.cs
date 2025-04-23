using System;
using Input;
using Unity.Netcode;
using UnityEngine;

namespace Core.MovementSystems
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private float curPlayerMoveSpeed;
        [Space] [Header("Dependencies")] [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Vector2 movementInput;
        //private NetworkVariable<bool> isMoving = new NetworkVariable<bool>();
        private NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false,
                                          NetworkVariableReadPermission.Everyone,
                                           NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> isRunning = new NetworkVariable<bool>();
        [SerializeField] private FieldOfView fieldOfView;
        [SerializeField] private Transform origin;

        [Header("Components")]
        [SerializeField] private Animator animator;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            inputReader.MoveEvent += SetMoveInput;
            inputReader.MouseMoveEvent += PlayerFacingHandler;

            //isMoving.OnValueChanged += OnIsMovingChanged;
            //isRunning.OnValueChanged += OnIsRunningChanged;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            inputReader.MoveEvent -= SetMoveInput;
            inputReader.MouseMoveEvent -= PlayerFacingHandler;

            //isMoving.OnValueChanged -= OnIsMovingChanged;
            //isRunning.OnValueChanged -= OnIsRunningChanged;
        }

        /*private void OnIsMovingChanged(bool previousValue, bool newValue)
        {
            animator.SetBool("isMoving", newValue);
        }

        private void OnIsRunningChanged(bool previousValue, bool newValue)
        {
            animator.SetBool("isRunning", newValue);
        }*/

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            MovementHandler();
        }

        public void SetFOV(FieldOfView fov)
        {
            if (!IsOwner) return;
            fieldOfView = fov;
        }
        
        private void SetMoveInput(Vector2 movement)
        {
            movementInput = movement;
        }
        
        private void PlayerFacingHandler(Vector2 mousePos)
        {
            if (!IsOwner) return;

            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 lookDirection = mousePos - (Vector2)transform.position;
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            bool shouldFaceLeft = angle > 90 || angle < -90;
            UpdateFacingServerRpc(shouldFaceLeft);
        }
        
        private void MovementHandler()
        {
            if (!IsOwner) return;

            Vector2 newPos = rb.position + movementInput * (curPlayerMoveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            isMoving.Value = !(movementInput.magnitude <= 0);
            isRunning.Value = curPlayerMoveSpeed > 15f;
            PlayMovingAnimationServerRpc(isMoving.Value, isRunning.Value);
            
            fieldOfView.SetOrigin(origin.position);
            Vector3 targetPoisition = GetMouseInWorldPosition();
            Vector3 aimDir = (targetPoisition - transform.position).normalized;
            fieldOfView.SetAimDirection(aimDir);
        }
        
        public Vector3 GetMouseInWorldPosition()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(inputReader.MousePos);
            worldPosition.z = 0f;
            return worldPosition;
        }

        [ServerRpc]
        private void PlayMovingAnimationServerRpc(bool isMoving, bool isRunning)
        {
            PlayMovingAnimationClientRpc(isMoving, isRunning);
        }

        [ClientRpc]
        private void PlayMovingAnimationClientRpc(bool isMoving, bool isRunning)
        {
            //if (!isMoving.Value) return;

            animator.SetBool("isMoving", isMoving); // Adjust 'someThreshold' based on your speeds
            animator.SetBool("isRunning", isRunning);
        }

        [ServerRpc]
        private void UpdateFacingServerRpc(bool shouldFaceLeft)
        {
            UpdateFacingClientRpc(shouldFaceLeft);
        }
        
        [ClientRpc]
        private void UpdateFacingClientRpc(bool shouldFaceLeft)
        {
            spriteRenderer.flipX = shouldFaceLeft;
        }
    }
}
