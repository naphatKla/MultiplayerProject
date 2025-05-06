using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Core.MovementSystems;

public class MonsterRole : NetworkBehaviour
{
    [SerializeField] private RuntimeAnimatorController mimicAnimatorController;
    [SerializeField] private RuntimeAnimatorController playerAnimatorController;
    [SerializeField] private GameObject bloodCountUIPrefab;
    [SerializeField] private GameObject transformReadyUIPrefab;
    [SerializeField] private Slider transformationSlider;

    private GameObject bloodUIInstance;
    private GameObject transformReadyUIInstance;
    private TMP_Text bloodCountText;
    private TMP_Text transformReadyText;
    private PlayerMovement playerMovement;

    private PlayerNameDisplay displayName;
    private Animator animator;
    public bool isTransforming = false;
    private float transformationDuration = 15f;
    private NetworkVariable<int> collectedItems = new NetworkVariable<int>(0);
    private const int requiredItems = 3;
    private NetworkVariable<bool> canTransformPermanently = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> transformMimic = new NetworkVariable<bool>(false);

    public RoleManager RoleManager { get; set; }

    public bool IsActive => enabled;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        displayName = GetComponent<PlayerNameDisplay>();
        transformationSlider.gameObject.SetActive(false);
        collectedItems.OnValueChanged += (oldValue, newValue) =>
        {
            UpdateBloodText();
            if (IsOwner && newValue >= requiredItems)
            {
                canTransformPermanently.Value = true;
                if (transformReadyText != null && !transformMimic.Value)
                {
                    transformReadyText.gameObject.SetActive(true);
                    transformReadyText.text = "Transform Ready! Press T";
                }
            }
            else if (IsOwner && newValue < requiredItems && transformReadyText != null)
            {
                transformReadyText.gameObject.SetActive(false);
            }
        };
    }

    private void Start()
    {
        if (RoleManager.Instance != null && RoleManager.Instance.PlayerRoles != null)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            if (RoleManager.Instance.PlayerRoles.TryGetValue(clientId, out Role assignedRole))
            {
                if (assignedRole != Role.Monster)
                {
                    enabled = false;
                    DeactivateUI();
                    return;
                }
                else
                {
                    Debug.Log($"Client {clientId} is Monster. Initializing UI for IsOwner: {IsOwner}.");
                    if (IsOwner)
                    {
                        InitializeUI();
                    }
                }
            }
            else
            {
                enabled = false;
                DeactivateUI();
                return;
            }
        }
        else
        {
            enabled = false;
            DeactivateUI();
            return;
        }
    }

    private void InitializeUI()
    {
        if (!IsOwner) return;
        
        // Initialize Blood Count UI
        if (bloodCountUIPrefab != null)
        {
            bloodUIInstance = Instantiate(bloodCountUIPrefab);
            bloodUIInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
            bloodCountText = bloodUIInstance.GetComponentInChildren<TMP_Text>();
            if (bloodCountText == null)
            {
                Debug.LogError("bloodCountText not found in bloodCountUIPrefab. Ensure it has a TMP_Text component.");
            }
            else
            {
                bloodCountText.gameObject.SetActive(true);
                UpdateBloodText();
            }
        }
        else
        {
            Debug.LogWarning("bloodCountUIPrefab not assigned in MonsterRole.");
        }

        // Initialize Transform Ready UI
        if (transformReadyUIPrefab != null)
        {
            transformReadyUIInstance = Instantiate(transformReadyUIPrefab);
            transformReadyUIInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
            transformReadyText = transformReadyUIInstance.GetComponentInChildren<TMP_Text>();
            if (transformReadyText == null)
            {
                Debug.LogError("transformReadyText not found in transformReadyUIPrefab. Ensure it has a TMP_Text component.");
            }
            else
            {
                transformReadyText.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("transformReadyUIPrefab not assigned in MonsterRole.");
        }

        // Initialize Slider
        if (transformationSlider != null)
        {
            transformationSlider.gameObject.SetActive(false);
            transformationSlider.maxValue = transformationDuration;
            transformationSlider.minValue = 0f;
        }
        else
        {
            Debug.LogWarning("transformationSlider not assigned in MonsterRole.");
        }
    }

    private void DeactivateUI()
    {
        if (IsOwner)
        {
            if (bloodUIInstance != null)
            {
                bloodUIInstance.SetActive(false);
            }
            if (transformReadyUIInstance != null)
            {
                transformReadyUIInstance.SetActive(false);
            }
            if (transformationSlider != null)
            {
                transformationSlider.gameObject.SetActive(false);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (bloodUIInstance != null)
        {
            Destroy(bloodUIInstance);
        }
        if (transformReadyUIInstance != null)
        {
            Destroy(transformReadyUIInstance);
        }
    }

    private void Update()
    {
        if (!IsOwner || !IsActive || isTransforming) return;

        if (UnityEngine.Input.GetKeyDown(KeyCode.T) && canTransformPermanently.Value)
        {
            StartCoroutine(TransformSequence());
        }
    }

    private void UpdateBloodText()
    {
        if (bloodCountText != null && IsOwner)
        {
            bloodCountText.text = $"Blood: {collectedItems.Value}/{requiredItems}";
            if (bloodUIInstance != null)
            {
                bloodUIInstance.SetActive(true);
            }
        }
    }

    private IEnumerator TransformSequence()
    {
        isTransforming = true;
        TransformMimic();

        float elapsedTime = 0f;
        if (transformationSlider != null && IsOwner && IsActive)
        {
            transformationSlider.gameObject.SetActive(true);
            transformationSlider.value = transformationDuration;
        }

        while (elapsedTime < transformationDuration)
        {
            elapsedTime += Time.deltaTime;
            if (transformationSlider != null && IsOwner && IsActive)
            {
                transformationSlider.value = transformationDuration - elapsedTime;
            }
            yield return null;
        }

        RevertToPlayer();
        isTransforming = false;

        if (transformationSlider != null && IsOwner && IsActive)
        {
            transformationSlider.gameObject.SetActive(false);
        }
    }

    public void TransformMimic()
    {
        if (!IsOwner) return;

        transformMimic.Value = true;
        var healthSystem = GetComponent<Core.HealthSystems.HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.SetMaxHealth(1000f, true);
        }

        if (playerMovement != null)
        {
            playerMovement.TransformToMonster(true);
        }


        TransformMimicServerRpc();
        if (transformReadyText != null)
        {
            transformReadyText.gameObject.SetActive(false);
        }
    }

    public void RevertToPlayer()
    {
        if (!IsOwner) return;

        transformMimic.Value = false;
        var healthSystem = GetComponent<Core.HealthSystems.HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.SetMaxHealth(100f, true);
        }

        if (playerMovement != null)
        {
            playerMovement.TransformToMonster(false);
        }

        RevertToPlayerServerRpc();
        ResetCollectedItemsServerRpc();
    }

    [ServerRpc]
    public void CollectItemServerRpc()
    {
        collectedItems.Value++;
        if (collectedItems.Value >= requiredItems)
        {
            canTransformPermanently.Value = true;
        }
    }

    [ServerRpc]
    private void ResetCollectedItemsServerRpc()
    {
        collectedItems.Value = 0;
        canTransformPermanently.Value = false;
        UpdateCollectionProgressClientRpc(collectedItems.Value);
    }

    [ClientRpc]
    private void UpdateCollectionProgressClientRpc(int currentItems)
    {
        if (IsOwner)
        {
            UpdateBloodText();
            if (transformReadyText != null)
            {
                transformReadyText.gameObject.SetActive(currentItems >= requiredItems && !transformMimic.Value);
                if (currentItems >= requiredItems)
                {
                    transformReadyText.text = "Transform Ready! Press T";
                }
            }
        }
    }

    [ServerRpc]
    private void TransformMimicServerRpc()
    {
        Debug.Log($"TransformMimicServerRpc called for {gameObject.name}");
        TransformMimicClientRpc();
    }

    [ClientRpc]
    private void TransformMimicClientRpc()
    {
        if (animator != null && mimicAnimatorController != null)
        {
            animator.runtimeAnimatorController = mimicAnimatorController;
        }
        if (displayName != null)
        {
            displayName.playerNameText.gameObject.SetActive(false);
        }
    }

    [ServerRpc]
    private void RevertToPlayerServerRpc()
    {
        RevertToPlayerClientRpc();
    }

    [ClientRpc]
    private void RevertToPlayerClientRpc()
    {
        if (animator != null && playerAnimatorController != null)
        {
            animator.runtimeAnimatorController = playerAnimatorController;
        }
        if (displayName != null)
        {
            displayName.playerNameText.gameObject.SetActive(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (transformMimic.Value)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}