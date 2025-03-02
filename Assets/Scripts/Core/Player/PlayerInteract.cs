using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerInteract : NetworkBehaviour
{
    #region Propertys
    
    public LayerMask interactableLayer;
    [SerializeField] private float interactRange = 5f;
    
    #endregion

    #region Methods

    private void Update()
    {
        if (!IsOwner)
        {return;}
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {Interact();}
        }
    }
    
    private void Interact()
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableLayer);

        if (hitObjects.Length > 0)
        {
            Collider2D closestObj = FindClosestObj(hitObjects);
            closestObj.TryGetComponent<InteractObject>(out InteractObject interactObj);
            if (closestObj == null) 
            {return;}
            else if (interactObj.isInteractable.Value)
            {
                Debug.Log("Interact with: " + closestObj.name);
                interactObj.RequestToggleServerRpc();
            }
        }

    }
    
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
        // แสดงรัศมีของการตรวจจับใน Scene View
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    #endregion

}
