using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Core.HealthSystems
{
    public class HealthDisplay : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem health;
        [SerializeField] private Image healthBarImage;
        [SerializeField] private Image healthBarBgImage;
        [SerializeField] private bool showToEveryone = true; //For Enemy

        public override void OnNetworkSpawn()
        {
            if (!IsClient || health == null || healthBarImage == null) return;

            // Health Bar Enemy
            if (!showToEveryone && !IsOwner)
            {
                healthBarImage.gameObject.SetActive(false);
                healthBarBgImage.gameObject.SetActive(false);
                return;
            }

            health.currentHealth.OnValueChanged += HandleHealthChanged;
            HandleHealthChanged(0, health.currentHealth.Value);
        }

        public override void OnNetworkDespawn()
        {
            if (!IsClient || health == null) return;
            health.currentHealth.OnValueChanged -= HandleHealthChanged;
            
            if (healthBarImage != null) healthBarImage.gameObject.SetActive(false);
            if (healthBarBgImage != null) healthBarBgImage.gameObject.SetActive(false);
        }

        private void HandleHealthChanged(float oldHealth, float newHealth)
        {
            float healthPercentage = newHealth / health.MaxHealth;
            healthBarImage.fillAmount = healthPercentage;
            
            bool isFullHealth = Mathf.Approximately(healthPercentage, 1f);
            healthBarImage.gameObject.SetActive(!isFullHealth);
            if (healthBarBgImage != null) healthBarBgImage.gameObject.SetActive(!isFullHealth);
        }
    }
}