using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class LobbyUIManager : NetworkBehaviour
{
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private GameObject playerBarPrefab;
    [SerializeField] private Transform content;
    

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
        for (int i = 0; i < playerCount; i++)
        {
            GameObject playerBar = Instantiate(playerBarPrefab, content);
            playerBar.GetComponentInChildren<TMP_Text>().text = "Player " + (i + 1);
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