using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.VoteSystems
{
    public class VoteSystem : MonoBehaviour
    {
        [SerializeField] private float approvalRate = 0.5f;
        private int _currentVoteRate;
        private int _maxVoteCount = 4;
        private int _currentVoteCount;
        private List<ulong> voterIDList;
        public Action onVoteAction;
            
        public void AddVote(ulong voterId)
        {
            if (voterIDList.Contains(voterId)) return; // this player have already voted
            _currentVoteCount = Mathf.Clamp(_currentVoteCount + 1, 0, _maxVoteCount);
            _currentVoteRate = _currentVoteCount / _maxVoteCount;
            
            if (_currentVoteRate < approvalRate) return;
            VoteActionPerform();
        }

        private void VoteActionPerform()
        {
            onVoteAction?.Invoke();
        }
    }
}
