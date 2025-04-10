using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomRoleUI : MonoBehaviour
{
    public TextMeshProUGUI slotText; // Assign this in the Inspector
    public GameObject title;
    public GameObject slot; // Assign this in the Inspector
    public GameObject indicateText;
    public float shuffleDuration = 2f; // How long the shuffle lasts
    public float shuffleSpeed = 0.1f; // How fast the text changes
    [SerializeField] private string[] possibleResults; // Possible outcomes

    public static RandomRoleUI instance;
    private string finalRole; // Store final role
    private bool isShuffling = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        indicateText.SetActive(false);
        slotText = slot.GetComponent<TextMeshProUGUI>();
    }

    public void StartShuffle(string role)
    {
        finalRole = role;
        isShuffling = true;
        InvokeRepeating(nameof(ShuffleText), 0f, shuffleSpeed);
        Invoke(nameof(StopShuffle), shuffleDuration);
    }

    private void ShuffleText()
    {
        if (!isShuffling) return;
        slotText.text = possibleResults[Random.Range(0, possibleResults.Length)];
    }

    private void StopShuffle()
    {
        isShuffling = false;
        CancelInvoke(nameof(ShuffleText));
        slotText.text = finalRole;
        StartCoroutine(DisableUI());
    }

    private IEnumerator DisableUI()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(WaitToClose());
    }

    IEnumerator WaitToClose()
    {
        yield return new WaitForSeconds(2f);
        title.SetActive(false);
        slot.SetActive(false);
        IndicateRole();
    }

    private void IndicateRole()
    {
        indicateText.SetActive(true);
        indicateText.GetComponent<TextMeshProUGUI>().text = slotText.text;
    }
}
