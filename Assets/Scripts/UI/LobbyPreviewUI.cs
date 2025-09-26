using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

    public class LobbyPreviewUI : MonoBehaviour
    {
        public TMP_Text lobbyNameText;
        private CSteamID lobbyID;
        [SerializeField]
        private SteamLobby steamLobby;


        public void SetLobbyInfo(CSteamID id, string lobbyName, string hostAddress)
        {
            lobbyID = id;
            if (lobbyNameText != null)
                lobbyNameText.text = lobbyName;
        }

        public void OnJoinClicked()
        {
            steamLobby.JoinLobby(lobbyID);
        }
    }
