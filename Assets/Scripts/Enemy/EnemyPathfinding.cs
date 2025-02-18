using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPathfinding : NetworkBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private Transform target;
    public float detectionRadius = 5f;
    public LayerMask playerLayer;
    private Vector2 startPosition;
    private Vector2 lastSentPosition;
    private float positionThreshold = 0.1f;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Enemy Spawned on Server");
        }
    }

    void Update()
    {
        if (IsServer)
        {
            DetectPlayer();
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                agent.SetDestination(startPosition);
            }
            
            if (Vector2.Distance(transform.position, lastSentPosition) > positionThreshold)
            {
                lastSentPosition = transform.position;
                UpdatePositionClientRpc(transform.position);
            }
        }
    }

    [ClientRpc]
    void UpdatePositionClientRpc(Vector2 newPosition)
    {
        if (!IsServer)
        {
            transform.position = Vector2.Lerp(transform.position, newPosition, Time.deltaTime * 10f);
        }
    }

    void DetectPlayer()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (Collider2D col in hitColliders)
        {
            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = col.transform;
            }
        }

        target = closestPlayer;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
