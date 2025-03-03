using System;
using Core.CombatSystems;
using Unity.Cinemachine;
using UnityEngine;

namespace Feedbacks
{
    public class AttackFeedback : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource cameraShakeSource;
        [SerializeField] private AttackSystem attackSystem;

        private void Start()
        {
            attackSystem.onStartAttack += Play;
        }

        private void Play()
        {
            cameraShakeSource.GenerateImpulse();
        }
    }
}
