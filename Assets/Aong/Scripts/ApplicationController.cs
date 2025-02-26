using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
        }
        else
        {
            var hostSingleton = Instantiate(hostPrefab);
            await hostSingleton.CreateHost();
            var clientSingleton = Instantiate(clientPrefab);
            var authenticated = await clientSingleton.CreateClient();

            if (authenticated)
                clientSingleton.GameManager.GoToMenu();
            else
                Debug.LogError("Failed to authenticate client!");
        }
    }
}