using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FogToggle : NetworkBehaviour
{
    [SerializeField] 
    private List<GameObject> fogGameObj;

    [SerializeField]
    private List<GameObject> layerObj;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            foreach (var fog in fogGameObj)
            {
                fog.SetActive(true);
            }
            foreach (var layer in layerObj)
            {
                layer.layer = 3;
            }
            
        }
        else
        {
            foreach (var fog in fogGameObj)
            {
                fog.SetActive(false);
            }
            foreach (var layer in layerObj)
            {
                layer.layer = 0;
            }
        }
    }
}
