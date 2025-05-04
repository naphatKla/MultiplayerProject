using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace Core.HealthSystems
{
    public class HealthSystem : NetworkBehaviour
    {
        [SerializeField] private float maxHealth = 100f; // Default max health
        public float MaxHealth { get => maxHealth; private set => maxHealth = value; }
        [SerializeField] public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
        [SerializeField] private List<Behaviour> disableOnDown;
        
        public Action OnDown { get; set; }
        public Action OnDie { get; set; }
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

        public void SetMaxHealth(float newMaxHealth, bool setCurrentHealth = true)
        {
            if (!IsOwner) return;
            SetMaxHealthServerRpc(newMaxHealth, setCurrentHealth);
        }

        [ServerRpc]
        private void SetMaxHealthServerRpc(float newMaxHealth, bool setCurrentHealth)
        {
            MaxHealth = newMaxHealth;
            if (setCurrentHealth)
            {
                currentHealth.Value = newMaxHealth;
            }
            else
            {
                currentHealth.Value = Mathf.Min(currentHealth.Value, newMaxHealth);
            }
            SetMaxHealthClientRpc(newMaxHealth);
        }

        [ClientRpc]
        private void SetMaxHealthClientRpc(float newMaxHealth)
        {
            MaxHealth = newMaxHealth;
            Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} updated MaxHealth to {newMaxHealth}");
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
            ReviveServerRpc(hpOnRevive);
        }

        [ServerRpc]
        private void ReviveServerRpc(float hpOnRevive)
        {
            isDead.Value = false;
            ModifyHealth(hpOnRevive);
        }

        public void Dead()
        {
            if (!IsOwner) return;
            DeadServerRpc();
            GameHUD.Instance.deathUI.SetActive(true);
        }

        [ServerRpc]
        private void DeadServerRpc()
        {
            isDead.Value = true;
            RoleManager.Instance.UpdatePlayerActiveStatus(OwnerClientId, false);
            DeadOnClientRpc();
        }
        
        public void SetActiveFalse()
        {
            if (!IsOwner) return;
            SetActiveFalseServerRpc();
            GameHUD.Instance.winUI.SetActive(true);
        }

        [ServerRpc]
        private void SetActiveFalseServerRpc()
        {
            isDead.Value = true;
            SetActiveOnClientRpc();
        }
        
        [ClientRpc]
        private void DownOnClientRpc()
        {
            if (LayerMask.LayerToName(gameObject.layer) == "Enemy")
            {
                Destroy(gameObject);
                return;
            }

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
                if (!IsOwner) return;
                CameraManager.Instance.StartSpectatorMode();
            });
        }
        
        [ClientRpc]
        private void SetActiveOnClientRpc()
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                gameObject.SetActive(false);
                if (!IsOwner) return;
                CameraManager.Instance.StartSpectatorMode();
            });
        }
    }
}