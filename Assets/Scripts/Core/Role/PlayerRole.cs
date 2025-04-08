using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

public abstract class PlayerRole : NetworkBehaviour
{
    [SerializeField] protected Role currentRole = Role.Null;

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
        yield return new WaitUntil(() => RoleManager.Instance != null);
        yield return new WaitUntil(() => RoleManager.Instance.ReadyToSet);

        // Now it's safe to call SetRole
        if (!IsOwner) yield break;
        Debug.Log($"OwnerID {OwnerClientId}");
        SetRoleToClientRpc(OwnerClientId);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void SetRoleToClientRpc(ulong myId)
    {
        Debug.Log($"Role Setting Complete");
        if (currentRole != Role.Null)
        {
            return;
        }

        CurrentRole = RoleManager.Instance.PlayerRoles[myId];
        UpdateRole(CurrentRole);
        ShuffleRoleOutput();
    }

    private void ShuffleRoleOutput()
    {
        if (!IsOwner) return;
        
        if (currentRole == Role.Null) return;
        switch (currentRole)
        {
            case Role.Monster:
                RandomRoleUI.instance.StartShuffle(currentRole.ToString());
                break;
            case Role.Explorer:
                RandomRoleUI.instance.StartShuffle(GetComponent<ExplorerRole>().ExplorerClass.ToString());
                break;
            case Role.Null:
                Debug.LogError("No rule assigned!");
                break;
        }
    }
}
