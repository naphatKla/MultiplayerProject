using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class LeverInteract : NetworkBehaviour
{
    public EnemyRoom enemyRoom; // Reference to the EnemyRoom script

    public void ActivateLever()
    {
        if (IsOwner && enemyRoom != null)
        {
            enemyRoom.Interact();
            LeverPulledServerRpc(); // Sync animation for all clients
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void LeverPulledServerRpc()
    {
        //LeverPulledClientRpc();
    }

    //[ClientRpc]
    /*void LeverPulledClientRpc()
    {
        // Play lever animation
        GetComponent<Animator>()?.SetTrigger("Pull");
    }*/
}
