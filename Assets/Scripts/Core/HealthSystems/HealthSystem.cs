using System;
using Unity.Netcode;
using UnityEngine;

public class HealthSystem : NetworkBehaviour
{
    [field: SerializeField] public float MaxHealth { get; private set; } = 100;
    [SerializeField] public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    private bool _isDead = false;
    public Action<HealthSystem> onDie;
    
    public override void OnNetworkSpawn()
    {
        if(!IsServer) {return;}

        currentHealth.Value = MaxHealth;
    }
    
    public void TakeDamage(float damageValue)
    {
        ModifyHealth(-damageValue);
    }
    
    public void RestoreHealth(float healValue)
    {
        ModifyHealth(healValue);
    }
    
    private void ModifyHealth(float value)
    {
        if (_isDead) return;
        float newHealth = currentHealth.Value + value;
        currentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);
        
        if (currentHealth.Value <= 0f)
        {
            Dead();
        }
    }
    
    private void Dead()
    {
        onDie?.Invoke(this);
        _isDead = true;
        
        Destroy(gameObject);
    }
    
}
