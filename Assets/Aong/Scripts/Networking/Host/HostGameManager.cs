using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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
    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch(Exception e)
        {
            Debug.Log(e);
            return;
        }
        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            PlayerPrefs.SetString(JoinCodeKey,joinCode);
        }
        catch (Exception e) 
        {
            Debug.Log(e);
            return;
        }
        
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode",new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    )
                }
            };
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby",MaxConnections,lobbyOptions);
            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }

        networkServer = new NetworkServer(NetworkManager.Singleton);
        
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true) 
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
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