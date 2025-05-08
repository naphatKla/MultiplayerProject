using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;
    public async void StartHost()
    {
        SoundEffectManager.Instance.PlayLocal("ButtonClick", 0.5f);
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        SoundEffectManager.Instance.PlayLocal("ButtonClick", 0.5f);
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}
