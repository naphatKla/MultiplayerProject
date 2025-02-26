using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : NetworkBehaviour
{
    private JoinAllocation allocation;
    private const string MenuSceneName = "Menu";
    public string JoinCode { get; private set; }

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        var authState = await AuthenticationWrapper.DoAuth();

        if (authState == AuthState.Authenticated) return true;

        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            JoinCode = joinCode;
            Debug.Log($"[ClientGameManager] Stored Join Code: {JoinCode}");
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

        NetworkManager.Singleton.StartClient();
    }

}