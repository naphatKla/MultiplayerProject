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
    Adventure
}

public class RoleManager : NetworkSingleton<RoleManager>
{
    private Dictionary<ulong, ExplorerClass> playerExplorerClasses;

    private Dictionary<ulong, bool> playerActiveStatus = new();

    public Dictionary<ulong, Role> PlayerRoles { get; private set; }

    [SerializeField] private bool readyToSet;

    public bool ReadyToSet
    {
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
            playerActiveStatus = new Dictionary<ulong, bool>();
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds) playerActiveStatus[clientId] = true;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (IsServer)
        {
            UpdatePlayerActiveStatus(clientId, false);
            if (PlayerRoles.ContainsKey(clientId))
            {
                Debug.Log($"Client {clientId} disconnected. Removed from playerRoles.");
                PlayerRoles.Remove(clientId);
            }

            if (playerActiveStatus.ContainsKey(clientId)) playerActiveStatus.Remove(clientId);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer) NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        base.OnNetworkDespawn();
    }


    private void AssignRoles()
    {
        if (!IsServer) return;

        PlayerRoles = new Dictionary<ulong, Role>();

        var userIDs = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        ShuffleList(userIDs);

        monsterCount = Mathf.Max(1, Mathf.FloorToInt(userIDs.Count * 0.3f));

        for (var i = 0; i < userIDs.Count; i++)
        {
            var assignedRole = i < monsterCount ? Role.Monster : Role.Explorer;
            PlayerRoles[userIDs[i]] = assignedRole;
        }

        // Debug: Log assigned roles
        foreach (var kvp in PlayerRoles) Debug.Log($"Player {kvp.Key} assigned role: {kvp.Value}");

        // Convert dictionary to arrays for RPC transmission
        var ids = new ulong[PlayerRoles.Count];
        var roles = new int[PlayerRoles.Count];

        var index = 0;
        foreach (var kvp in PlayerRoles)
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
        if (!IsServer) return;

        // Filter out players who have the Explorer role
        var explorerIds = new List<ulong>();
        foreach (var kvp in PlayerRoles)
            if (kvp.Value == Role.Explorer)
                explorerIds.Add(kvp.Key);

        var assignedClasses = new Dictionary<ulong, ExplorerClass>();

        // Classes to assign (excluding the 'Null' class)
        var availableClasses = new List<ExplorerClass>
        {
            ExplorerClass.Adventure
        };

        // Shuffle the available classes to randomize the assignment
        ShuffleList(availableClasses);

        // Assign each explorer a random class without duplication
        var classIndex = 0;
        foreach (var explorerId in explorerIds)
        {
            if (classIndex >= availableClasses.Count) break; // Stop if all classes are assigned

            // Assign the class to the explorer
            var assignedClass = availableClasses[classIndex];
            assignedClasses[explorerId] = assignedClass;

            // Debug log to track assignment
            Debug.Log($"Explorer {explorerId} assigned class: {assignedClass}");

            classIndex++;
        }

        var ids = assignedClasses.Keys.ToArray();
        var classes = assignedClasses.Values.ToArray();

        // Update class assignment on clients
        UpdateManagerExplorerClassRpc(ids, classes);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateManagerExplorerClassRpc(ulong[] ids, ExplorerClass[] classes)
    {
        // Create a dictionary from the arrays
        var serverClass = new Dictionary<ulong, ExplorerClass>();
        for (var i = 0; i < ids.Length; i++) serverClass[ids[i]] = classes[i];

        // Update the playerExplorerClasses dictionary
        playerExplorerClasses = serverClass;
    }

    public ExplorerClass AssignClassForExplorer(ulong id)
    {
        if (playerExplorerClasses.ContainsKey(id))
            // Return the class associated with the player ID
            return playerExplorerClasses[id];

        // If the ID is not found, return 'ExplorerClass.Null' or handle appropriately
        Debug.LogWarning($"Explorer class not found for player ID: {id}");
        return ExplorerClass.NullClass;
    }

    public List<ulong> GetExplorerPlayerIds()
    {
        var explorerPlayerIds = new List<ulong>();

        foreach (var kvp in PlayerRoles)
            if (kvp.Value == Role.Explorer) // Check if the role is Explorer
                explorerPlayerIds.Add(kvp.Key); // Add the player ID to the list

        return explorerPlayerIds;
    }


    private void Start()
    {
        AssignManagerClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AssignManagerClientRpc()
    {
        var playObj = GameObject.FindGameObjectsWithTag("Player");
        foreach (var obj in playObj)
        {
            obj.GetComponent<ExplorerRole>().RoleManager = this;
            obj.GetComponent<MonsterRole>().RoleManager = this;
            var playerId = obj.GetComponent<NetworkBehaviour>().OwnerClientId;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateRoleToClientManagerRpc(ulong[] ids, int[] roles)
    {
        if (PlayerRoles != null)
        {
            Debug.Log("Player Role Maybe Server?");
            readyToSet = true;
            return;
        }

        PlayerRoles = new Dictionary<ulong, Role>();
        Debug.Log($"Player role on client Created id:{ids.Length}");
        for (var i = 0; i < ids.Length; i++)
        {
            PlayerRoles[ids[i]] = (Role)roles[i];
            Debug.Log($"Player {ids[i]}, Role {roles[i]}");
        }

        readyToSet = true;
    }

    public void CheckPlayerCountAndStatus()
    {
        if (!IsServer) return;
        var activePlayerCount = playerActiveStatus.Count(kvp => kvp.Value);
        var activeExplorerCount = 0;
        var activeMonsterCount = 0;

        foreach (var kvp in PlayerRoles)
            if (playerActiveStatus.ContainsKey(kvp.Key) && playerActiveStatus[kvp.Key])
            {
                if (kvp.Value == Role.Explorer)
                    activeExplorerCount++;
                else if (kvp.Value == Role.Monster)
                    activeMonsterCount++;
            }

        Debug.Log($"Active Players: {activePlayerCount}, Explorers: {activeExplorerCount}, Monsters: {activeMonsterCount}");
        
        // Check for Monster (Mimic) win condition
        if (activeMonsterCount == 1 && activeExplorerCount == 0)
        {
            var allExplorersInactive = true;
            foreach (var kvp in PlayerRoles)
                if (kvp.Value == Role.Explorer && playerActiveStatus.ContainsKey(kvp.Key) &&
                    playerActiveStatus[kvp.Key])
                {
                    allExplorersInactive = false;
                    break;
                }

            if (allExplorersInactive) NotifyMimicWinClientRpc();
        }
        // Check for Explorer win condition
        else if (activeMonsterCount == 0 && activeExplorerCount > 0)
        {
            var allMonstersInactive = true;
            foreach (var kvp in PlayerRoles)
                if (kvp.Value == Role.Monster && playerActiveStatus.ContainsKey(kvp.Key) &&
                    playerActiveStatus[kvp.Key])
                {
                    allMonstersInactive = false;
                    break;
                }

            if (allMonstersInactive) NotifyExplorerWinClientRpc();
        }
    }

    public void UpdatePlayerActiveStatus(ulong playerId, bool isActive)
    {
        if (!IsServer) return;

        if (playerActiveStatus.ContainsKey(playerId))
        {
            playerActiveStatus[playerId] = isActive;
            Debug.Log($"Player {playerId} Active Status Updated: {isActive}");
            CheckPlayerCountAndStatus();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyMimicWinClientRpc()
    {
        var localClientId = NetworkManager.Singleton.LocalClientId;
        if (PlayerRoles.TryGetValue(localClientId, out var role))
            if (role == Role.Monster)
                if (GameHUD.Instance != null)
                    GameHUD.Instance.ShowMimicWinUI();
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyExplorerWinClientRpc()
    {
        var localClientId = NetworkManager.Singleton.LocalClientId;
        if (PlayerRoles.TryGetValue(localClientId, out var role))
            if (role == Role.Explorer)
                if (GameHUD.Instance != null)
                    GameHUD.Instance.ShowExplorerWinUI();
    }

    // Fisher-Yates shuffle algorithm
    private void ShuffleList<T>(List<T> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}