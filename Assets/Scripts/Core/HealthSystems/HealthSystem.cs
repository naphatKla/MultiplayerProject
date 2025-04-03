using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace Core.HealthSystems
{
    public class HealthSystem : NetworkBehaviour
    {
        [field: SerializeField] public float MaxHealth { get; private set; } = 100;
        [SerializeField] public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
        [SerializeField] private List<Behaviour> disableOnDown;
        
        public Action OnDown { get; set; }
        public Action OnDie { get; set;}
        public Action onTakeDamage;
        public Action<ulong> OnTakeDamageFromPlayer { get; set; }
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
            DownOnClientRpc();
        }
        
        public void Revive(float hpOnRevive)
        {
            foreach (var behaviour in disableOnDown)
                behaviour.enabled = true;
            
            if (!IsOwner) return;
            ReviveServerRPC(hpOnRevive);
        }

        [ServerRpc]
        private void ReviveServerRPC(float hpOnRevive)
        {
            isDead.Value = false;
            ModifyHealth(hpOnRevive);
        }

        public void Dead()
        {
            if (!IsOwner) return;
            DeadServerRPC();
        }

        [ServerRpc]
        private void DeadServerRPC()
        {
            isDead.Value = true;
            DeadOnClientRpc();
        }
        
        
        [ClientRpc]
        private void DownOnClientRpc()
        {    
            foreach (var behaviour in disableOnDown)
                behaviour.enabled = false;
            
            OnDown?.Invoke();
        }
        
        [ClientRpc]
        private void DeadOnClientRpc()
        {
            if (animator)
                animator.SetTrigger("isDead");
            
            OnDie?.Invoke();
            DOVirtual.DelayedCall(0.5f, () =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}