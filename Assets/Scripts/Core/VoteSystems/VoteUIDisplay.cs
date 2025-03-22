using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.VoteSystems
{
    public class VoteUI : MonoBehaviour
    {
        [SerializeField] private Image checkImage;
        private Vector2 originalScale = Vector2.one;
        private Vector2 checkOriginalScale = Vector2.one;

        private void Start()
        {
            originalScale = transform.localScale;
            checkOriginalScale = checkImage.transform.localScale;
        }

        public void Initialize()
        {
            transform.localScale = Vector2.zero;
            transform.DOScale(originalScale.x, 0.3f).SetEase(Ease.InExpo);
        }

        public void PlayCheck()
        {
            checkImage.gameObject.SetActive(true);
            checkImage.transform.localScale = Vector2.zero;
            checkImage.transform.DOScale(checkOriginalScale.x, 0.3f).SetEase(Ease.InExpo);
        }
    }
}
