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
        
        [Space] [Header("Dependencies")]
        [SerializeField] private SpriteRenderer spriteRenderer;

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
        
        private void AttackHandler(bool isAttacking)
        {
            if (!IsOwner) return;
            if (!isAttacking) return;
            
            AttackHandlerServerRpc(AttackCenterPosition, attackSize, attackDamage, NetworkObjectId);
        }

        [ServerRpc]
        private void AttackHandlerServerRpc(Vector2 damageCenter, Vector2 damageSize, float damage, ulong attackerID)
        {
            Debug.Log($"{attackerID} was attacked");
            Collider2D[] targetsInAttackRange = Physics2D.OverlapBoxAll(damageCenter, damageSize, 0, targetLayer);
            foreach (Collider2D target in targetsInAttackRange)
            {
                if (!target.TryGetComponent(out HealthSystem targetHealth)) continue;
                if (targetHealth.NetworkObjectId == attackerID) continue;
                targetHealth.TakeDamage(damage);
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
