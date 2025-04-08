using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Role
{
    Null,
    Explorer,
    Monster
}

public class RoleManager : NetworkSingleton<RoleManager>
{
    private Dictionary<ulong, Role> playerRoles;
    public Dictionary<ulong, Role> PlayerRoles {
        get => playerRoles;
        private set => playerRoles = value;
    }

    [SerializeField] private bool readyToSet = false;
    public bool ReadyToSet {
        get => readyToSet;
        private set => readyToSet = value;
    }
    [SerializeField] private int monsterCount;
    [SerializeField] public RandomRoleUI randomRoleUI;
    
    public override void OnNetworkSpawn()
    { 
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            AssignRoles();
        }
    }

    private void AssignRoles()
    {
        if (!IsServer) return;

        playerRoles = new Dictionary<ulong, Role>();

        List<ulong> userIDs = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        ShuffleList(userIDs);

        monsterCount = Mathf.Max(1, Mathf.FloorToInt(userIDs.Count * 0.3f));

        for (int i = 0; i < userIDs.Count; i++)
        {
            Role assignedRole = (i < monsterCount) ? Role.Monster : Role.Explorer;
            playerRoles[userIDs[i]] = assignedRole;
        }

        // Debug: Log assigned roles
        foreach (var kvp in playerRoles)
        {
            Debug.Log($"Player {kvp.Key} assigned role: {kvp.Value}");
        }

        // Convert dictionary to arrays for RPC transmission
        ulong[] ids = new ulong[playerRoles.Count];
        int[] roles = new int[playerRoles.Count];

        int index = 0;
        foreach (var kvp in playerRoles)
        {
            ids[index] = kvp.Key;
            roles[index] = (int)kvp.Value;
            index++;
        }

        UpdateRoleToClientManagerRpc(ids, roles);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateRoleToClientManagerRpc(ulong[] ids, int[] roles)
    {
        if (playerRoles != null)
        {
            Debug.Log($"Player Role Maybe Server?");
            readyToSet = true;
            return;
        }
        playerRoles = new Dictionary<ulong, Role>();
        Debug.Log($"Player role on client Created id:{ids.Length}");
        for (int i = 0; i < ids.Length; i++)
        {
            playerRoles[ids[i]] = (Role)roles[i];
            Debug.Log($"Player {ids[i]}, Role {roles[i]}");
        }
        readyToSet = true;
    }
    
    // Fisher-Yates shuffle algorithm
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
