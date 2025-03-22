using System;
using Unity.Netcode;
using UnityEngine;

public class ObjectiveManager : NetworkBehaviour
{
    public static ObjectiveManager Instance { get; private set; }
    
    public NetworkVariable<float> objective = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public NetworkVariable<float> maxObjective = new NetworkVariable<float>(4f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public bool CheckObjectiveIsDone()
    {
        if (objective.Value >= maxObjective.Value)
        {
            return true;
        }
        return false;
    }
    public void ObjectiveAdd()
    {
        Debug.Log("Objective: " + objective.Value + "/" + maxObjective.Value);
        IncreaseValueServerRpc();
    }
    
    public void ObjectiveRemove()
    {
        Debug.Log("Objective: " + objective.Value + "/" + maxObjective.Value);
        RemoveValueServerRpc();
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    private void Update()
    {
        if (IsServer)
        {
            UpdateMaxObjective();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void IncreaseValueServerRpc()
    {
        objective.Value += 1f;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RemoveValueServerRpc()
    {
        if (objective.Value > 0)
        {
            objective.Value -= 1f;
        }
    }
    
    private void UpdateMaxObjective()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
        
        //Start 4
        float baseMax = 4f;
        //Calculate additional players
        float additionalPlayers = Mathf.Max(0, playerCount - 4);
        //Start 4 and plus with additional players
        float newMax = baseMax + Mathf.Floor(additionalPlayers / 2f);
        maxObjective.Value = newMax;

        if (objective.Value > maxObjective.Value)
        {
            objective.Value = maxObjective.Value;
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this) { Instance = null; }
    }
}
