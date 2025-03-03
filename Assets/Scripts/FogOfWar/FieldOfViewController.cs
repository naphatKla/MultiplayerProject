using System;
using Input;
using UnityEngine;

public class FieldOfViewController : MonoBehaviour
{
         [SerializeField] private InputReader inputReader;
         [SerializeField] private float curPlayerMoveSpeed;
         [Space] [Header("Dependencies")] [SerializeField] private Rigidbody2D rb;
         [SerializeField] private SpriteRenderer spriteRenderer;
         [SerializeField] private FieldOfView fieldOfView;
         private Vector2 movementInput;

         private void Start()
         {
             inputReader.MoveEvent += SetMoveInput;
             inputReader.MouseMoveEvent += SetAimDirection;
         }
         
         private void FixedUpdate()
         {
             MovementHandler();
             fieldOfView.SetOrigin(transform.position);
         }
         
         private void SetMoveInput(Vector2 movement)
         {
             movementInput = movement;
         }

         private void SetAimDirection(Vector2 mousePos)
         {
             Vector3 aimDir = (GetMouseInWorldPosition(mousePos) - transform.position).normalized;
             fieldOfView.SetAimDirection(aimDir);
         }
         
         public Vector3 GetMouseInWorldPosition(Vector2 pos)
         {
             Vector3 worldPosition = Camera.main.ScreenToWorldPoint(pos);
             worldPosition.z = 0f;
             return worldPosition;
         }
         
         private void MovementHandler()
         {
             Vector2 newPos = rb.position + movementInput * (curPlayerMoveSpeed * Time.fixedDeltaTime);
             rb.MovePosition(newPos);
         }
}
