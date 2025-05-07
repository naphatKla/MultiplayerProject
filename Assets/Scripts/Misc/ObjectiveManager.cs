using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ObjectiveManager : NetworkBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public NetworkVariable<float> objective = new();

    public NetworkVariable<float> maxObjective = new(4f);

    private NetworkList<ulong> objectiveCompleteObjects;

    [SerializeField] private List<GameObject> objectsToActivateOnComplete = new();

    [SerializeField] private TMP_Text objectiveText;

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
        SoundEffectManager.Instance.PlayLocal("Objective", 2f);
    }

    public void ObjectiveRemove()
    {
        Debug.Log("Objective: " + objective.Value + "/" + maxObjective.Value);
        RemoveValueServerRpc();
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

        if (objectiveText != null) objectiveText.text = $"Objective: {objective.Value}/{maxObjective.Value}";
    }

    private void Start()
    {
        if (IsServer)
            foreach (var obj in objectsToActivateOnComplete)
                if (obj != null && obj.GetComponent<NetworkObject>() != null)
                    objectiveCompleteObjects.Add(obj.GetComponent<NetworkObject>().NetworkObjectId);

        objective.OnValueChanged += UpdateObjectiveText;
        maxObjective.OnValueChanged += UpdateObjectiveText;
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.O)) IncreaseValueServerRpc();
        if (IsServer) UpdateMaxObjective(); CheckObjectiveIsDone();
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
        if (NetworkManager.Singleton == null) { return; }
        if (NetworkManager.Singleton.ConnectedClients == null) { return; }
        var playerCount = NetworkManager.Singleton.ConnectedClients.Count;
        if (playerCount < 1) return;

        var baseMax = 4f;
        float additionalPlayers = Mathf.Max(0, playerCount - 4);
        var newMax = baseMax + Mathf.Floor(additionalPlayers / 2f);
        maxObjective.Value = newMax;

        if (objective.Value > maxObjective.Value) objective.Value = maxObjective.Value;
        CheckObjectiveIsDone();
    }

    private void ActivateObjectiveObjects()
    {
        if (!IsServer) return;
        foreach (var networkId in objectiveCompleteObjects)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var networkObject))
            {
                if (networkObject != null)
                {
                    var obj = networkObject.gameObject;
                    bool newState = !obj.activeSelf;
                    obj.SetActive(newState);
                    ActivateObjectClientRpc(networkId, newState);
                }
            }
        }
    }

    [ClientRpc]
    private void ActivateObjectClientRpc(ulong networkId, bool state)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var networkObject))
        {
            if (networkObject != null)
            {
                networkObject.gameObject.SetActive(state);
            }
        }
    }

    private void UpdateObjectiveText(float oldValue, float newValue)
    {
        if (objectiveText != null) objectiveText.text = $"Objective: {objective.Value}/{maxObjective.Value}";
    }
    

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        objective.OnValueChanged -= UpdateObjectiveText;
        maxObjective.OnValueChanged -= UpdateObjectiveText;
    }
}