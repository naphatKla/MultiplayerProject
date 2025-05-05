using System;
using Unity.Netcode;
using UnityEngine;

public class GameHUD : Singleton<GameHUD>
{
    public GameObject deathUI;
    public GameObject winUI;
    public GameObject mimicWinUI;
    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
        {
            LeaveGame();
        }
    }
    
    public void ShowMimicWinUI()
    {
        mimicWinUI.SetActive(true);
    }
    
    public void ShowExplorerWinUI()
    {
        winUI.SetActive(true);
    }

    public void LeaveGame()
    {
        Debug.Log("Leave");
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }

        ClientSingleton.Instance.GameManager.Disconnect();
    }
}