using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class LobbyMenu : MonoBehaviour
{
    private const string GameplaySceneName = "Gameplay";
    private const string MenuSceneName = "Menu";
    [SerializeField] private int minPlayersToStart = 2;

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Only the Host can start the game!");
            return;
        }

        int playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        if (playerCount < minPlayersToStart)
        {
            Debug.LogWarning($"Need at least {minPlayersToStart} players to start the game. Current: {playerCount}");
            return;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(GameplaySceneName, LoadSceneMode.Single);
    }

    public void LeaveLobby()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("NetworkManager is not available, loading Menu directly!");
            SceneManager.LoadScene(MenuSceneName);
            return;
        }
        
        var lobbyDisplay = FindObjectOfType<LobbyPlayerDisplay>();
        if (lobbyDisplay != null)
        {
            lobbyDisplay.OnNetworkDespawn();
        }

        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[Host] Leaving Lobby and shutting down...");
            HostSingleton.Instance.GameManager.Dispose();
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[Host] Server shutdown complete.");
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("[Client] Disconnecting from Lobby...");
            NetworkManager.Singleton.Shutdown();
            ClientSingleton.Instance.GameManager.Dispose();
            Debug.Log("[Client] Disconnected from Host.");
        }

        SceneManager.LoadScene(MenuSceneName);
        Debug.Log($"Loading Scene: {MenuSceneName}");
    }
}