using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class LobbyMenu : MonoBehaviour
{
    private const string GameplaySceneName = "Gameplay";
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
}