using Core.HealthSystems;
using DG.Tweening;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Feedbacks
{
    public class TakeDamageFeedback : NetworkBehaviour
    {
        [SerializeField] private CinemachineImpulseSource cameraShakeSource;
        [SerializeField] private SpriteRenderer bodySprite;
        [SerializeField] private Image redScreenFlashUI;
        [SerializeField] private HealthSystem healthSystem;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            healthSystem.onTakeDamage += PlayToEveryClientRpc;
            healthSystem.onTakeDamage += PlayOnlyOwnerRpc;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            healthSystem.onTakeDamage -= PlayToEveryClientRpc;
            healthSystem.onTakeDamage -= PlayOnlyOwnerRpc;
        }

        [ClientRpc]
        private void PlayToEveryClientRpc()
        {
            bodySprite.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
        }

        [Rpc(SendTo.Owner)]
        private void PlayOnlyOwnerRpc()
        {
            cameraShakeSource.GenerateImpulse();
            redScreenFlashUI.DOFade(0.5f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }
}
