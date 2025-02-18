using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;

public class Gap_PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Vector2 mousePosition;
    [SerializeField] private Vector2 movement;
    [SerializeField] private float curPlayerMoveSpeed;
    
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sprRndr;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

    }

    private void Update()
    {
        if (!IsOwner) { return; }

        PlayerFacing();

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        rb.MovePosition(rb.position + movement * curPlayerMoveSpeed * Time.deltaTime);
    }

    private void PlayerFacing()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDirection = mousePosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

        // Determine whether the player should be flipped
        bool shouldFaceLeft = angle > 90 || angle < -90;

        // Send facing direction to all clients
        UpdateFacingServerRpc(shouldFaceLeft);
    }

    // ServerRpc: Called by the owner to update the server
    [ServerRpc]
    private void UpdateFacingServerRpc(bool shouldFaceLeft)
    {
        // Broadcast to all clients, including the host
        UpdateFacingClientRpc(shouldFaceLeft);
    }

    // ClientRpc: Updates the sprite on all clients, including the host
    [ClientRpc]
    private void UpdateFacingClientRpc(bool shouldFaceLeft)
    {
        sprRndr.flipX = shouldFaceLeft;
    }
}
