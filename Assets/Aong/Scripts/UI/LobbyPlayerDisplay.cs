using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayerDisplay : NetworkBehaviour
{
    [SerializeField] private Transform lobbyPlayerItemParent;
    [SerializeField] private LobbyPlayerItem lobbyPlayerItemPrefab;
    [SerializeField] private TMP_Text joinCodeText;

    private readonly Dictionary<ulong, LobbyPlayerItem> playerItems = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds) SpawnPlayerItem(client);

            string joinCode = PlayerPrefs.GetString("JoinCode", "N/A");
            UpdateJoinCode(joinCode);
            Debug.Log($"[Server] Initial JoinCode set: {joinCode}");
            SyncJoinCodeClientRpc(joinCode);
        }

        if (IsClient)
        {
            UpdatePlayerList();
            string joinCode = PlayerPrefs.GetString("JoinCode", "N/A");
            UpdateJoinCode(joinCode);
            Debug.Log($"[Client] Initial JoinCode on spawn: {joinCode}");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        SpawnPlayerItem(clientId);
        string joinCode = PlayerPrefs.GetString("JoinCode", "N/A");
        SyncJoinCodeClientRpc(joinCode);
        Debug.Log($"[Server] Syncing JoinCode to new Client {clientId}: {joinCode}");
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        RemovePlayerItem(clientId);
    }

    private void SpawnPlayerItem(ulong clientId)
    {
        if (playerItems.ContainsKey(clientId)) return;

        var newItem = Instantiate(lobbyPlayerItemPrefab, lobbyPlayerItemParent);
        var playerName = HostSingleton.Instance.GameManager.GetPlayerName(clientId);
        var isLocalPlayer = NetworkManager.Singleton.IsServer && clientId == NetworkManager.Singleton.LocalClientId;
        newItem.SetPlayerName(playerName, isLocalPlayer);
        playerItems[clientId] = newItem;
        UpdatePlayerListClientRpc(clientId, playerName);
    }

    private void RemovePlayerItem(ulong clientId)
    {
        if (playerItems.TryGetValue(clientId, out var item))
        {
            if (item != null) Destroy(item.gameObject);
            playerItems.Remove(clientId);
        }

        RemovePlayerClientRpc(clientId);
    }

    [ClientRpc]
    private void UpdatePlayerListClientRpc(ulong clientId, string playerName)
    {
        if (IsServer) return;

        if (!playerItems.ContainsKey(clientId))
        {
            var newItem = Instantiate(lobbyPlayerItemPrefab, lobbyPlayerItemParent);
            var isLocalPlayer = clientId == NetworkManager.Singleton.LocalClientId;
            newItem.SetPlayerName(playerName, isLocalPlayer);
            playerItems[clientId] = newItem;
        }
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(ulong clientId)
    {
        if (IsServer) return;

        RemovePlayerItem(clientId);
    }

    private void UpdatePlayerList()
    {
        foreach (var item in playerItems)
            if (item.Value != null)
                Destroy(item.Value.gameObject);
        playerItems.Clear();
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var newItem = Instantiate(lobbyPlayerItemPrefab, lobbyPlayerItemParent);
            var playerName = HostSingleton.Instance.GameManager.GetPlayerName(clientId);
            var isLocalPlayer = clientId == NetworkManager.Singleton.LocalClientId;
            newItem.SetPlayerName(playerName, isLocalPlayer);
            playerItems[clientId] = newItem;
        }
    }

    [ClientRpc]
    private void SyncJoinCodeClientRpc(string joinCode)
    {
        if (IsServer) return;
        UpdateJoinCode(joinCode);
    }

    private void UpdateJoinCode(string joinCode)
    {
        if (joinCodeText != null)
        {
            joinCodeText.text = $"Code: {joinCode}";
            Debug.Log($"[Lobby] Displaying Join Code: {joinCode}");
        }
    }
}