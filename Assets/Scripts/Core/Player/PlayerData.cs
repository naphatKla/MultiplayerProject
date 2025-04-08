using UnityEngine;
using TMPro;
using Unity.Collections;
using Unity.Netcode;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData =
                HostSingleton.Instance.GameManager.networkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.userName;
        }
    }
}