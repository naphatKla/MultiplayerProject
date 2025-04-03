using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class CameraController : NetworkBehaviour
    {
        [SerializeField] private Transform bodyTarget;
     
        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            CameraManager.Instance.SetCameraFollowTarget(bodyTarget);
        }
    }
}
