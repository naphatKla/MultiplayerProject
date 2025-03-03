using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
    private List<ulong> connectedClients = new List<ulong>();
    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        this.networkManager.OnServerStarted += OnNetworkReady;
    }

    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private async void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
            connectedClients.Remove(clientId);
            Debug.Log($"[NetworkServer] Client {clientId} disconnected.");

            if (HostSingleton.Instance?.GameManager != null)
            {
                await HostSingleton.Instance.GameManager.UpdateLobbyPlayerCount();
            }
        }
    }

    private async void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;
        connectedClients.Add(request.ClientNetworkId);
        Debug.Log($"[NetworkServer] Client {request.ClientNetworkId} connected with name: {userData.userName}");

        response.Approved = true;
        response.CreatePlayerObject = false;

        if (HostSingleton.Instance?.GameManager != null)
        {
            await HostSingleton.Instance.GameManager.UpdateLobbyPlayerCount();
        }
    }
    
    public List<ulong> GetConnectedClients()
    {
        return new List<ulong>(connectedClients);
    }
    
    public string GetUserName(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId) && 
            authIdToUserData.TryGetValue(authId, out UserData userData))
        {
            return userData.userName;
        }
        return null;
    }

    public void Dispose()
    {
        if(networkManager == null) {return;}

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}