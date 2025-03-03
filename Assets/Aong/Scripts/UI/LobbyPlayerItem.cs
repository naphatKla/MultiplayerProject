using TMPro;
using UnityEngine;

public class LobbyPlayerItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    public void SetPlayerName(string name, bool isLocalPlayer = false)
    {
        if (playerNameText != null)
        {
            playerNameText.text = name;
            playerNameText.color = isLocalPlayer ? Color.green : Color.white;
        }
    }
}
