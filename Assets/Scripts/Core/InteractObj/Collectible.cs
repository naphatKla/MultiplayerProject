using Unity.Netcode;
using UnityEngine;

public class Collectible : NetworkBehaviour
{
    [SerializeField] private string collectibleName = "Blood";
    [SerializeField] private float collectionRange = 2f;
    [SerializeField] private float reactivationDelay = 20f;
    [SerializeField] private KeyCode collectKey = KeyCode.E;

    private NetworkVariable<bool> canInteract = new NetworkVariable<bool>(true);

    private void Awake()
    {
        canInteract.OnValueChanged += (oldValue, newValue) =>
        {
            Debug.Log($"Collectible {collectibleName} canInteract changed from {oldValue} to {newValue}");
        };
    }

    private void Update()
    {
        if (!IsClient || !canInteract.Value) return;
        CheckCollection();
    }

    private void CheckCollection()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
            return;

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject == null)
            return;

        GameObject player = playerObject.gameObject;
        MonsterRole monsterRole = player.GetComponent<MonsterRole>();
        if (monsterRole == null || !monsterRole.IsActive || monsterRole.transformMimic.Value)
            return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance <= collectionRange && UnityEngine.Input.GetKeyDown(collectKey))
        {
            if (!canInteract.Value) return;
            CollectServerRpc(monsterRole.OwnerClientId);
        }
    }
    
    [Rpc(SendTo.Server)]
    private void CollectServerRpc(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) { return; }

        GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        MonsterRole monsterRole = player.GetComponent<MonsterRole>();
        if (monsterRole == null || !monsterRole.IsActive || monsterRole.transformMimic.Value)
        {
            return;
        }

        PerformCollection(clientId);
    }
    
    private void PerformCollection(ulong clientId)
    {
        canInteract.Value = false;
        MonsterRole monsterRole = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<MonsterRole>();
        monsterRole.CollectItemServerRpc();

        DeactivateCollectibleClientRpc();

        if (IsServer)
        {
            StartCoroutine(ReactivateAfterDelay());
        }
    }

    [ClientRpc]
    private void DeactivateCollectibleClientRpc()
    {
        SetCollectibleVisible(false);
        SoundEffectManager.Instance.PlayGlobal3DAtPosition("Collect", transform.position, 2f, 1f, 10f);
    }

    [ClientRpc]
    private void ReactivateCollectibleClientRpc()
    {
        SetCollectibleVisible(true);
    }

    private System.Collections.IEnumerator ReactivateAfterDelay()
    {
        yield return new WaitForSeconds(reactivationDelay);
        canInteract.Value = true;
        ReactivateCollectibleClientRpc();
    }

    private void SetCollectibleVisible(bool visible)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = visible;
        }

        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = visible;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }
}