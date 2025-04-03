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
        private Tween _spriteTween;
        private Tween _redScreenTween;

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
            if(_spriteTween.IsActive()) return;
            _spriteTween = bodySprite.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
        }

        [Rpc(SendTo.Owner)]
        private void PlayOnlyOwnerRpc()
        {
            cameraShakeSource.GenerateImpulse();
            if (_redScreenTween.IsActive()) return;
            _redScreenTween = redScreenFlashUI.DOFade(0.5f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }
}
