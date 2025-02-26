using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;

    public HostGameManager GameManager { get; private set; }

    public static HostSingleton Instance
    {
        get
        {
            if (instance == null)
            {
                var hostSingletonObj = new GameObject("HostSingleton");
                instance = hostSingletonObj.AddComponent<HostSingleton>();
                DontDestroyOnLoad(hostSingletonObj);
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void SetupGameManager()
    {
        if (GameManager != null) return;
        var gameManagerObj = new GameObject("HostGameManager");
        GameManager = gameManagerObj.AddComponent<HostGameManager>();
        DontDestroyOnLoad(gameManagerObj);
    }

    public async Task CreateHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager is not present in the scene!");
            return;
        }

        if (!NetworkManager.Singleton.IsListening)
        {
            SetupGameManager();
            await GameManager.StartHostAsync();
            GameManager.gameObject.AddComponent<NetworkObject>();
        }
    }
}