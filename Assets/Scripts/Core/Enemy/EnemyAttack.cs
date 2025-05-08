using Core.HealthSystems;
using Unity.Netcode;
using UnityEngine;

public class EnemyAttack : NetworkBehaviour
{
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackCooldown;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Vector2 attackSize = new(1f, 1f);
    [SerializeField] private Vector2 attackOffset = new(1f, 0f);
    [SerializeField] private int attackIndex = 1;

    [Header("Dependencies")] [SerializeField]
    private SpriteRenderer spriteRenderer;

    private EnemyPathfinding enemyPathfinding;
    [SerializeField] private Animator animator;

    private NetworkVariable<bool> isAttacking =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private float lastAttackTime;
    private Vector2 baseAttackOffset;
    private bool isFlipped;

    private void Start()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        baseAttackOffset = attackOffset;
        isFlipped = spriteRenderer.flipX;
    }

    private Vector2 AttackCenterPosition => (Vector2)transform.position + (isFlipped
        ? new Vector2(-baseAttackOffset.x, baseAttackOffset.y)
        : baseAttackOffset);

    private void Update()
    {
        var currentFlip = spriteRenderer.flipX;
        if (currentFlip != isFlipped) isFlipped = currentFlip;
    }

    private void FixedUpdate()
    {
        if (!IsServer || (enemyPathfinding.target != null && Time.time < lastAttackTime + attackCooldown)) return;
        AttackHandler();
    }

    private void AttackHandler()
    {
        lastAttackTime = Time.time;
        AttackHandlerServerRpc(AttackCenterPosition, attackSize, attackDamage);
    }

    [ServerRpc]
    private void AttackHandlerServerRpc(Vector2 damageCenter, Vector2 damageSize, float damage)
    {
        var targets = Physics2D.OverlapBoxAll(damageCenter, damageSize, 0, targetLayer);

        if(targets.Length <= 0)
        {
            ResetAttackServerRpc();
        }
        else
        {
            PlayAttackAnimationServerRpc();
        }

        foreach (var target in targets)
            if (target.TryGetComponent(out HealthSystem targetHealth))
            {
                targetHealth.TakeDamage(damage);
            }
    }

    [ServerRpc]
    private void PlayAttackAnimationServerRpc()
    {
        PlayAttackAnimationClientRpc();
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        attackIndex = attackIndex == 1 ? 2 : 1;
        animator.SetInteger("attackIndex", attackIndex);
        animator.SetTrigger("attack");
        animator.SetBool("isAttacking", true);
        //Invoke(nameof(ResetAttackServerRpc), 0.49f);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var offset = spriteRenderer != null && spriteRenderer.flipX
            ? new Vector2(-attackOffset.x, attackOffset.y)
            : attackOffset;
        Gizmos.DrawWireCube((Vector2)transform.position + offset, attackSize);
    }
}