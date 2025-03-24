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
        [SerializeField] private Vector2 attackSize;
        [SerializeField] private Vector2 offset;
        [SerializeField] private LayerMask targetLayer;
        private NetworkVariable<bool> isAttacking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private int attackIndex = 1;

        [Space] [Header("Dependencies")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        public Action onStartAttack;

        [Header("Components")]
        [SerializeField] private Animator animator;

        private Vector2 AttackCenterPosition
        {
            get
            {
                Vector2 offsetX = new Vector2(offset.x,0);
                Vector2 offsetY = new Vector2(0, offset.y);

                if (spriteRenderer.flipX)
                {
                    return (Vector2)transform.position + offsetY - offsetX;
                }
                
                return (Vector2)transform.position + offsetY + offsetX;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            inputReader.PrimaryAttackEvent += AttackHandler;
        }
        
        private void AttackHandler(bool isAttackingInput)
        {
            if (!IsOwner) return;
            if (!isAttackingInput) return;
            //if (isAttacking.Value) return;

            PlayAttackAnimationServerRpc();
            AttackHandlerServerRpc(AttackCenterPosition, attackSize, attackDamage, NetworkObjectId);
        }

        [ServerRpc]
        private void PlayAttackAnimationServerRpc()
        {
            PlayAttackAnimationClientRpc();
        }

        [ClientRpc]
        private void PlayAttackAnimationClientRpc()
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

            if (!IsOwner) return;
            Invoke("ResetAttackServerRpc", 0.375f);
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
        }

        public void EndAttack()
        {
            //animator.SetInteger("attackIndex", 0);
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(AttackCenterPosition, attackSize);
        }
    }
}
