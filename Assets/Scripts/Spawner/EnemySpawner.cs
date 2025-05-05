using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Core.HealthSystems;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private float maxSpawn = 10f;
    [SerializeField] private float cooldown = 5f;

    private NetworkList<int> activeEnemies; // Track active enemy NetworkObject IDs
    private float lastSpawnTime;

    private void Awake()
    {
        activeEnemies = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            lastSpawnTime = Time.time;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        // Check if we can spawn a new enemy
        if (activeEnemies.Count < maxSpawn && Time.time >= lastSpawnTime + cooldown)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Count == 0 || enemyPrefabs.Count == 0) return;

        // Randomly select spawn point and enemy prefab
        int spawnIndex = Random.Range(0, spawnPoints.Count);
        int enemyIndex = Random.Range(0, enemyPrefabs.Count);

        Vector3 spawnPosition = spawnPoints[spawnIndex].position;
        GameObject enemyPrefab = enemyPrefabs[enemyIndex];

        // Instantiate and spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
        
        if (networkObject != null)
        {
            networkObject.Spawn();
            enemy.transform.SetParent(enemyParent);
            activeEnemies.Add((int)networkObject.NetworkObjectId);
            
            if (enemy.TryGetComponent<HealthSystem>(out var health))
            {
                health.OnDie += () => OnEnemyDespawn(networkObject.NetworkObjectId);
            }
        }
    }

    private void OnEnemyDespawn(ulong networkObjectId)
    {
        if (!IsServer) return;
        
        int index = activeEnemies.IndexOf((int)networkObjectId);
        if (index != -1)
        {
            activeEnemies.RemoveAt(index);
        }
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            foreach (int enemyId in activeEnemies)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue((ulong)enemyId, out NetworkObject networkObject))
                {
                    networkObject.Despawn();
                }
            }
            activeEnemies.Clear();
        }
    }
}