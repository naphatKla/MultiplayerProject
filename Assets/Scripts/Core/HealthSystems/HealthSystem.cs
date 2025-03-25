using System;
using Unity.Netcode;
using UnityEngine;

namespace Core.HealthSystems
{
    public class HealthSystem : NetworkBehaviour
    {
        [field: SerializeField] public float MaxHealth { get; private set; } = 100;
        [SerializeField] public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
        
        public Action OnDie { get; set; }
        public Action<ulong> OnTakeDamageFromPlayer { get; set; }
        public Action onTakeDamage;
        private NetworkVariable<bool> isDead = new NetworkVariable<bool>();

        [Header("Components")]
        [SerializeField] private Animator animator;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            currentHealth.Value = MaxHealth;
        }
        
        public void TakeDamage(float damageValue, ulong? attackerID = null)
        {
            if (!IsServer) return;
            ModifyHealth(-damageValue);
            
            if (attackerID == null)
            {
                TakeDamageOnClientRpc();
                return;
            }
            TakeDamageOnClientRpc(attackerID.Value);
        }
        
        [ClientRpc]
        private void TakeDamageOnClientRpc()
        {
            if (animator)
                animator.SetTrigger("isHurt");
            
            onTakeDamage?.Invoke();
        }
        
        [ClientRpc]
        private void TakeDamageOnClientRpc(ulong attackerID)
        {
            TakeDamageOnClientRpc();
            OnTakeDamageFromPlayer?.Invoke(attackerID);
        }

        public void RestoreHealth(float healValue)
        {
            if (!IsServer) return;
            ModifyHealth(healValue);
        }

        private void ModifyHealth(float value)
        {
            if (isDead.Value) return;
            float newHealth = currentHealth.Value + value;
            currentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

            if (currentHealth.Value > 0f) return;
            isDead.Value = true;
            DeadOnClientRpc();
        }
        
        [ClientRpc]
        private void DeadOnClientRpc()
        {
            if (animator)
                animator.SetTrigger("isDead");
            
            Debug.LogWarning("DIE");
            OnDie?.Invoke();
        }
    }
}