using System;
using Unity.Netcode;
using UnityEngine;

public class InteractObject : NetworkBehaviour
{
    #region Propertys

    public NetworkVariable<bool> isInteractable;
    public NetworkVariable<bool> isInteracting = new NetworkVariable<bool>(false);
    private bool _isDoorOpen;

    #endregion

    #region Methods

    public override void OnNetworkSpawn()
    {
        isInteracting.OnValueChanged += DoorActive;
    }

    public override void OnNetworkDespawn()
    {
        isInteracting.OnValueChanged -= DoorActive;
    }

    private void DoorActive(bool previousValue, bool newValue)
    {
        
        if (newValue)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    public void OpenDoor()
    {
        SetTest(0.1f,true);
        _isDoorOpen = true;
        Debug.Log("open");
    }
    public void CloseDoor()
    {
        SetTest(1f,false);
        _isDoorOpen = false;
        Debug.Log("close");
    }

    void SetTest(float setColor, bool setCollider)
    {
        TryGetComponent<SpriteRenderer>(out SpriteRenderer baseSprite);
        TryGetComponent<Collider2D>(out Collider2D colli2D);
        colli2D.isTrigger = setCollider;
        Color color = baseSprite.color;
        color.a = setColor;
        baseSprite.color = color;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestToggleServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsClient) return;
        if (isInteractable.Value)
        {
            isInteracting.Value = !isInteracting.Value;
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestChangeInteractableRpc()
    {
        isInteractable.Value = !isInteractable.Value;
    }

    #endregion
    

}
