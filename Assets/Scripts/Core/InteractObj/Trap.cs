using Core.HealthSystems;
using Unity.Netcode;
using UnityEngine;

public class Trap : NetworkBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private float trapDamage = 50f;
    [SerializeField] private float resetTrapTime = 3f;

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D triggerCollider;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite activatedSprite;

    private bool isActivated = false;
    private bool isResetting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer || isActivated) return;

        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<HealthSystem>(out var health))
        {
            ActivateTrap(health);
        }
    }

    private void ActivateTrap(HealthSystem targetHealth)
    {
        isActivated = true;
        targetHealth.TakeDamage(trapDamage);
        
        TrapActivateClientRpc(true);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestResetTrapServerRpc()
    {
        if (!isActivated || isResetting) return;

        isResetting = true;
        Debug.Log("Resetting trap...");
        Invoke(nameof(FinalizeResetTrap), resetTrapTime);
    }

    private void FinalizeResetTrap()
    {
        isActivated = false;
        isResetting = false;
        TrapActivateClientRpc(false);
        Debug.Log("Trap has been reset.");
    }

    [ClientRpc]
    private void TrapActivateClientRpc(bool activated)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = activated ? activatedSprite : idleSprite;
        }
    }
}