using Core.InteractObj;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class PlayerInteract : NetworkBehaviour
    {
        #region Properties

        // Interact object need to be in this layer in order to get detected
        public LayerMask interactableLayer;
        [SerializeField] private float interactRange = 5f;

        #endregion

        #region Methods

        private void Update()
        {
            if (!IsOwner) return;
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
                Interact();
        }

        private void Interact()
        {
            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableLayer);

            if (hitObjects.Length <= 0) return;

            Collider2D closestObj = FindClosestObj(hitObjects);
            closestObj.TryGetComponent<InteractObject>(out InteractObject interactObj);

            if (closestObj == null) return;

            // Checking if player can interact with this object
            if (!interactObj.isInteractable.Value) return;

            // Request sever to interact with this selected game object
            Debug.Log("Interact with: " + closestObj.name);
            interactObj.RequestToggleServerRpc();
        }

        // Find the object in interact range and pick the closest one
        private Collider2D FindClosestObj(Collider2D[] interactedObject)
        {
            Collider2D closest = null;
            float minDistance = Mathf.Infinity;

            foreach (Collider2D obj in interactedObject)
            {
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = obj;
                }
            }

            return closest;
        }

        void OnDrawGizmos()
        {
            // Draw player interact range in scene view
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }

        #endregion
    }
}