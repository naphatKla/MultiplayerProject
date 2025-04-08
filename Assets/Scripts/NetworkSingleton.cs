using Unity.Netcode;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    public static T Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate {typeof(T).Name} detected. Destroying the extra.");
            Destroy(this.gameObject);
            return;
        }

        Instance = this as T;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Instance == this)
            Instance = null;
    }
}