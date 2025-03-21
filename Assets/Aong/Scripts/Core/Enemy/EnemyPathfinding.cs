using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPathfinding : NetworkBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] public Transform target;
    public float detectionRadius = 5f;
    public LayerMask playerLayer;
    private Vector2 startPosition;
    private Vector2 lastSentPosition;
    private readonly float positionThreshold = 0.1f;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) Debug.Log("Enemy Spawned on Server");
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            DetectPlayer();
            if (target != null) EnemyFacingHandler(target.position);

            if (target != null)
                agent.SetDestination(target.position);
            else
                agent.SetDestination(startPosition);

            if (Vector2.Distance(transform.position, lastSentPosition) > positionThreshold)
            {
                lastSentPosition = transform.position;
                UpdatePositionClientRpc(transform.position);
            }
        }
    }

    [ClientRpc]
    private void UpdatePositionClientRpc(Vector2 newPosition)
    {
        if (!IsServer) transform.position = Vector2.Lerp(transform.position, newPosition, Time.deltaTime * 10f);
    }

    private void DetectPlayer()
    {
        var hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
        var closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (var col in hitColliders)
        {
            var distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = col.transform;
            }
        }

        target = closestPlayer;
    }

    private void EnemyFacingHandler(Vector2 target)
    {
        var lookDirection = target - (Vector2)transform.position;
        var angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        var shouldFaceLeft = angle > 90 || angle < -90;
        UpdateFacingServerRpc(shouldFaceLeft);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}