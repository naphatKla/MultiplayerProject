using UnityEngine;

public class MonsterRole : PlayerRole
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentRole != Role.Monster)
        {
            return;
        }
    }
    
    protected override void UpdateRole(Role role)
    {
        if (role != Role.Monster)
        {
            Destroy(this);
        }
    }
}
