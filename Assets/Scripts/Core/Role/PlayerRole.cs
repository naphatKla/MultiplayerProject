using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

public abstract class PlayerRole : NetworkBehaviour
{
    [field: SerializeField] protected RoleManager roleManager;
    public RoleManager RoleManager { get => roleManager ;
        set => roleManager = value; }

    [SerializeField] protected Role currentRole = Role.NullRole;
    private Role CurrentRole { get => currentRole ;
        set => currentRole = value; }
    protected abstract void UpdateRole(Role role);
    
    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitForRoleManager());
    }

    private IEnumerator WaitForRoleManager()
    {
        // Wait until roleManager is assigned
        yield return new WaitUntil(() => roleManager != null);
        yield return new WaitUntil(() => roleManager.ReadyToSet);

        // Now it's safe to call SetRole
        if (!IsOwner) yield break;
        Debug.Log($"OwnerID {OwnerClientId}");
        SetRoleToClientRpc(OwnerClientId);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void SetRoleToClientRpc(ulong myId)
    {
        Debug.Log($"Role Setting Complete");
        if (currentRole != Role.NullRole)
        {
            return;
        }

        CurrentRole = roleManager.PlayerRoles[myId];
        UpdateRole(CurrentRole);
        ShuffleRoleOutput();
    }

    private void ShuffleRoleOutput()
    {
        if (!IsOwner) return;
        
        if (currentRole == Role.NullRole) return;
        switch (currentRole)
        {
            case Role.Monster:
                RandomRoleUI.instance.StartShuffle(currentRole.ToString());
                break;
            case Role.Explorer:
                RandomRoleUI.instance.StartShuffle(GetComponent<ExplorerRole>().ExplorerClass.ToString());
                break;
            case Role.NullRole:
                Debug.LogError("No rule assigned!");
                break;
        }
    }
}
