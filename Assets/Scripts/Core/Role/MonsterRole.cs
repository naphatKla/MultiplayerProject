using UnityEngine;

public class MonsterRole : PlayerRole
{
    private bool isActive = true;

    public bool tranformMimic = false;

    public void TranformMimic()
    {
        tranformMimic = true;
        Debug.Log("Tranform to mimic");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;
        if (currentRole != Role.Monster)
        {
            return;
        }
    }
    
    protected override void UpdateRole(Role role)
    {
        if (role != Role.Monster)
        {
            enabled = false;
            isActive = false;
        }
    }
}
