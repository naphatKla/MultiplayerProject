using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private TMP_InputField nameInputField;

    public async void StartHost()
    {
        if (string.IsNullOrEmpty(nameInputField.text)) { return; }
        await HostSingleton.Instance.CreateHost();
        HostSingleton.Instance.GameManager.SetPlayerName(NetworkManager.Singleton.LocalClientId, nameInputField.text);
    }

    public async void StartClient()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            Debug.LogWarning("Please enter a name!");
            return;
        }

        if (string.IsNullOrEmpty(joinCodeField.text))
        {
            Debug.LogWarning("Please enter a lobby code!");
            return;
        }
        
        Debug.Log($"StartClient - Player name entered: {nameInputField.text}");
        
        await ClientSingleton.Instance.CreateClient();
        ClientSingleton.Instance.GameManager.SetPlayerName(NetworkManager.Singleton.LocalClientId, nameInputField.text);
        
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}