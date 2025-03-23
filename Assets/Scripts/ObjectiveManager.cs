using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectiveManager : NetworkBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public NetworkVariable<float> objective = new();

    public NetworkVariable<float> maxObjective = new(4f);

    private NetworkList<ulong> objectiveCompleteObjects;

    [SerializeField] private List<GameObject> objectsToActivateOnComplete = new();

    public bool CheckObjectiveIsDone()
    {
        if (objective.Value >= maxObjective.Value)
        {
            ActivateObjectiveObjects();
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

    private void Start()
    {
        if (IsServer)
            foreach (var obj in objectsToActivateOnComplete)
                if (obj != null && obj.GetComponent<NetworkObject>() != null)
                    objectiveCompleteObjects.Add(obj.GetComponent<NetworkObject>().NetworkObjectId);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        objectiveCompleteObjects = new NetworkList<ulong>(new ulong[] { });
    }


    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.R)) IncreaseValueServerRpc();
        if (IsServer) UpdateMaxObjective();
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseValueServerRpc()
    {
        objective.Value += 1f;
        CheckObjectiveIsDone();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveValueServerRpc()
    {
        if (objective.Value > 0) objective.Value -= 1f;
    }

    private void UpdateMaxObjective()
    {
        var playerCount = NetworkManager.Singleton.ConnectedClients.Count;

        //Start 4
        var baseMax = 4f;
        //Calculate additional players
        float additionalPlayers = Mathf.Max(0, playerCount - 4);
        //Start 4 and plus with additional players
        var newMax = baseMax + Mathf.Floor(additionalPlayers / 2f);
        maxObjective.Value = newMax;

        if (objective.Value > maxObjective.Value) objective.Value = maxObjective.Value;
    }

    private void ActivateObjectiveObjects()
    {
        if (!IsServer) return;
        Debug.Log("Activate");
        foreach (var networkId in objectiveCompleteObjects)
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var networkObject))
                if (networkObject != null)
                {
                    var obj = networkObject.gameObject;
                    if (obj.activeSelf) obj.SetActive(false);
                }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}