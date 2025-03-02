using System;
using Unity.Netcode;
using UnityEngine;

public class InteractObject : NetworkBehaviour
{
    #region Propertys

    // can player interact with this obj?
    public NetworkVariable<bool> isInteractable;
    
    // player interact states of this obj
    // true = try to Activate, false = try to Inactivate
    public NetworkVariable<bool> isInteracting = new NetworkVariable<bool>(false);

    #endregion

    #region Methods

    public override void OnNetworkSpawn()
    {
        isInteracting.OnValueChanged += InteractMechanic;
    }

    public override void OnNetworkDespawn()
    {
        isInteracting.OnValueChanged -= InteractMechanic;
    }

    private void InteractMechanic(bool previousValue, bool newValue)
    {
        ActivateObject();
    }

    // Method that Activate if isInteracting value is change
    // Add or change in this method of what you want it to happen
    public void ActivateObject()
    {
        if (isInteracting.Value)
        {
            // if player try to Activate use this
            SetTest(1f);
            Debug.Log("Activate: " + gameObject.name);
        }
        else
        {
            // if player try to Inactivate use this
            SetTest(0.1f);
            Debug.Log("Inactivate: " + gameObject.name);
        }
        
    }

    // Test method for what happened after player interact with this obj
    void SetTest(float setColor)
    {
        TryGetComponent<SpriteRenderer>(out SpriteRenderer baseSprite);
        TryGetComponent<Collider2D>(out Collider2D colli2D);
        colli2D.isTrigger = !colli2D.isTrigger;
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
