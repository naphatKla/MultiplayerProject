using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyMenu : MonoBehaviour
{
    private const string GameplaySceneName = "Gameplay";

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameplaySceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("Only the Host can start the game!");
        }
    }
}