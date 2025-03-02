using System;
using Unity.Netcode;
using UnityEngine;

namespace Core.HealthSystems
{
    public class HealthSystem : NetworkBehaviour
    {
        [field: SerializeField] public float MaxHealth { get; private set; } = 100;
        [SerializeField] public NetworkVariable<float> currentHealth;
        public Action<HealthSystem> onDie;
        private bool isDead;
    
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
            if (isDead) return;
            float newHealth = currentHealth.Value + value;
            currentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);
            
            if (currentHealth.Value > 0f) return;
            Dead();
        }
    
        private void Dead()
        {
            onDie?.Invoke(this);
            isDead = true;
        
            Destroy(gameObject);
        }
    }
}
