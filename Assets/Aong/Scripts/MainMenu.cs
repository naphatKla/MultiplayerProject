using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private TMP_InputField nameInputField;

    public async void StartHost()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            Debug.LogWarning("Please enter a name!");
            return;
        }

        byte[] connectionData = System.Text.Encoding.UTF8.GetBytes(nameInputField.text);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = connectionData;

        await HostSingleton.Instance.CreateHost();
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

        byte[] connectionData = System.Text.Encoding.UTF8.GetBytes(nameInputField.text);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = connectionData;

        await ClientSingleton.Instance.CreateClient();
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}