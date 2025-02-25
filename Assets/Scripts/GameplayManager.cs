using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameplayManager : NetworkBehaviour
{
    void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            StartCoroutine(SpawnPlayersWhenSceneLoaded());
        }
    }

    private IEnumerator SpawnPlayersWhenSceneLoaded()
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Gameplay");
        
        yield return new WaitForSeconds(0.5f);
        HostSingleton.Instance.GameManager.SpawnAllPlayers();
        Debug.Log("Scene Gameplay is active, spawning players...");
    }
}