using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private CinemachineCamera cinemachineMainCamera;
    private bool _isSpectatorMode;
    private int _cameraIndex;

    private void FixedUpdate()
    {
        if (!_isSpectatorMode) return;
        SpectatorCameraHandler();
    }
    
    public void SetCameraFollowTarget(Transform target)
    {
        cinemachineMainCamera.Follow = target;
    }
    
    private void SpectatorCameraHandler()
    {
        if (!UnityEngine.Input.GetMouseButtonDown(0)) return;
        ChangeCameraToNextAlivePlayer();
    }

    public void ChangeCameraToNextAlivePlayer()
    {
        List<NetworkObject> playerAlive = GetActivePlayers();
        if (playerAlive.Count <= 0) return;
        _cameraIndex++;
        if (_cameraIndex >= playerAlive.Count) _cameraIndex = 0;
        _cameraIndex = Mathf.Clamp(_cameraIndex, 0, playerAlive.Count - 1);
        SetCameraFollowTarget(playerAlive[_cameraIndex].transform);
    }

    private List<NetworkObject> GetActivePlayers()
    {
        List<NetworkObject> activePlayers = new List<NetworkObject>();

        foreach (var clientPair in NetworkManager.Singleton.ConnectedClients)
        {
            var client = clientPair.Value;
            var playerObj = client.PlayerObject;

            if (playerObj != null && playerObj.gameObject.activeSelf)
            {
                activePlayers.Add(playerObj);
            }
        }

        return activePlayers;
    }

    public void StartSpectatorMode()
    {
        _isSpectatorMode = true;
    }
}
