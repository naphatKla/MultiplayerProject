using System;
using Unity.Netcode;
using UnityEngine;

namespace Core.HealthSystems
{
    public class HealthSystem : NetworkBehaviour
    {
        [field: SerializeField] public float MaxHealth { get; private set; } = 100;
        [SerializeField] public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
        public Action<HealthSystem> onDie;
        public Action onTakeDamage;
        private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // เปลี่ยนเป็น Server

        [Header("Components")]
        [SerializeField] private Animator animator;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                currentHealth.Value = MaxHealth;
            }
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
            if (animator != null)
            {
                animator.SetTrigger("isHurt");
            }
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

            if (currentHealth.Value > 0f) return;
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
            if (animator != null)
            {
                animator.SetTrigger("isDead");
            }
            onDie?.Invoke(this);
            isDead.Value = true;
            
            if (IsServer)
            {
                Destroy(gameObject, 1f);
            }
        }
    }
}