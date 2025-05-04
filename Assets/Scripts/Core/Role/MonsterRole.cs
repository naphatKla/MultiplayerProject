using Unity.Netcode;
using UnityEngine;

public class MonsterRole : PlayerRole
{
    [SerializeField] private RuntimeAnimatorController mimicAnimatorController; // Mimic's Animator Controller
    private bool isActive = true;
    private Animator animator; // Reference to the player's Animator

    public bool IsActive => isActive; // Public property to check if MonsterRole is active
    public bool tranformMimic = false;

    private void Awake()
    {
        // Get the Animator component on the player
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on player!");
        }
    }

    private void Start()
    {
        // Check RoleManager for this player's role and update state
        if (RoleManager.Instance != null && RoleManager.Instance.PlayerRoles != null)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            if (RoleManager.Instance.PlayerRoles.TryGetValue(clientId, out Role assignedRole))
            {
                currentRole = assignedRole; // Update currentRole from PlayerRole
                if (assignedRole != Role.Monster)
                {
                    isActive = false;
                    enabled = false;
                    Debug.Log($"MonsterRole for client {clientId} disabled (Role: {assignedRole})");
                }
                else
                {
                    isActive = true;
                    enabled = true;
                    Debug.Log($"MonsterRole for client {clientId} active (Role: {assignedRole})");
                }
            }
            else
            {
                Debug.LogWarning($"No role found for client {clientId} in RoleManager.PlayerRoles");
                isActive = false;
                enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("RoleManager or PlayerRoles not initialized, disabling MonsterRole");
            isActive = false;
            enabled = false;
        }
    }

    public void TranformMimic()
    {
        if (!IsOwner)
        {
            return;
        }

        if (playermovement == null)
        {
            Debug.LogError("PlayerMovement component not found!");
            return;
        }
        
        playermovement.IsMonster.Value = true;
        playermovement.TransformToMonster(true);
        tranformMimic = true;
        
        var healthSystem = GetComponent<Core.HealthSystems.HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.SetMaxHealth(1000f, true);
        }
        
        TransformMimicServerRpc();
    }

    [ServerRpc]
    private void TransformMimicServerRpc()
    {
        TransformMimicClientRpc();
    }

    [ClientRpc]
    private void TransformMimicClientRpc()
    {
        if (animator != null && mimicAnimatorController != null)
        {
            animator.runtimeAnimatorController = mimicAnimatorController;
            Debug.Log("Animator switched to Mimic controller");
        }
    }

    void Update()
    {
        if (!isActive) return;
        if (currentRole != Role.Monster)
        {
            return;
        }
    }

    protected override void UpdateRole(Role role)
    {
        if (role != Role.Monster)
        {
            enabled = false;
            isActive = false;
            Debug.Log($"MonsterRole disabled via UpdateRole (Role: {role})");
        }
    }
}