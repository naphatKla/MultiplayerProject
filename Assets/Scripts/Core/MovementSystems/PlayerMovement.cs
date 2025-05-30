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
        private NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false,
                                          NetworkVariableReadPermission.Everyone,
                                          NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false,
                                          NetworkVariableReadPermission.Everyone,
                                          NetworkVariableWritePermission.Owner);
        [SerializeField] private FieldOfView fieldOfView;
        [SerializeField] private Transform origin;
        [SerializeField] private NetworkVariable<bool> isMonster = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private float footstepTimer = 0f;
        [SerializeField] private float footstepInterval = 0.4f; 
        
        public NetworkVariable<bool> IsMonster { get { return isMonster;} set { isMonster = value; } }
        

        [Header("Components")]
        [SerializeField] private Animator animator;

        // Default scale and position for player
        private Vector3 defaultScale;
        private Vector3 defaultPositionOffset = Vector3.zero;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            inputReader.MoveEvent += SetMoveInput;
            inputReader.MouseMoveEvent += PlayerFacingHandler;
            defaultScale = origin.localScale;
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

        public void TransformToMonster(bool isMonster)
        {
            if (!IsOwner) return;

            IsMonster.Value = isMonster;
            if (isMonster)
            {
                // Transform to monster
                Vector3 newScale = new Vector3(4f, 4f, 1f);
                TransformToMonsterServerRpc(newScale);
            }
            else
            {
                // Revert to player
                Vector3 newScale = defaultScale;
                TransformToMonsterServerRpc(newScale);
            }
        }

        [ServerRpc]
        private void TransformToMonsterServerRpc(Vector3 newScale)
        {
            origin.localScale = newScale;
            TransformToMonsterClientRpc(newScale);
        }

        [ClientRpc]
        private void TransformToMonsterClientRpc(Vector3 newScale)
        {
            origin.localScale = newScale;
        }
        
        private void MovementHandler()
        {
            if (!IsOwner) return;

            Vector2 newPos = rb.position + movementInput * (curPlayerMoveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            isMoving.Value = !(movementInput.magnitude <= 0);
            isRunning.Value = curPlayerMoveSpeed > 15f;
            PlayMovingAnimationServerRpc(isMoving.Value, isRunning.Value, isMonster.Value);
            
            fieldOfView.SetOrigin(origin.position);
            Vector3 targetPoisition = GetMouseInWorldPosition();
            Vector3 aimDir = (targetPoisition - transform.position).normalized;
            fieldOfView.SetAimDirection(aimDir);
            
            if (movementInput.magnitude > 0.1f)
            {
                footstepTimer -= Time.fixedDeltaTime;
                if (footstepTimer <= 0f)
                {
                    SoundEffectManager.Instance.PlayLocal("Walk", 1f);
                    footstepTimer = footstepInterval;
                }
            }
            else
            {
                footstepTimer = 0f;
            }
        }
        
        public Vector3 GetMouseInWorldPosition()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(inputReader.MousePos);
            worldPosition.z = 0f;
            return worldPosition;
        }

        [ServerRpc]
        private void PlayMovingAnimationServerRpc(bool isMoving, bool isRunning, bool isMonster)
        {
            PlayMovingAnimationClientRpc(isMoving, isRunning, isMonster);
        }

        [ClientRpc]
        private void PlayMovingAnimationClientRpc(bool isMoving, bool isRunning, bool isMonster)
        {
            animator.SetBool("isMoving", isMoving);
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isMonster", isMonster);
        }

        [ServerRpc]
        private void UpdateFacingServerRpc(bool shouldFaceLeft)
        {
            if (isMonster.Value)
            {
                UpdateFacingClientRpc(!shouldFaceLeft);
            }
            else
            {
                UpdateFacingClientRpc(shouldFaceLeft);
            }
        }
        
        [ClientRpc]
        private void UpdateFacingClientRpc(bool shouldFaceLeft)
        {
            spriteRenderer.flipX = shouldFaceLeft;
        }
    }
}