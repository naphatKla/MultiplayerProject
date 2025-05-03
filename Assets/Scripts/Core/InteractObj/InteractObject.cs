using Unity.Netcode;
using UnityEngine;

namespace Core.InteractObj
{
    public enum InteractObjectType
    {
        Door,
        Lever,
        Trap
    }

    public class InteractObject : NetworkBehaviour
    {
        #region Properties

        // can player interact with this obj?
        public NetworkVariable<bool> isInteractable;

        // player interact states of this obj
        // true = try to Activate, false = try to Inactivate
        public NetworkVariable<bool> isInteracting = new NetworkVariable<bool>(false);

        public NetworkVariable<InteractObjectType> interactType;

        public EnemyRoom enemyRoom; // Reference to the EnemyRoom script

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
        private void ActivateObject()
        {
            if(interactType.Value == InteractObjectType.Door && !enemyRoom.roomActive.Value)
            {
                if (!isInteracting.Value)
                {
                    // if player try to Activate use this
                    SetTest(1f, true);
                    Debug.Log("Activate: " + gameObject.name);
                }
                else
                {
                    // if player try to Inactivate use this
                    SetTest(0.1f, false);
                    Debug.Log("Inactivate: " + gameObject.name);
                }
            }
            else if(interactType.Value == InteractObjectType.Lever)
            {
                if (gameObject.GetComponent<LeverInteract>() == null) return;

                Debug.Log("Activate: " + gameObject.name);
                gameObject.GetComponent<LeverInteract>().ActivateLever();
                SetTest(1f, true);
            }
            else if(interactType.Value == InteractObjectType.Trap)
            {
                if (gameObject.GetComponent<Trap>() == null) return;

                Debug.Log("Activate: " + gameObject.name);
                gameObject.GetComponent<Trap>().DeactivateTrap();
                SetTest(1f, true);
            }
        }

        // Test method for what happened after player interact with this obj
        public void SetTest(float setColor, bool isEnabled)
        {
            TryGetComponent(out SpriteRenderer baseSprite);
            TryGetComponent(out Collider2D colli2D);
            //colli2D.isTrigger = !colli2D.isTrigger;
            colli2D.isTrigger = !isEnabled;
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

        #endregion
    }
}