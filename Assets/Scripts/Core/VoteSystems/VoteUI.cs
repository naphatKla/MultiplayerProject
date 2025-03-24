using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.VoteSystems
{
    public class VoteUI : MonoBehaviour
    {
        [SerializeField] private Image checkUIImage;
        private Vector2 originalScale = Vector2.one;
        private Vector2 checkOriginalScale = Vector2.one;

        private void Awake()
        {
            originalScale = transform.localScale;
            checkOriginalScale = checkUIImage.transform.localScale;
            ResetUI();
        }
        
        public void PlayBorderAnimation()
        {
            gameObject.SetActive(true);
            transform.localScale = Vector2.zero;
            transform.DOScale(originalScale.x, 0.5f).SetEase(Ease.OutBounce);
        }

        public void PlayCheckAnimation()
        {
            gameObject.SetActive(true);
            checkUIImage.gameObject.SetActive(true);
            checkUIImage.transform.localScale = Vector2.zero;
            checkUIImage.transform.DOScale(checkOriginalScale.x, 0.5f).SetEase(Ease.OutBounce);
        }

        public void ResetUI()
        {
            transform.localScale = originalScale;
            checkUIImage.transform.localScale = checkOriginalScale;
            checkUIImage.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
