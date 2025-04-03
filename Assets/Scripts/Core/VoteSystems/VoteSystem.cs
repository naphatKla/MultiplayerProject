using System;
using System.Collections;
using System.Collections.Generic;
using Core.HealthSystems;
using Unity.Netcode;
using UnityEngine;

namespace Core.VoteSystems
{
    public class VoteSystem : NetworkBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private HealthSystem healthSystem;
        
        [Header("")]
        [SerializeField] private float approvalRate = 0.5f;
        [SerializeField] private float voteDuration = 15f;
        [SerializeField] private float hpOnRevive = 20f;
        private readonly List<ulong> _voterIDList = new List<ulong>();
        private int _maxVoteCount;
        private int _currentVoteCount;
        private bool _isVoteStarted;
   
        public Action<int> OnVoteStart { get; set; }
        public Action<int> OnVoteReceive { get; set; }
        public Action<float> OnVoteProgressCountdown { get; set; } 
        public Action OnVoteSucceed { get; set; }
        public Action OnVoteReset { get; set; }

        private void OnEnable()
        {
            healthSystem.OnDown += StartVote;
            healthSystem.OnTakeDamageFromPlayer += AddVote;
        }

        private void OnDisable()
        {
            healthSystem.OnDown -= StartVote;
            healthSystem.OnTakeDamageFromPlayer -= AddVote;
        }

        private void StartVote()
        {
            if (_isVoteStarted) return;
            _isVoteStarted = true;
            _maxVoteCount = GetActivePlayer() - 1;
            if (_maxVoteCount <= 1)
            {
                _isVoteStarted = false;
                OnVoteSucceed?.Invoke();
                healthSystem.Dead();
                return;
            }
            OnVoteStart?.Invoke(GetActivePlayer() - 1);
            StartCoroutine(VoteUpdateProgress());
        }

        private void AddVote(ulong voterId)
        {
            if (!_isVoteStarted) return;
            if (_voterIDList.Contains(voterId)) return; // this player have already voted
            OnVoteReceive?.Invoke(_currentVoteCount);
            _currentVoteCount = Mathf.Clamp(_currentVoteCount + 1, 0, _maxVoteCount);
            _voterIDList.Add(voterId);
            
            float currentVoteRate = (float)_currentVoteCount / _maxVoteCount;
            if (currentVoteRate < approvalRate) return;
            OnVoteSucceed?.Invoke();
            healthSystem.Dead();
        }
        
        [ClientRpc]
        private void ResetVoteClientRPC()
        {
            _voterIDList.Clear();
            OnVoteReset?.Invoke();
            _isVoteStarted = false;
            healthSystem.Revive(hpOnRevive);
        }

        private int GetActivePlayer()
        {
            int playerAliveCount = 0;

            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                var client = kvp.Value;
                if (!client?.PlayerObject) continue;
                if (!client.PlayerObject.gameObject.activeSelf) continue;
                playerAliveCount++;
            }

            return playerAliveCount;
        }

        private IEnumerator VoteUpdateProgress()
        {
            float timeCount = voteDuration;
            while (timeCount > 0)
            {
                float timeProgressionLeft = timeCount / voteDuration;
                OnVoteProgressCountdown?.Invoke(timeProgressionLeft);
                yield return new WaitForSeconds(0.2f);
                timeCount -= 0.2f;
            }
            ResetVoteClientRPC();
        }
    }
}
