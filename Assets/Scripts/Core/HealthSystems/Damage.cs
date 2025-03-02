using System;
using UnityEngine;
using Unity.Netcode;

public class Damage : MonoBehaviour
{
    [SerializeField] private float damage = 10;
    private ulong _ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this._ownerClientId = ownerClientId;
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!col.attachedRigidbody) return;

        if (col.attachedRigidbody.TryGetComponent(out NetworkObject netObject))
        {
            if (_ownerClientId == netObject.OwnerClientId)
            {
                return;
            }    
        }
        
        if (col.attachedRigidbody.TryGetComponent(out HealthSystem health))
        {
            health.TakeDamage(damage);
        }
    }
}
