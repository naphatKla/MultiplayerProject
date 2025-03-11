using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class GameSceneManager : MonoBehaviour
{
    private const string GameSceneName = "Gameplay";
    
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);
        if (SceneManager.GetActiveScene().name != GameSceneName) yield break;
        HostSingleton.Instance.GameManager.SpawnAllPlayers();
    }
}