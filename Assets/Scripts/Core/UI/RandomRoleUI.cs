using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomRoleUI : MonoBehaviour
{
    public TextMeshProUGUI slotText; // Assign this in the Inspector
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
    }
}
