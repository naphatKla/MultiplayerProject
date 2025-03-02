using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class TakeDamageFeedback : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource cameraShakeSource;
    [SerializeField] private SpriteRenderer bodySprite;
    [SerializeField] private Image redScreenFlashUI;
    
    public void Play()
    {
        cameraShakeSource.GenerateImpulse();
        bodySprite.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
        redScreenFlashUI.DOFade(0.5f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }
}
