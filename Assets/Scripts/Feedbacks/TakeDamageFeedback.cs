using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class TakeDamageFeedback : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource cameraShakeSource;
    [SerializeField] private SpriteRenderer bodySprite;
        
    public void Play()
    {
        cameraShakeSource.GenerateImpulse();
        bodySprite.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
    }
}
