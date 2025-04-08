using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TreasureObject : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<bool> canInteract = new(true);
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private KeyCode buttonToInteract = KeyCode.E;
    [SerializeField] private float interactHoldTime = 3f;
    [SerializeField] private Slider progressBar;

    [SerializeField] private List<InteractableObject> runfunctionOnComplete = new();

    private readonly NetworkVariable<float> interactProgress = new();
    private bool isInteracting;
    private ulong interactingClientId;

    private void Update()
    {
        if (!IsOwner || !IsClient) return;
        if (!canInteract.Value) return;

        CheckInteraction();
        UpdateProgressBarOrientation();
    }

    private void CheckInteraction()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null) return;

        var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject == null) return;

        var player = playerObject.gameObject;
        var distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= interactDistance)
        {
            if (UnityEngine.Input.GetKey(buttonToInteract))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    interactingClientId = NetworkManager.Singleton.LocalClientId;
                }

                UpdateProgressServerRpc(interactingClientId, Time.deltaTime / interactHoldTime);
            }
            else if (isInteracting)
            {
                isInteracting = false;
                ResetInteractServerRpc();
            }
        }
        else if (isInteracting)
        {
            isInteracting = false;
            ResetInteractServerRpc();
        }

        UpdateProgressUI();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateProgressServerRpc(ulong clientId, float progressDelta)
    {
        if (!canInteract.Value) return;

        var newProgress = interactProgress.Value + progressDelta;
        interactProgress.Value = Mathf.Clamp01(newProgress);
        UpdateProgressClientRpc(interactProgress.Value);

        if (interactProgress.Value >= 1f)
        {
            PerformInteraction(clientId);
            InteractClientRpc();
            interactProgress.Value = 0f;
            isInteracting = false;
            Debug.Log("Interaction completed");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetInteractServerRpc()
    {
        interactProgress.Value = 0f;
        UpdateProgressClientRpc(0f);
    }

    [ClientRpc]
    private void UpdateProgressClientRpc(float progress)
    {
        interactProgress.Value = progress;
        UpdateProgressUI();
    }

    [ClientRpc]
    private void InteractClientRpc()
    {
        PerformInteractionEffects();
    }

    private void PerformInteraction(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            SetPlayerActiveServerRpc(clientId, false);
        }

        canInteract.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerActiveServerRpc(ulong clientId, bool active)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            ActivateObjectiveObjects();
        }
    }

    private void ActivateObjectiveObjects()
    {
        if (!IsServer) return;
        foreach (var door in runfunctionOnComplete)
            if (door != null &&
                NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                    door.GetComponent<NetworkObject>().NetworkObjectId, out var networkObject))
                if (networkObject != null)
                {
                    var obj = networkObject.gameObject;
                    var interactable = obj.GetComponent<InteractableObject>();
                    interactable.canInteract.Value = true;
                    SetInteractableClientRpc(networkObject.NetworkObjectId, true);
                }
    }

    [ClientRpc]
    private void SetInteractableClientRpc(ulong networkId, bool canInteract)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var networkObject))
            if (networkObject != null)
                networkObject.gameObject.GetComponent<InteractableObject>().canInteract.Value = canInteract;
    }

    private void PerformInteractionEffects()
    {
        Debug.Log("Interaction effect triggered");
    }

    private void UpdateProgressUI()
    {
        if (progressBar != null)
        {
            progressBar.value = interactProgress.Value;
            progressBar.gameObject.SetActive(interactProgress.Value > 0f);
        }
        else
        {
            Debug.LogWarning("ProgressBar is null");
        }
    }

    private void UpdateProgressBarOrientation()
    {
        if (progressBar != null && Camera.main != null)
            progressBar.transform.LookAt(progressBar.transform.position + Camera.main.transform.forward);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}