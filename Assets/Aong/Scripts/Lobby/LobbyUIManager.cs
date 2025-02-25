using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class LobbyUIManager : NetworkBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private GameObject playerBarPrefab;
    [SerializeField] private Transform content;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            UpdatePlayerList();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
    }
        
    private void OnClientConnected(ulong clientId)
    {
        UpdatePlayerList();
    }

    private void OnClientDisconnect(ulong clientId)
    {
        UpdatePlayerList();
    }

    public void UpdatePlayerList()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        int playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        Debug.Log($"Connected players: {playerCount}");
    
        HostGameManager hostGameManager = HostSingleton.Instance?.GameManager;
        if (hostGameManager == null)
        {
            Debug.LogError("HostGameManager not found in HostSingleton!");
            return;
        }

        var clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        for (int i = 0; i < playerCount; i++)
        {
            ulong clientId = clientIds[i];
            string playerName = hostGameManager.GetPlayerName(clientId);
            Debug.Log($"Player name for clientId {clientId}: {playerName}");
            GameObject playerBar = Instantiate(playerBarPrefab, content);
            playerBar.GetComponentInChildren<TMP_Text>().text = playerName;
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}