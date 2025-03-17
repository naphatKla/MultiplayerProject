using System;
using Core.HealthSystems;
using Feedbacks;
using Input;
using Unity.Netcode;
using UnityEngine;

public class EnemyAttack : NetworkBehaviour
{
        [SerializeField] private float attackDamage;
        [SerializeField] private float attackCooldown;
        [SerializeField] private LayerMask targetLayer;
        private NetworkVariable<bool> isAttacking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private int attackIndex = 1;

        [Space] [Header("Dependencies")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform attackPoint;
        public Action onStartAttack;

        [Header("Components")]
        [SerializeField] private Animator animator;
        
        private float lastAttackTime;
        private EnemyPathfinding enemyPathfinding;
        private Vector2 baseAttackPointOffset;
        private bool lastFlipXState;

        private void Start()
        {
            enemyPathfinding = GetComponent<EnemyPathfinding>();
            baseAttackPointOffset = attackPoint.localPosition;
            lastFlipXState = spriteRenderer.flipX;
        }
        
        private Vector2 AttackCenterPosition
        {
            get
            {
                return attackPoint.position;
            }
        }
        
        private void Update()
        {
            if (spriteRenderer.flipX != lastFlipXState)
            {
                UpdateAttackPointPosition();
                lastFlipXState = spriteRenderer.flipX;
            }
        }
        
        private void UpdateAttackPointPosition()
        {
            Vector3 newLocalPosition = baseAttackPointOffset;
            if (spriteRenderer.flipX)
            {
                newLocalPosition.x = -baseAttackPointOffset.x;
            }
            attackPoint.localPosition = newLocalPosition;
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackHandler();
            }
        }

        private void AttackHandler()
        {
            lastAttackTime = Time.time;
            /*PlayAttackAnimationServerRpc();*/
            Vector2 attackScaleSize = new Vector2(
                Mathf.Abs(attackPoint.localScale.x),
                Mathf.Abs(attackPoint.localScale.y)
            );
            AttackHandlerServerRpc(AttackCenterPosition, attackScaleSize, attackDamage);
        }

        [ServerRpc]
        private void PlayAttackAnimationServerRpc()
        {
            PlayAttackAnimationClientRpc();
        }

        [ClientRpc]
        private void PlayAttackAnimationClientRpc()
        {
            if (attackIndex == 1)
            {
                attackIndex = 2;
            }
            else if (attackIndex == 2)
            {
                attackIndex = 1;
            }

            animator?.SetInteger("attackIndex", attackIndex);
            animator?.SetTrigger("attack");
            animator?.SetBool("isAttacking", true);
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
        private void AttackHandlerServerRpc(Vector2 damageCenter, Vector2 damageSize, float damage)
        {
            Collider2D[] targetsInAttackRange = Physics2D.OverlapBoxAll(damageCenter, damageSize, 0, targetLayer);
            foreach (Collider2D target in targetsInAttackRange)
            {
                if (!target.TryGetComponent(out HealthSystem targetHealth)) continue;
                targetHealth.TakeDamage(damage);
                Debug.Log($"Hit {target.name} at {target.transform.position} : {damage} damage");
            }
        }

        private void OnDrawGizmos()
        {
            if (attackPoint != null)
            {
                Vector2 gizmoSize = new Vector2(
                    Mathf.Abs(attackPoint.localScale.x),
                    Mathf.Abs(attackPoint.localScale.y)
                );
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(AttackCenterPosition, gizmoSize);
            }
        }
    }