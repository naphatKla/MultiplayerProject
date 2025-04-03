using System;
using Core.CombatSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

namespace Input
{
    [CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
    public class InputReader : ScriptableObject, IPlayerActions
    {
        public event Action<Vector2> MoveEvent;
        public event Action<bool> PrimaryAttackEvent;
        public event Action<Vector2> MouseMoveEvent;
        private Controls controls;

        [SerializeField] private AttackSystem attackSystem;
        private void OnEnable()
        {
            if (controls == null)
            {
                controls = new Controls();
                controls.Player.SetCallbacks(this);
            }
        
            controls.Player.Enable();
        }
    
        private void OnDisable()
        {
            controls.Player.Disable();
        }
    
        public void OnMove(InputAction.CallbackContext context)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
    
        public void OnPrimaryAttack(InputAction.CallbackContext context)
        {
            if (attackSystem != null && !IsDestroyed(attackSystem))
            {
                if (context.performed)
                {
                    PrimaryAttackEvent?.Invoke(true);
                }
                else if (context.canceled)
                {
                    PrimaryAttackEvent?.Invoke(false);
                }
            }
            else
            {
                // ล้าง attackSystem ถ้ามันถูกลบไปแล้ว
                attackSystem = null;
            }
        }
        
        public void OnAim(InputAction.CallbackContext context)
        {
            MouseMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
        
        private bool IsDestroyed(UnityEngine.Object obj)
        {
            return obj == null;
        }
    }
}