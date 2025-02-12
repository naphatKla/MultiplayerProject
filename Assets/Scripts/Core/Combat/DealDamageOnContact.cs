using System;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int damage = 5;

    private ulong ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!col.attachedRigidbody) return;

        if (col.attachedRigidbody.TryGetComponent(out NetworkObject netObject))
        {
            if (ownerClientId == netObject.OwnerClientId)
            {
                return;
            }    
        }
        
        if (col.attachedRigidbody.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
        }
    }
}
