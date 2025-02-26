using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerListDebugger : NetworkBehaviour
{
    [SerializeField] private GameObject playerBarPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private TMP_Text debugText;

    private readonly Dictionary<ulong, string> playerNames = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!NetworkManager.Singleton) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (IsHost)
            InitialUpdatePlayerList();
        else if (IsClient) RequestPlayerListServerRpc();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsHost)
        {
            var hostGameManager = HostSingleton.Instance?.GameManager;
            if (hostGameManager != null)
            {
                var playerName = hostGameManager.GetPlayerName(clientId);
                playerNames[clientId] = playerName;
            }

            UpdatePlayerList();
            SyncPlayerListToClientsClientRpc();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsHost)
        {
            playerNames.Remove(clientId);
            UpdatePlayerList();
            SyncPlayerListToClientsClientRpc();
        }
    }

    private void InitialUpdatePlayerList()
    {
        if (!IsHost) return;

        var hostGameManager = HostSingleton.Instance?.GameManager;
        if (hostGameManager == null)
        {
            Debug.LogError("[PlayerListDebugger] HostGameManager not found!");
            return;
        }

        playerNames.Clear();
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerName = hostGameManager.GetPlayerName(clientId);
            playerNames[clientId] = playerName;
        }

        UpdatePlayerList();
        SyncPlayerListToClientsClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerListServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsHost)
            SyncPlayerListToClientsClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
            });
    }

    [ClientRpc]
    private void SyncPlayerListToClientsClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsHost) return;

        playerNames.Clear();
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds) RequestPlayerNameServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerNameServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        if (!IsHost) return;

        var hostGameManager = HostSingleton.Instance?.GameManager;
        if (hostGameManager == null) return;

        var playerName = hostGameManager.GetPlayerName(clientId);
        playerNames[clientId] = playerName;
        SyncPlayerNameToClientClientRpc(clientId, playerName, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
        });
    }

    [ClientRpc]
    private void SyncPlayerNameToClientClientRpc(ulong clientId, FixedString128Bytes playerName,
        ClientRpcParams rpcParams = default)
    {
        playerNames[clientId] = playerName.ToString();
        UpdatePlayerList();
    }

    public void UpdatePlayerList()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        var debugString = IsHost ? "Host Player List:\n" : "Client Player List:\n";
        var playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerName = playerNames.ContainsKey(clientId) ? playerNames[clientId] : "Unknown";
            CreatePlayerBar(clientId, playerName);
            debugString += $"ClientID: {clientId}, Name: {playerName}\n";
        }

        Debug.Log($"[PlayerListDebugger] {debugString}");
        if (debugText != null) debugText.text = debugString;
    }

    private void CreatePlayerBar(ulong clientId, string playerName)
    {
        var playerBar = Instantiate(playerBarPrefab, content);
        var nameText = playerBar.GetComponentInChildren<TMP_Text>();
        if (nameText != null) nameText.text = $"{playerName} (ID: {clientId})";
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        base.OnNetworkDespawn();
    }
}