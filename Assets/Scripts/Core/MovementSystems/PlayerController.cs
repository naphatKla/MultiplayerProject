using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private bool isBeingDestroyed = false; // ป้องกันการเรียกซ้ำหลังจากเริ่มลบ

    public void RemovePlayer()
    {
        if (isBeingDestroyed) return; // ห้ามเรียกซ้ำถ้ากำลังลบอยู่

        if (IsServer)
        {
            isBeingDestroyed = true;
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Despawn(); // ลบออกจาก network ก่อน
                // รอให้ despawn เสร็จสมบูรณ์ก่อน destroy
                Invoke(nameof(DestroySelf), 0.1f); // รอ 0.1 วินาที
            }
        }
        else
        {
            RequestRemovePlayerServerRpc();
        }
    }

    private void DestroySelf()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRemovePlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isBeingDestroyed) return;

        isBeingDestroyed = true;
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn();
            Invoke(nameof(DestroySelf), 0.1f);
        }
    }
    private void Update()
    {
        if (IsOwner && UnityEngine.Input.GetKeyDown(KeyCode.P) && !isBeingDestroyed)
        {
            RemovePlayer();
        }
    }
}