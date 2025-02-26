using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;
    public ClientGameManager GameManager { get; private set; }

    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) { return instance; }
            instance = FindFirstObjectByType<ClientSingleton>();
            if (instance == null)
            {
                Debug.LogError("No ClientSingleton in the scene!");
                return null;
            }
            return instance;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SetupGameManager();
    }

    private void SetupGameManager()
    {
        GameObject gameManagerObj = new GameObject("ClientGameManager");
        GameManager = gameManagerObj.AddComponent<ClientGameManager>();
        gameManagerObj.transform.SetParent(transform);
    }

    public async Task<bool> CreateClient()
    {
        if (GameManager == null)
        {
            SetupGameManager();
        }
        return await GameManager.InitAsync();
    }
}