using Edgegap;
using SteamLobbyTutorial;
using Steamworks;
using UnityEngine;

public class PasswordUI : MonoBehaviour
{
    private CSteamID lobbyID;

    public void SetLobbyInfo(CSteamID id)
    {
        lobbyID = id;
    }

    public void OnJoinLobbyClick()
    {
        SteamLobby.Instance.JoinLobby(lobbyID);
    }
}
