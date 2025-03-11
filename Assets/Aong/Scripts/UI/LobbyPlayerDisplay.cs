using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayerDisplay : NetworkBehaviour
{
    [SerializeField] private Transform lobbyPlayerItemParent;
    [SerializeField] private LobbyPlayerItem lobbyPlayerItemPrefab;
    [SerializeField] private TMP_Text joinCodeText;

    private readonly Dictionary<ulong, LobbyPlayerItem> playerItems = new();
    private readonly Dictionary<ulong, string> playerNames = new();

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton == null) return;
        playerItems.Clear();
        playerNames.Clear();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            InitialUpdatePlayerList();

            var joinCode = PlayerPrefs.GetString("JoinCode", "N/A");
            UpdateJoinCode(joinCode);
            Debug.Log($"[Server] Initial JoinCode set: {joinCode}");
            SyncJoinCodeClientRpc(joinCode);
        }

        if (IsClient && !IsHost)
        {
            var joinCode = PlayerPrefs.GetString("JoinCode", "N/A");
            UpdateJoinCode(joinCode);
            Debug.Log($"[Client] Initial JoinCode on spawn: {joinCode}");
            RequestPlayerListServerRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
            foreach (var item in playerItems.Values)
                if (item != null)
                    Destroy(item.gameObject);
            playerItems.Clear();
            playerNames.Clear();
            Debug.Log($"[{(IsServer ? "Server" : "Client")}] Cleared playerItems and playerNames on despawn");
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        var playerName = HostSingleton.Instance.GameManager.GetPlayerName(clientId);
        playerNames[clientId] = playerName;
        UpdatePlayerList();
        SyncPlayerListToClientsClientRpc();
        SyncPlayerNameToClientClientRpc(clientId, playerName);
        Debug.Log($"[Server] Client connected: {clientId} with name {playerName}");
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        playerNames.Remove(clientId);
        RemovePlayerItem(clientId);
        SyncPlayerListToClientsClientRpc();
        Debug.Log($"[Server] Client disconnected: {clientId}");
    }

    private void InitialUpdatePlayerList()
    {
        if (!IsServer) return;

        var connectedClients = HostSingleton.Instance.GameManager.networkServer.GetConnectedClients();
        playerNames.Clear();
        foreach (var clientId in connectedClients)
        {
            var playerName = HostSingleton.Instance.GameManager.GetPlayerName(clientId);
            playerNames[clientId] = playerName;
            SyncPlayerNameToClientClientRpc(clientId, playerName);
        }

        UpdatePlayerList();
        SyncPlayerListToClientsClientRpc();
        Debug.Log(
            $"[Server] Initial player list: {string.Join(", ", playerNames.Select(kv => $"{kv.Key}: {kv.Value}"))}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerListServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        foreach (var kvp in playerNames)
            SyncPlayerNameToClientClientRpc(kvp.Key, kvp.Value, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
            });
    }

    [ClientRpc]
    private void SyncPlayerListToClientsClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;
        Debug.Log("[Client] Waiting for player list update from Host");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerNameServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        var playerName = HostSingleton.Instance.GameManager.GetPlayerName(clientId);
        playerNames[clientId] = playerName;
        SyncPlayerNameToClientClientRpc(clientId, playerName, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
        });
    }

    [ClientRpc]
    private void SyncPlayerNameToClientClientRpc(ulong clientId, string playerName, ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;

        playerNames[clientId] = playerName;
        UpdatePlayerList();
        Debug.Log($"[Client] Received name for client {clientId}: {playerName}");
    }

    private void UpdatePlayerList()
    {
        foreach (var item in playerItems.Values)
            if (item != null)
                Destroy(item.gameObject);
        playerItems.Clear();

        foreach (var kvp in playerNames)
        {
            var clientId = kvp.Key;
            var playerName = kvp.Value;
            if (lobbyPlayerItemPrefab == null || lobbyPlayerItemParent == null)
            {
                Debug.LogError(
                    $"[{(IsServer ? "Server" : "Client")}] lobbyPlayerItemPrefab or lobbyPlayerItemParent is null");
                return;
            }

            var newItem = Instantiate(lobbyPlayerItemPrefab, lobbyPlayerItemParent);
            var isLocalPlayer = clientId == NetworkManager.Singleton.LocalClientId;
            newItem.SetPlayerName(playerName, isLocalPlayer);
            playerItems[clientId] = newItem;
            Debug.Log(
                $"[{(IsServer ? "Server" : "Client")}] Updated PlayerItem for client {clientId} with name {playerName}");
        }
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
    private void RemovePlayerClientRpc(ulong clientId)
    {
        if (IsServer) return;
        if (playerItems.TryGetValue(clientId, out var item))
        {
            if (item != null) Destroy(item.gameObject);
            playerItems.Remove(clientId);
            Debug.Log($"[Client] Removed PlayerItem for client {clientId}");
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