using Unity.Netcode;
using UnityEngine;

public class Trap : NetworkBehaviour
{
    
    [SerializeField] public Sprite trap01; 
    [SerializeField] public Sprite trap02;
    
    [SerializeField] private float trapDamage;
    [SerializeField] private bool _isActive = false;
    
    
    private void AttackHandler(bool isAttackingInput)
    {
        if (this == null || !gameObject.activeSelf) return;
        if (!isAttackingInput) return;
    }
    
}
