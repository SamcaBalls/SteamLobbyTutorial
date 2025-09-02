using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class LobbyPreviewUI : MonoBehaviour
{
    public TMP_Text lobbyNameText;
    private CSteamID lobbyID;

    public void SetLobbyInfo(CSteamID id, string lobbyName, string hostAddress)
    {
        lobbyID = id;
        if (lobbyNameText != null)
            lobbyNameText.text = lobbyName;
    }

    public void OnJoinClicked()
    {
        Debug.Log("Joining lobby: " + lobbyID);
        SteamMatchmaking.JoinLobby(lobbyID);
    }
}
