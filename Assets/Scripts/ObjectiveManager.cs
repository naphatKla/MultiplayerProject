using System;
using Unity.Netcode;
using UnityEngine;

public class ObjectiveManager : NetworkBehaviour
{
    public static ObjectiveManager Instance { get; private set; }
    
    public NetworkVariable<float> objective = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public void ObjectiveAdd()
    {
        Debug.Log("Objective: " + objective.Value);
        IncreaseValueServerRpc();
    }
    
    public void ObjectiveRemove()
    {
        Debug.Log("Objective: " + objective.Value);
        RemoveValueServerRpc();
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void IncreaseValueServerRpc()
    {
        objective.Value += 1f;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RemoveValueServerRpc()
    {
        objective.Value -= 1f;
    }
    
    private void OnDestroy()
    {
        if (Instance == this) { Instance = null; }
    }
}
