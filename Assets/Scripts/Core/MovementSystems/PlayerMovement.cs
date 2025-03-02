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
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Vector2 movementInput;
        public override void OnNetworkSpawn()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (!IsOwner) return;
            inputReader.MoveEvent += SetMoveInput;
            inputReader.MouseMoveEvent += PlayerFacingHandler;
        }

        private void Update()
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
            Vector2 newPos = rb.position + movementInput * (curPlayerMoveSpeed * Time.deltaTime);
            rb.MovePosition(newPos);
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
