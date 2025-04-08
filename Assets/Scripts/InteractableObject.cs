using Core.HealthSystems;
using UnityEngine;
using Unity.Netcode;

public class InteractableObject : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<bool> canInteract = new NetworkVariable<bool>(true);
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private KeyCode buttonToInteract = KeyCode.E;
    

    void Update()
    {
        if (!IsOwner || !IsClient) return;
        if (!canInteract.Value) return;

        CheckInteraction();
    }

    private void CheckInteraction()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null) { return; }
        
        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject == null) { return; }

        GameObject player = playerObject.gameObject;
        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= interactDistance && UnityEngine.Input.GetKeyDown(buttonToInteract))
        {
            InteractServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!canInteract.Value) return;
        PerformInteraction(serverRpcParams.Receive.SenderClientId);
        InteractClientRpc();
    }

    [ClientRpc]
    private void InteractClientRpc()
    {
        PerformInteractionEffects();
    }

    private void PerformInteraction(ulong clientId)
    {
        Debug.Log("Object Interacted by: " + clientId);
        
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            SetPlayerActiveServerRpc(clientId, false);
        }
        
        canInteract.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerActiveServerRpc(ulong clientId, bool active)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            player.SetActive(active);
            SetPlayerActiveClientRpc(clientId, active);
        }
    }

    [ClientRpc]
    private void SetPlayerActiveClientRpc(ulong clientId, bool active)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            player.GetComponent<HealthSystem>().SetActiveFalse();
        }
    }

    private void PerformInteractionEffects()
    {
        Debug.Log("Interaction effect triggered");
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}