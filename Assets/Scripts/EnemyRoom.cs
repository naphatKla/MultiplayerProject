using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Core.InteractObj;

public class EnemyRoom : NetworkBehaviour
{
    public List<Transform> enemySpawnPoints;
    public GameObject[] enemyPrefabs;
    public GameObject[] doors;
    public GameObject interactionObject;

    public List<NetworkObject> spawnedEnemies = new List<NetworkObject>();
    public NetworkVariable<bool> roomActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isEnemyRoom = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        if (interactionObject != null)
            interactionObject.SetActive(true);
    }

    public void Interact()
    {
        if (IsOwner && !roomActive.Value)
        {
            InteractServerRpc(); // Call server-side logic
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void InteractServerRpc()
    {
        if (!roomActive.Value)
        {
            roomActive.Value = true;

            if(!isEnemyRoom.Value)
            {
                CompleteObjectiveServerRpc();
            }
            else
            {
                LockRoomClientRpc(); // Lock room on all clients
                SpawnEnemiesServerRpc();
            }
        }
    }

    [ClientRpc]
    void LockRoomClientRpc()
    {
        foreach(GameObject door in doors)
        {
            door.GetComponent<InteractObject>().SetTest(1f, true);
        }
    }

    [ServerRpc]
    void SpawnEnemiesServerRpc()
    {
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemy = Instantiate(enemyPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
            NetworkObject enemyNetworkObject = enemy.GetComponent<NetworkObject>();

            if (enemyNetworkObject != null)
            {
                enemyNetworkObject.Spawn(true); // Spawn across all clients
                spawnedEnemies.Add(enemyNetworkObject);
            }
        }

        CheckForClearConditionServerRpc();
    }

    [ServerRpc]
    void CheckForClearConditionServerRpc()
    {
        if (!isEnemyRoom.Value) return; // Skip checking if it's an empty room

        spawnedEnemies.RemoveAll(enemy => enemy == null || !enemy.IsSpawned);

        if (spawnedEnemies.Count == 0)
        {
            CompleteObjectiveServerRpc();
        }
    }

    [ServerRpc]
    void CompleteObjectiveServerRpc()
    {
        roomActive.Value = false;
        UnlockRoomClientRpc();
    }

    [ClientRpc]
    void UnlockRoomClientRpc()
    {
        foreach (GameObject door in doors)
        {
            door.GetComponent<InteractObject>().SetTest(0.1f, false);
        }
    }

    void Update()
    {
        if (IsServer && roomActive.Value && isEnemyRoom.Value)
        {
            CheckForClearConditionServerRpc();
        }
    }
}
