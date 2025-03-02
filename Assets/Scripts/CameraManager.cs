
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private CinemachineCamera cinemachineMainCamera;

    public void SetCameraFollowTarget(Transform target)
    {
        cinemachineMainCamera.Follow = target;
    }
}
