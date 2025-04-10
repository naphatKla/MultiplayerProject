using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Role
{
    NullRole,
    Explorer,
    Monster
}
public enum ExplorerClass
{
    NullClass,
    Paladin,
    Wizard,
    Healer
}

public class RoleManager : NetworkSingleton<RoleManager>
{
    private Dictionary<ulong, ExplorerClass> playerExplorerClasses;
    
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
        AssignManagerClientRpc();
        AssignClassForExplorer();
    }

    public void AssignClassForExplorer()
    {
        if (!IsServer)
        {
            return;
        }

        // Filter out players who have the Explorer role
        List<ulong> explorerIds = new List<ulong>();
        foreach (var kvp in playerRoles)
        {
            if (kvp.Value == Role.Explorer)
            {
                explorerIds.Add(kvp.Key);
            }
        }

        Dictionary<ulong, ExplorerClass> assignedClasses = new Dictionary<ulong, ExplorerClass>();
        
        // Classes to assign (excluding the 'Null' class)
        List<ExplorerClass> availableClasses = new List<ExplorerClass> {
            ExplorerClass.Paladin, 
            ExplorerClass.Wizard, 
            ExplorerClass.Healer
        };

        // Shuffle the available classes to randomize the assignment
        ShuffleList(availableClasses);

        // Assign each explorer a random class without duplication
        int classIndex = 0;
        foreach (ulong explorerId in explorerIds)
        {
            if (classIndex >= availableClasses.Count) break; // Stop if all classes are assigned

            // Assign the class to the explorer
            ExplorerClass assignedClass = availableClasses[classIndex];
            assignedClasses[explorerId] = assignedClass;
        
            // Debug log to track assignment
            Debug.Log($"Explorer {explorerId} assigned class: {assignedClass}");

            classIndex++;
        }

        ulong[] ids = assignedClasses.Keys.ToArray();
        ExplorerClass[] classes = assignedClasses.Values.ToArray();

        // Update class assignment on clients
        UpdateManagerExplorerClassRpc(ids, classes);
        
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateManagerExplorerClassRpc(ulong[] ids, ExplorerClass[] classes)
    {
        // Create a dictionary from the arrays
        Dictionary<ulong, ExplorerClass> serverClass = new Dictionary<ulong, ExplorerClass>();
        for (int i = 0; i < ids.Length; i++)
        {
            serverClass[ids[i]] = classes[i];
        }

        // Update the playerExplorerClasses dictionary
        playerExplorerClasses = serverClass;
    }
    
    public ExplorerClass AssignClassForExplorer(ulong id)
    {
        if (playerExplorerClasses.ContainsKey(id))
        {
            // Return the class associated with the player ID
            return playerExplorerClasses[id];
        }
        else
        {
            // If the ID is not found, return 'ExplorerClass.Null' or handle appropriately
            Debug.LogWarning($"Explorer class not found for player ID: {id}");
            return ExplorerClass.NullClass;
        }
    }
    
    public List<ulong> GetExplorerPlayerIds()
    {
        List<ulong> explorerPlayerIds = new List<ulong>();
    
        foreach (var kvp in playerRoles)
        {
            if (kvp.Value == Role.Explorer) // Check if the role is Explorer
            {
                explorerPlayerIds.Add(kvp.Key); // Add the player ID to the list
            }
        }

        return explorerPlayerIds;
    }
    
    
    private void Start()
    {
        AssignManagerClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void AssignManagerClientRpc()
    {
        GameObject[] playObj = GameObject.FindGameObjectsWithTag("Player");
        foreach (var obj in playObj)
        {
            obj.GetComponent<ExplorerRole>().RoleManager = this;
            obj.GetComponent<MonsterRole>().RoleManager = this;
            ulong playerId = obj.GetComponent<NetworkBehaviour>().OwnerClientId;
        }
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
