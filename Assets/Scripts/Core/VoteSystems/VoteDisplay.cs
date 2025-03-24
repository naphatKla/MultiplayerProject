using System.Collections.Generic;
using Core.VoteSystems.UI;
using UnityEngine;

namespace Core.VoteSystems
{
    public class VoteDisplay : MonoBehaviour
    {
        [Header("Dependencies")] [SerializeField]
        private VoteSystem voteSystem;

        [SerializeField] private Transform voteUIParent;
        [SerializeField] private Transform voteCountdownUIParent;

        [Header("UI Prefab")] 
        [SerializeField] private VoteUI voteUIPrefab;
        [SerializeField] private VoteCountdownUI voteCountdownUIPrefab;

        private readonly List<VoteUI> _voteUIInstances = new List<VoteUI>();
        private VoteCountdownUI _countdownUIInstance;

        private void OnEnable()
        {
            voteSystem.OnVoteStart += InitializeUI;
            voteSystem.OnVoteReceive += AddVoteDisplay;
            voteSystem.OnVoteProgressCountdown += UpdateCountdownProgression;
            voteSystem.OnVoteReset += ResetAndCloseAllUI;
        }

        private void OnDisable()
        {
            voteSystem.OnVoteStart -= InitializeUI;
            voteSystem.OnVoteReceive -= AddVoteDisplay;
            voteSystem.OnVoteProgressCountdown -= UpdateCountdownProgression;
            voteSystem.OnVoteReset -= ResetAndCloseAllUI;
        }

        private void InitializeUI(int maxCount)
        {
            var instantiatedCount = _voteUIInstances.Count;
            for (var i = 0; i < maxCount; i++)
            {
                if (i >= instantiatedCount)
                {
                    var newVoteUIObj = Instantiate(voteUIPrefab, voteUIParent);
                    _voteUIInstances.Add(newVoteUIObj);
                }

                _voteUIInstances[i].gameObject.SetActive(true);
                _voteUIInstances[i].PlayBorderAnimation();
            }

            if (!_countdownUIInstance)
                _countdownUIInstance = Instantiate(voteCountdownUIPrefab, voteCountdownUIParent);

            _countdownUIInstance.gameObject.SetActive(true);
            _countdownUIInstance.PlayStartAnimation();
        }

        private void AddVoteDisplay(int currentIndex)
        {
            _voteUIInstances[currentIndex].PlayBorderAnimation();
            _voteUIInstances[currentIndex].PlayCheckAnimation();
        }

        private void UpdateCountdownProgression(float progression)
        {
            if (!_countdownUIInstance) return;
            _countdownUIInstance.UpdateProgression(progression);
        }

    private void ResetAndCloseAllUI()
        {
            foreach (var instance in _voteUIInstances)
            {
                instance.ResetUI();
                instance.gameObject.SetActive(false);
            }
            
            _countdownUIInstance.ResetUI();
            _countdownUIInstance.gameObject.SetActive(false);
        }
    }
}
