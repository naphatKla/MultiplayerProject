using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

public abstract class PlayerRole : NetworkBehaviour
{
    [SerializeField] protected Role currentRole = Role.Null;
    public Role CurrentRole { get => currentRole ; private set => currentRole = value; }

    public RoleManager roleManager;
    protected abstract void UpdateRole(Role role);

    

    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitForRoleManager());
    }

    private IEnumerator WaitForRoleManager()
    {
        // Wait until roleManager is assigned
        while (roleManager == null)
        {
            yield return null; // Wait one frame
        }
        while (!roleManager.ReadToSet)
        {
            yield return null; // Wait one frame
        }
        // Now it's safe to call SetRole
        if (IsOwner)
        {
            Debug.Log($"OwnerID {OwnerClientId}");
            SetRoleToClientRpc(OwnerClientId);
        }
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void SetRoleToClientRpc(ulong myId)
    {
        Debug.Log($"Role Setting Complete");
        if (currentRole != Role.Null)
        {
            return;
        }
        CurrentRole = roleManager.PlayerRoles[myId];
        UpdateRole(CurrentRole);
        ShuffleRoleOutput();
    }

    protected void ShuffleRoleOutput()
    {
        if (!IsOwner)
        {
            return;
        }
        if (currentRole != Role.Null)
        {
            switch (currentRole)
            {
                case Role.Monster:
                    RandomRoleUI.instance.StartShuffle(currentRole.ToString());
                    break;
                case Role.Explorer:
                    RandomRoleUI.instance.StartShuffle(GetComponent<ExplorerRole>().ExplorerClass.ToString());
                    break;
            }
            
        }
    }
}
