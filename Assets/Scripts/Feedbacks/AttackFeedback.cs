using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace Feedbacks
{
    public class AttackFeedback : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource cameraShakeSource;
        
        public void Play()
        {
            cameraShakeSource.GenerateImpulse();
        }
    }
}
