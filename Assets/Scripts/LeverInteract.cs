using Unity.Netcode;
using UnityEngine;

public class LeverInteract : NetworkBehaviour
{
    public EnemyRoom enemyRoom;
    public Sprite leverUpSprite;
    public Sprite leverDownSprite; 
    private SpriteRenderer spriteRenderer;
    private bool isPulled = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ActivateLever()
    {
        if (IsOwner && enemyRoom != null)
        {
            enemyRoom.Interact();
            LeverPulledServerRpc(!isPulled);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void LeverPulledServerRpc(bool newState)
    {
        LeverPulledClientRpc(newState);
    }

    [ClientRpc]
    void LeverPulledClientRpc(bool newState)
    {
        isPulled = newState;
        spriteRenderer.sprite = isPulled ? leverDownSprite : leverUpSprite;
    }
}