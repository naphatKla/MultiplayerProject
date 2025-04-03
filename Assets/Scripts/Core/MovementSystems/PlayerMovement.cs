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
        private NetworkVariable<bool> isMoving = new NetworkVariable<bool>();
        private NetworkVariable<bool> isRunning = new NetworkVariable<bool>();

        [Header("Components")]
        [SerializeField] private Animator animator;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            inputReader.MoveEvent += SetMoveInput;
            inputReader.MouseMoveEvent += PlayerFacingHandler;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            inputReader.MoveEvent -= SetMoveInput;
            inputReader.MouseMoveEvent -= PlayerFacingHandler;
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            MovementHandler();
        }
        
        private void SetMoveInput(Vector2 movement)
        {
            movementInput = movement;
        }
        
        private void PlayerFacingHandler(Vector2 mousePos)
        {
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 lookDirection = mousePos - (Vector2)transform.position;
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            bool shouldFaceLeft = angle > 90 || angle < -90;
            UpdateFacingServerRpc(shouldFaceLeft);
        }
        
        private void MovementHandler()
        {
            Vector2 newPos = rb.position + movementInput * (curPlayerMoveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            PlayMovingAnimationServerRpc();
        }

        [ServerRpc]
        private void PlayMovingAnimationServerRpc()
        {
            isMoving.Value = !(movementInput.magnitude <= 0);
            isRunning.Value = curPlayerMoveSpeed > 15f;
            PlayMovingAnimationClientRpc();
        }

        [ClientRpc]
        private void PlayMovingAnimationClientRpc()
        {
            if (!isMoving.Value) return;
            animator.SetBool("isRunning", isRunning.Value); // Adjust 'someThreshold' based on your speeds
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
