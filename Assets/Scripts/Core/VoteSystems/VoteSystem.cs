using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Core.VoteSystems
{
    public class VoteSystem : MonoBehaviour
    {
        [SerializeField] private float approvalRate = 0.5f;
        [Header("UI")] [SerializeField] private VoteUI voteUIPrefab;
        [SerializeField] private Canvas voteUICanvas;
        private List<VoteUI> voteUIObjectList;
        private int _currentVoteRate;
        private int _maxVoteCount = 4;
        private int _currentVoteCount;
        private List<ulong> voterIDList;
        public Action onVoteAction;
        private bool _isInitialized;
        
        public void AddVote(ulong voterId)
        {
            if (!_isInitialized)
            {
                InitializeUI();
            }
            if (voterIDList.Contains(voterId)) return; // this player have already voted
            
            voteUIObjectList[_currentVoteCount].PlayCheck();
            _currentVoteCount = Mathf.Clamp(_currentVoteCount + 1, 0, _maxVoteCount);
            _currentVoteRate = _currentVoteCount / _maxVoteCount;
            
            if (_currentVoteRate < approvalRate) return;
            VoteActionPerform();
        }

        private void InitializeUI()
        {
            for (int i = 0; i < _maxVoteCount; i++)
            {
                voteUIObjectList.Add(Instantiate(voteUIPrefab, voteUICanvas.transform));
                voteUIPrefab.Initialize();
            }

            _isInitialized = true;
        }

        private void VoteActionPerform()
        {
            onVoteAction?.Invoke();
        }

        private void ResetVote()
        {
            _isInitialized = false;
            foreach (VoteUI voteUI in voteUIObjectList)
                Destroy(voteUI.gameObject);
            voteUIObjectList.Clear();
        }
    }
}
