using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerInteract : NetworkBehaviour
{
    public LayerMask interactableLayer;
    [SerializeField] private float interactRange = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {Interact();}
    }

    private void Interact()
    {
        Collider2D hitObject = Physics2D.OverlapCircle(transform.position, interactRange, interactableLayer);
        hitObject.TryGetComponent<InteractObject>(out InteractObject interactObj);
        
        if (hitObject == null) 
        {return;}
        else if (hitObject.CompareTag("Interactable") && interactObj.isInteractable == true)
        {
            Debug.Log("Interact with: " + hitObject.name);
        }
        
    }

    void OnDrawGizmos()
    {
        // แสดงรัศมีของการตรวจจับใน Scene View
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
