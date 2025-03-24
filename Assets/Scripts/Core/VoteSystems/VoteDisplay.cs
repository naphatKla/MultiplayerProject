using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.VoteSystems
{
    public class VoteDisplay : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private VoteSystem voteSystem;
        [SerializeField] private Canvas canvasUI;

        [Header("UI")] 
        [SerializeField] private VoteUI voteUIPrefab;
        [SerializeField] private Image voteTimeCountdownUI;
        private readonly List<VoteUI> _voteUIInstances = new List<VoteUI>();
        
        private void OnEnable()
        {
            voteSystem.OnVoteStart += InitializeUI;
            voteSystem.OnVoteReceive += AddVoteDisplay;
            voteSystem.OnVoteProgressCountdown += UpdateTimeCountdownUI;
            voteSystem.OnVoteReset += ResetAllUI;
        }

        private void OnDisable()
        {
            voteSystem.OnVoteStart -= InitializeUI;
            voteSystem.OnVoteReceive -= AddVoteDisplay;
            voteSystem.OnVoteProgressCountdown -= UpdateTimeCountdownUI;
            voteSystem.OnVoteReset -= ResetAllUI;
        }

        private void InitializeUI(int maxCount)
        {
            var instantiatedCount = _voteUIInstances.Count;
            for (var i = 0; i < maxCount; i++)
            {
                if (i >= instantiatedCount)
                {
                    var newVoteUIObj = Instantiate(voteUIPrefab, canvasUI.transform);
                    _voteUIInstances.Add(newVoteUIObj);
                }
                _voteUIInstances[i].PlayBorderAnimation();
            }
        }

        private void AddVoteDisplay(int currentIndex)
        {
            _voteUIInstances[currentIndex].PlayBorderAnimation();
            _voteUIInstances[currentIndex].PlayCheckAnimation();
        }

        private void ResetAllUI()
        {
            foreach (var instance in _voteUIInstances)
                instance.ResetUI();
        }

        private void UpdateTimeCountdownUI(float timeProgression)
        {
            voteTimeCountdownUI.fillAmount = timeProgression;
        }
    }
}
