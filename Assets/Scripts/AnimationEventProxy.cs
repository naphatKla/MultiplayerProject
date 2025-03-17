using Core.CombatSystems;
using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    [SerializeField] private AttackSystem attackSystem;

    public void ResetAttackTrigger()
    {
        attackSystem?.EndAttack();
    }
}
