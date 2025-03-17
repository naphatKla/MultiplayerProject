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
        private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Components")]
        [SerializeField] private Animator animator;

        public override void OnNetworkSpawn()
        {
            if(!IsServer) return;
            currentHealth.Value = MaxHealth;
        }

        public void TakeDamage(float damageValue)
        {
            if (!IsServer) return;
            ModifyHealth(-damageValue);
            TakeDamageOnClientRpc();
        }

        [ClientRpc]
        public void TakeDamageOnClientRpc()
        {
            //if (!IsServer) return;
            
            animator.SetTrigger("isHurt");
            onTakeDamage?.Invoke();
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
            
            if (currentHealth.Value > 0f && !isDead.Value) return;
            DeadOnServerRpc();
        }

        [ServerRpc]
        private void DeadOnServerRpc()
        {
            DeadOnClientRpc();
        }

        [ClientRpc]
        private void DeadOnClientRpc()
        {
            animator.SetTrigger("isDead");
            onDie?.Invoke(this);
            isDead.Value = true;

            Destroy(gameObject);
        }
    }
}
