using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;
using SteamLobbyTutorial;

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
        SteamLobby.Instance.JoinLobby(lobbyID);
    }
}
