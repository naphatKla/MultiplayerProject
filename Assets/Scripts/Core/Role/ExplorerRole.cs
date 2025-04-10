using Unity.Netcode;
using UnityEngine;


public class ExplorerRole : PlayerRole
{
    [SerializeField] private ExplorerClass explorerClass;
    private bool isActive = true;
    public ExplorerClass ExplorerClass { 
        get => explorerClass;
        private set => explorerClass = value; 
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) { return;}
        if (currentRole != Role.Explorer)
        {
            return;
        }
    }

    protected override void UpdateRole(Role role)
    {
        if (role != Role.Explorer)
        {
            enabled = false;
            isActive = false;
        }
        else
        {
            if (IsOwner)
            {
                UpdateClassToClientRpc(roleManager.AssignClassForExplorer(OwnerClientId));
            }
        }
    }
    
    
    [Rpc(SendTo.ClientsAndHost)]
    void UpdateClassToClientRpc(ExplorerClass playerclass)
    {
        if (explorerClass == ExplorerClass.NullClass)
        {
            explorerClass = playerclass;
        }
    }
}
