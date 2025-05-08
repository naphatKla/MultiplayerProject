using TMPro;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text lobbyPlayerText;

    private LobbiesList lobbiesList;
    private Lobby lobby;

    public void Initalise(LobbiesList lobbiesList, Lobby lobby, string playerCount = null)
    {
        this.lobbiesList = lobbiesList;
        this.lobby = lobby; 

        lobbyNameText.text = lobby.Name;
        lobbyPlayerText.text = playerCount != null ? $"{playerCount}/{lobby.MaxPlayers}" : $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void Join()
    {
        lobbiesList.JoinAsync(lobby);
    }
}