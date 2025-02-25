using System.Threading.Tasks;
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
                GameObject hostSingletonObj = new GameObject("HostSingleton");
                instance = hostSingletonObj.AddComponent<HostSingleton>();
                DontDestroyOnLoad(hostSingletonObj);
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SetupGameManager();
    }

    private void SetupGameManager()
    {
        GameObject gameManagerObj = new GameObject("HostGameManager");
        GameManager = gameManagerObj.AddComponent<HostGameManager>();
        gameManagerObj.transform.SetParent(transform);
        DontDestroyOnLoad(gameManagerObj);
    }

    public async Task CreateHost()
    {
        if (GameManager == null)
        {
            Debug.LogError("GameManager is null! This should not happen.");
            SetupGameManager();
        }
        await GameManager.StartHostAsync();
    }
}