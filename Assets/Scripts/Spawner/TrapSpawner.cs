using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrapSpawner : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<GameObject> trapPrefabs;
    [SerializeField] private Transform trapParent;
    [SerializeField] private float maxSpawn = 5f;

    private NetworkList<int> activeTraps;
    private List<Transform> availableSpawnPoints;

    private void Awake()
    {
        activeTraps = new NetworkList<int>();
        availableSpawnPoints = new List<Transform>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            availableSpawnPoints.AddRange(spawnPoints);
            StartCoroutine(WaitThenSpawnInitialTraps());
        }
    }

    private IEnumerator WaitThenSpawnInitialTraps()
    {
        yield return new WaitForSeconds(2f);
        SpawnInitialTraps();
    }
    
    private void SpawnInitialTraps()
    {
        if (spawnPoints.Count == 0 || trapPrefabs.Count == 0)
        {
            Debug.LogWarning("TrapSpawner: No spawn points or trap prefabs assigned.");
            return;
        }

        if (trapParent == null)
        {
            Debug.LogWarning("TrapSpawner: trapParent is not assigned. Traps will not be parented.");
            
        }
        
        int trapsToSpawn = Mathf.Min((int)maxSpawn, availableSpawnPoints.Count);

        for (int i = 0; i < trapsToSpawn; i++)
        {
            if (availableSpawnPoints.Count == 0) break;
            
            int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
            int trapIndex = Random.Range(0, trapPrefabs.Count);

            Vector3 spawnPosition = availableSpawnPoints[spawnIndex].position;
            GameObject trapPrefab = trapPrefabs[trapIndex];
            
            GameObject trap = Instantiate(trapPrefab, spawnPosition, Quaternion.identity);
            
            NetworkObject networkObject = trap.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Destroy(trap);
                continue;
            }
            
            networkObject.Spawn();
            if (trapParent != null)
            {
                NetworkObject parentNetworkObject = trapParent.GetComponent<NetworkObject>();
                if (parentNetworkObject != null && parentNetworkObject.IsSpawned) { networkObject.TrySetParent(parentNetworkObject); }
            }
            activeTraps.Add((int)networkObject.NetworkObjectId);
            availableSpawnPoints.RemoveAt(spawnIndex);
        }
    }

    public void OnTrapDeactivate(ulong networkObjectId)
    {
        if (!IsServer) return;
        
        int index = activeTraps.IndexOf((int)networkObjectId);
        if (index != -1)
        {
            activeTraps.RemoveAt(index);
        }
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            foreach (int trapId in activeTraps)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue((ulong)trapId, out NetworkObject networkObject))
                {
                    networkObject.Despawn();
                }
            }
            activeTraps.Clear();
        }
    }
}