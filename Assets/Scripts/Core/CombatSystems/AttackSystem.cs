using System;
using Core.HealthSystems;
using Input;
using Unity.Netcode;
using UnityEngine;

namespace Core.CombatSystems
{
    public class AttackSystem : NetworkBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private float attackDamage;
        [SerializeField] private float monsterAttackDamage = 50f; // Damage when player is a Monster
        [SerializeField] private Vector2 attackSize;
        [SerializeField] private Vector2 offset;
        [SerializeField] private LayerMask targetLayer;
        private NetworkVariable<bool> isAttacking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private int attackIndex = 1;

        [Space] [Header("Dependencies")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Core.MovementSystems.PlayerMovement playerMovement; // Reference to PlayerMovement
        public Action onStartAttack;

        [Header("Components")]
        [SerializeField] private Animator animator;
        
        private bool isBeingDestroyed = false;

        private Vector2 AttackCenterPosition
        {
            get
            {
                Vector2 offsetX = new Vector2(offset.x, 0);
                Vector2 offsetY = new Vector2(0, offset.y);
                bool isMonster = playerMovement != null && playerMovement.IsMonster.Value;

                // Invert offset direction for monsters to match inverted flipX
                if (isMonster)
                {
                    return spriteRenderer.flipX
                        ? (Vector2)transform.position + offsetY + offsetX // FlipX true: offset to right
                        : (Vector2)transform.position + offsetY - offsetX; // FlipX false: offset to left
                }
                else
                {
                    return spriteRenderer.flipX
                        ? (Vector2)transform.position + offsetY - offsetX // FlipX true: offset to left
                        : (Vector2)transform.position + offsetY + offsetX; // FlipX false: offset to right
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            inputReader.PrimaryAttackEvent += AttackHandler;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            inputReader.PrimaryAttackEvent -= AttackHandler;
        }
        
        public void OnEnable()
        {
            if (!IsOwner) return;
            inputReader.PrimaryAttackEvent += AttackHandler;
        }

        public void OnDisable()
        {
            if (!IsOwner) return;
            inputReader.PrimaryAttackEvent -= AttackHandler;
        }

        private void Awake()
        {
            playerMovement = GetComponent<Core.MovementSystems.PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("PlayerMovement component not found on AttackSystem!");
            }
        }

        private void AttackHandler(bool isAttackingInput)
        {
            if (this == null || !gameObject.activeSelf) return;
            if (!IsOwner || isBeingDestroyed) return;
            if (!isAttackingInput) return;
            onStartAttack?.Invoke();
            bool isMonster = playerMovement != null && playerMovement.IsMonster.Value;
            float damage = isMonster ? monsterAttackDamage : attackDamage;
            PlayAttackAnimationServerRpc(isMonster);
            AttackHandlerServerRpc(AttackCenterPosition, attackSize, damage, NetworkObjectId);
        }

        [ServerRpc]
        private void PlayAttackAnimationServerRpc(bool isMonster)
        {
            if (isBeingDestroyed || !gameObject.activeSelf) return;
            PlayAttackAnimationClientRpc(isMonster);
        }

        [ClientRpc]
        private void PlayAttackAnimationClientRpc(bool isMonster)
        {
            float resetTime;
            if (!isMonster)
            {
                attackIndex = attackIndex switch
                {
                    1 => 2,
                    2 => 1,
                    _ => attackIndex
                };

                animator.SetInteger("attackIndex", attackIndex);
                animator.SetTrigger("attack");
                animator.SetBool("isAttacking", true);
                resetTime = 0.375f;
            }
            else
            {
                animator.SetBool("isAttacking", true);
                resetTime = 1f;
            }

            if (!IsOwner) return;
            Invoke(nameof(ResetAttackServerRpc), resetTime);
        }

        [ServerRpc]
        private void ResetAttackServerRpc()
        {
            ResetAttackClientRpc();
        }

        [ClientRpc]
        private void ResetAttackClientRpc()
        {
            animator.SetBool("isAttacking", false);
            animator.SetInteger("attackIndex", 0);
            Debug.Log($"Reset attack animation: isAttacking={animator.GetBool("isAttacking")}");
        }

        public void EndAttack()
        {
            // Optional: Can be called by animation events if needed
        }

        [ServerRpc]
        private void AttackHandlerServerRpc(Vector2 damageCenter, Vector2 damageSize, float damage, ulong attackerID)
        {
            Collider2D[] targetsInAttackRange = Physics2D.OverlapBoxAll(damageCenter, damageSize, 0, targetLayer);
            foreach (Collider2D target in targetsInAttackRange)
            {
                if (!target.TryGetComponent(out HealthSystem targetHealth)) continue;
                if (targetHealth.NetworkObjectId == attackerID) continue;
                targetHealth.TakeDamage(damage, attackerID);
                Debug.Log($"Hit {target.name} : {damage} damage");
            }
        }
        
        public void MarkAsBeingDestroyed()
        {
            isBeingDestroyed = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(AttackCenterPosition, attackSize);
        }
    }
}