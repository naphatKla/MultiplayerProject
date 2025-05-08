using Unity.Netcode;
using UnityEngine;

public class LeverInteract : NetworkBehaviour
{
    public EnemyRoom enemyRoom;
    public Sprite leverUpSprite;
    public Sprite leverDownSprite; 
    private SpriteRenderer spriteRenderer;
    private bool isPulled = false;
    private bool isUsed = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ActivateLever()
    {
        if (IsOwner && enemyRoom != null && !isUsed)
        {
            SoundEffectManager.Instance.PlayGlobal3DAtPosition("Interact", transform.position, 2f,1f,7f);
            enemyRoom.Interact();
            LeverPulledServerRpc();
            isUsed = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void LeverPulledServerRpc()
    {
        LeverPulledClientRpc();
    }

    [ClientRpc]
    void LeverPulledClientRpc()
    {
        isPulled = true;
        spriteRenderer.sprite = leverDownSprite;
    }
}