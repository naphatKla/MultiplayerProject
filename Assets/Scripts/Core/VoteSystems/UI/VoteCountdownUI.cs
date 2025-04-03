using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


namespace Core.VoteSystems.UI
{
    public class VoteCountdownUI : MonoBehaviour
    {
        [SerializeField] private Image timeCountdownUI;
        private Vector2 _originalScale = Vector2.one;
        private Color _originalColor = Color.white;
        private Tween _startAnimationTween;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _originalColor = timeCountdownUI.color;
            ResetUI();
        }
        
        public void PlayStartAnimation()
        {
            _startAnimationTween.Kill();
            _startAnimationTween = transform.DOScale(1.15f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        
        // progression = 0-1
        public void UpdateProgression(float progression)
        {
            timeCountdownUI.fillAmount = progression;
            timeCountdownUI.color = Color.Lerp(_originalColor, Color.red, 1-progression);
        }

        public void ResetUI()
        {
            _startAnimationTween.Kill();
            timeCountdownUI.fillAmount = 1f;
            transform.localScale = _originalScale;
            timeCountdownUI.color = _originalColor;
        }
    }
}
