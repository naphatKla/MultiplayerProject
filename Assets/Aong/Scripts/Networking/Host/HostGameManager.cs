using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class HostGameManager : IDisposable
{
    private Allocation allocation;
    private string joinCode;
    private string lobbyId;

    private NetworkServer networkServer;

    private const int MaxConnections = 20;
    private const string GameSceneName = "Lobby";
    private const string JoinCodeKey = "JoinCode";

    private readonly Dictionary<ulong, string> playerNames = new();

    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            PlayerPrefs.SetString(JoinCodeKey, joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        var relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            var lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>
            {
                {
                    "JoinCode", new DataObject(
                        DataObject.VisibilityOptions.Member,
                        joinCode
                    )
                }
            };
            var playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            var lobby = await LobbyService.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }

        networkServer = new NetworkServer(NetworkManager.Singleton);

        var userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        var payload = JsonUtility.ToJson(userData);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public string GetPlayerName(ulong clientId)
    {
        if (networkServer != null)
        {
            var name = networkServer.GetUserName(clientId);
            if (!string.IsNullOrEmpty(name))
            {
                playerNames[clientId] = name;
                return name;
            }
        }

        return playerNames.TryGetValue(clientId, out var fallbackName)
            ? fallbackName
            : PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
    }

    public async Task UpdateLobbyPlayerCount()
    {
        if (string.IsNullOrEmpty(lobbyId) || networkServer == null) return;

        var currentPlayers = networkServer.GetConnectedClients().Count;
        try
        {
            await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "PlayerCount", new DataObject(DataObject.VisibilityOptions.Public, currentPlayers.ToString()) }
                }
            });
            Debug.Log($"[Host] Updated Lobby player count: {currentPlayers}/{MaxConnections}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to update Lobby player count: {e.Message}");
        }
    }

    public void SpawnAllPlayers()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;


        var playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
        if (playerPrefab == null) return;

        if (networkServer == null) return;

        var connectedClients = networkServer.GetConnectedClients();

        foreach (var clientId in connectedClients)
        {
            var playerInstance = Object.Instantiate(playerPrefab);
            var networkObject = playerInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId);
                Debug.Log($"Spawned player for client {clientId}");

                playerInstance.transform.position = new Vector3(clientId * 2f, 0f, 0f);
            }
        }
    }


    public async void Dispose()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            lobbyId = string.Empty;
        }

        if (networkServer != null)
        {
            try
            {
                networkServer.Dispose();
                Debug.Log("NetworkServer disposed.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error disposing NetworkServer: {e}");
            }

            networkServer = null;
        }
        else
        {
            Debug.LogWarning("networkServer was null, nothing to dispose.");
        }
    }
}