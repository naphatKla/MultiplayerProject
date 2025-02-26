using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ClientJoinCodeDebugger : NetworkBehaviour
{
    private ClientGameManager clientGameManager;
    private HostGameManager hostGameManager;
    [SerializeField] private TMP_Text codeText;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Lobby") return;

        SetupManagers();
        UpdateJoinCodeUI();
        DebugJoinCode();
    }

    private void SetupManagers()
    {
        //Host
        if (IsHost && HostSingleton.Instance != null)
        {
            hostGameManager = HostSingleton.Instance.GameManager;
            if (hostGameManager == null) Debug.LogError("[ClientJoinCodeDebugger] HostGameManager is null!");
        }
        //Client
        else if (IsClient && ClientSingleton.Instance != null)
        {
            clientGameManager = ClientSingleton.Instance.GameManager;
            if (clientGameManager == null) Debug.LogError("[ClientJoinCodeDebugger] ClientGameManager is null!");
        }
    }

    private void DebugJoinCode()
    {
        var joinCode = GetJoinCode();
        if (!string.IsNullOrEmpty(joinCode))
            Debug.Log($"[ClientJoinCodeDebugger] Connected with Join Code: {joinCode} (IsHost: {IsHost})");
        else
            Debug.LogWarning("[ClientJoinCodeDebugger] Could not retrieve Join Code");
    }

    private void UpdateJoinCodeUI()
    {
        var joinCode = GetJoinCode();
        if (codeText != null)
        {
            if (!string.IsNullOrEmpty(joinCode))
                codeText.text = $"Code: {joinCode}";
            else
                codeText.text = "Code: Not Available";
        }
        else
        {
            Debug.LogError("[ClientJoinCodeDebugger] TMP_Text codeText is not assigned in Inspector!");
        }
    }

    private string GetJoinCode()
    {
        //Host
        if (IsHost && hostGameManager != null)
        {
            if (string.IsNullOrEmpty(hostGameManager.joinCode))
            {
                Debug.LogWarning("[ClientJoinCodeDebugger] JoinCode is null or empty in HostGameManager");
                return "Not Set (Host)";
            }

            return hostGameManager.joinCode;
        }
        //Client

        if (IsClient && clientGameManager != null)
        {
            if (string.IsNullOrEmpty(clientGameManager.JoinCode))
            {
                Debug.LogWarning("[ClientJoinCodeDebugger] JoinCode is null or empty in ClientGameManager");
                return "Not Set (Client)";
            }

            return clientGameManager.JoinCode;
        }

        return "No Manager Available";
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            SetupManagers();
            UpdateJoinCodeUI();
            DebugJoinCode();
        }
    }
}