using Core.HealthSystems;
using UnityEngine;
using Unity.Netcode;

public class InteractableObject : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<bool> canInteract = new NetworkVariable<bool>(true);
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private KeyCode buttonToInteract = KeyCode.E;
 
    [SerializeField] private NetworkVariable<int> playersEscapedCount = new NetworkVariable<int>(0);
    [SerializeField] private int maxPlayersPerDoor = 2;

    private void Awake()
    {
        canInteract.OnValueChanged += (oldValue, newValue) =>
        {
            Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} canInteract changed from {oldValue} to {newValue} on {gameObject.name}");
        };
        playersEscapedCount.OnValueChanged += (oldValue, newValue) =>
        {
            Debug.Log($"Players escaped through {gameObject.name}: {newValue}/{maxPlayersPerDoor}");
        };
    }

    void Update()
    {
        if (!IsClient) return;
        if (!canInteract.Value) return;

        CheckInteraction();
    }

    private void CheckInteraction()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
        {
            return;
        }

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject == null)
        {
            return;
        }

        GameObject player = playerObject.gameObject;
        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= interactDistance)
        {
            if (UnityEngine.Input.GetKeyDown(buttonToInteract))
            {
                InteractServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!canInteract.Value) return;
        
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) { return; }
        
        if (RoleManager.Instance.PlayerRoles.TryGetValue(clientId, out Role role) && role == Role.Explorer)
        {
            if (playersEscapedCount.Value < maxPlayersPerDoor)
            {
                PerformInteraction(clientId);
            }
        }
    }

    [ClientRpc]
    private void InteractClientRpc()
    {
        PerformInteractionEffects();
    }

    private void PerformInteraction(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) { return; }
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            if (RoleManager.Instance != null)
            {
                RoleManager.Instance.UpdatePlayerActiveStatus(clientId, false);
            }
            else
            {
                Debug.LogError("RoleManager instance is null!");
            }
            playersEscapedCount.Value++;
            if (playersEscapedCount.Value >= maxPlayersPerDoor)
            {
                canInteract.Value = false;
                Debug.Log($"Door {gameObject.name} is now locked. Max players ({maxPlayersPerDoor}) reached.");
            }

            SetPlayerActiveServerRpc(clientId, false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerActiveServerRpc(ulong clientId, bool active)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) { return; }
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            SetPlayerActiveClientRpc(clientId, active);
        }
    }

    [ClientRpc]
    private void SetPlayerActiveClientRpc(ulong clientId, bool active)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            var healthSystem = player.GetComponent<HealthSystem>();
            if (healthSystem != null && active == false)
            {
                healthSystem.SetActiveFalse();
            }
        }
    }

    private void PerformInteractionEffects()
    {
        Debug.Log("Interaction effect triggered");
        SoundEffectManager.Instance.PlayGlobal3DAtPosition("Interact", transform.position, 2f,1f,10f);
    }
    
    public void ResetDoor()
    {
        if (IsServer)
        {
            playersEscapedCount.Value = 0;
            canInteract.Value = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}