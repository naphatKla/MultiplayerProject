using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.VoteSystems.UI
{
    public class VoteUI : MonoBehaviour
    {
        [SerializeField] private Image borderImage;
        [SerializeField] private Image checkImage;
        private Vector2 _borderOriginalScale = Vector2.one;
        private Vector2 _checkOriginalScale = Vector2.one;

        private void Awake()
        {
            _borderOriginalScale = borderImage.transform.localScale;
            _checkOriginalScale = checkImage.transform.localScale;
            ResetUI();
        }
        
        public void PlayBorderAnimation()
        {
            borderImage.gameObject.SetActive(true);
            borderImage.transform.localScale = Vector2.zero;
            borderImage.transform.DOScale(_borderOriginalScale.x, 0.5f).SetEase(Ease.OutBounce);
        }

        public void PlayCheckAnimation()
        {
            checkImage.gameObject.SetActive(true);
            checkImage.transform.localScale = Vector2.zero;
            checkImage.transform.DOScale(_checkOriginalScale.x, 0.5f).SetEase(Ease.OutBounce);
        }

        public void ResetUI()
        {
            borderImage.transform.localScale = _borderOriginalScale;
            checkImage.transform.localScale = _checkOriginalScale;
            checkImage.gameObject.SetActive(false);
            borderImage.gameObject.SetActive(true);
        }
    }
}
