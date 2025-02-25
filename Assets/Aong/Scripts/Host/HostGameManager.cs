using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager
{
    private Allocation allocation;
    private string joinCode;
    
    private const int MaxConnections = 15;
    private const string GameSceneName = "Lobby";
    
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
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }
    
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string connectionData = System.Text.Encoding.UTF8.GetString(request.Payload);
        Debug.Log($"Client connection data: {connectionData}");

        response.Approved = true;
        response.CreatePlayerObject = false;
    }
    
    // Spawn Player one Client
    public void SpawnPlayer(ulong clientId)
    {
        GameObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not set in NetworkManager!");
            return;
        }
        GameObject playerInstance = GameObject.Instantiate(playerPrefab);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    // ฟSpawn Player All Client
    public void SpawnAllPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    // Getter JoinCode
    public string GetJoinCode()
    {
        return joinCode;
    }
}