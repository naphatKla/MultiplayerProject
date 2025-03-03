using System;
using Feedbacks;
using Unity.Netcode;
using UnityEngine;

namespace Core.HealthSystems
{
    public class HealthSystem : NetworkBehaviour
    {
        [field: SerializeField] public float MaxHealth { get; private set; } = 100;
        [SerializeField] public NetworkVariable<float> currentHealth;
        public Action<HealthSystem> onDie;
        public Action onTakeDamage;
        private bool isDead;
    
        public override void OnNetworkSpawn()
        {
            if(!IsServer) return;
            currentHealth.Value = MaxHealth;
        }
        
        public void TakeDamage(float damageValue)
        {
            if (!IsServer) return;
            ModifyHealth(-damageValue);
            onTakeDamage?.Invoke();
        }
    
        public void RestoreHealth(float healValue)
        {
            if (!IsServer) return;
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
