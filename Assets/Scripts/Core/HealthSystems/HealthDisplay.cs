using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Core.HealthSystems
{
    public class HealthDisplay : NetworkBehaviour
    {
        [Header("References")] [SerializeField] private HealthSystem health;
        [SerializeField] private Image healthBarImage;
        [SerializeField] private Image healthBarBgImage;
        [SerializeField] private bool showToEveryone;

        public override void OnNetworkSpawn()
        {
            if (!IsClient || health == null || healthBarImage == null) return;
        
            if (!IsOwner && !showToEveryone)
            {
                healthBarImage.gameObject.SetActive(false);
                healthBarBgImage.gameObject.SetActive(false);
                return;
            }
            health.currentHealth.OnValueChanged += HandleHealthChanged;
            HandleHealthChanged(0,health.currentHealth.Value);
        }

        public override void OnNetworkDespawn()
        {
            if (!IsClient) return;
            health.currentHealth.OnValueChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float oldHealth, float newHealth)
        {
            healthBarImage.fillAmount = (float)newHealth / health.MaxHealth;
        }
    }
}

