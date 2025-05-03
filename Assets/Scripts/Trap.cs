using Core.HealthSystems;
using Unity.Netcode;
using UnityEngine;

public class Trap : NetworkBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private float trapDamage = 50f;
    [SerializeField] private float resetTrapTime = 3f;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D triggerCollider;

    private bool isActivated = false;

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
        
        TrapActivateClientRpc();
    }
    
    public void DeactivateTrap()
    {
        Invoke(nameof(ResetTrap), resetTrapTime);
    }

    [ClientRpc]
    private void TrapActivateClientRpc()
    {
        if (animator != null)
            animator.SetTrigger("Activate");
    }

    private void ResetTrap()
    {
        isActivated = false;
    }
    
    // ✅ ปุ่มใน Inspector (คลิกขวา component → Call ToggleTrap)
    [ContextMenu("Toggle Trap")]
    private void ToggleTrap()
    {
        if (isActivated)
        {
            Debug.Log("Trap is active. Resetting...");
            CancelInvoke(nameof(ResetTrap));
            ResetTrap();
        }
        else
        {
            Debug.Log("Trap is inactive. Activating...");
            ActivateTrapFake(); // ใช้เวอร์ชันปลอมของ Activate
        }
    }

    // ✅ ใช้สำหรับทดสอบ Activate ผ่านปุ่ม (ไม่ต้องมี Player)
    private void ActivateTrapFake()
    {
        isActivated = true;
        TrapActivateClientRpc();
        Invoke(nameof(ResetTrap), resetTrapTime);
    }
}
