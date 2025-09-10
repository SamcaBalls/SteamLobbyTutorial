using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;

namespace SteamLobbyTutorial
{
    public class SteamLobby : NetworkBehaviour
    {
        public static SteamLobby Instance;
        public GameObject hostButton = null;
        public ulong lobbyID;
        public NetworkManager networkManager;
        public PanelSwapper panelSwapper;
        [SerializeField] TMP_Dropdown dropdown;
        [SerializeField] TMP_InputField inputFieldHost;
        [SerializeField] TMP_InputField inputFieldClient;
        bool privateLobby = false;

        private Callback<LobbyCreated_t> lobbyCreated;
        private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        private Callback<LobbyEnter_t> lobbyEntered;
        private Callback<LobbyChatUpdate_t> lobbyChatUpdate;

        private const string HostAddressKey = "HostAddress";
        private bool callbacksRegistered = false;
        private bool waitingForSteam = false;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            networkManager = GetComponentInParent<NetworkManager>();
            panelSwapper.gameObject.SetActive(true);
            RegisterCallbacks();
            SteamAPI.RunCallbacks();
        }


        void RegisterCallbacks()
        {
            if (callbacksRegistered)
                return;

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

            callbacksRegistered = true;
            Debug.Log("Steam callbacky registrované");
        }

        public void HostLobby()
        {
            if (!SteamManager.Initialized)
            {
                if (!waitingForSteam)
                {
                    StartCoroutine(WaitForSteamAndHost());
                    waitingForSteam = true;
                }
                return;
            }

            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);

            Debug.Log(privateLobby
                ? "Požadavek na vytvoøení PUBLIC lobby s heslem odeslán"
                : "Požadavek na vytvoøení PUBLIC lobby bez hesla odeslán");
        }

        private IEnumerator WaitForSteamAndHost()
        {
            while (!SteamManager.Initialized)
                yield return null;

            Debug.Log("Steam inicializován, registruji callbacky a vytváøím lobby");
            RegisterCallbacks();

            HostLobby();
        }

        void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Nepodaøilo se vytvoøit lobby: " + callback.m_eResult);
                return;
            }

            lobbyID = callback.m_ulSteamIDLobby;
            var lobby = new CSteamID(lobbyID);

            Debug.Log("Lobby vytvoøeno. ID: " + lobbyID);

            SteamMatchmaking.SetLobbyData(lobby, HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(lobby, "name", SteamFriends.GetPersonaName() + "'s Lobby");
            SteamMatchmaking.SetLobbyData(lobby, "game_id", "xXBallerXx");

            // uložíme jestli má lobby heslo
            SteamMatchmaking.SetLobbyData(lobby, "private", privateLobby ? "true" : "false");
            SteamMatchmaking.SetLobbyData(lobby, "password", privateLobby ? inputFieldHost.text : "");

            networkManager.StartHost();

            // reset flagu pro jistotu
            privateLobby = false;
        }


        void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("Join request received for lobby: " + callback.m_steamIDLobby);

            if (NetworkServer.active)
                NetworkManager.singleton.StopHost();
            if (NetworkClient.isConnected || NetworkClient.active)
            {
                NetworkManager.singleton.StopClient();
                NetworkClient.Shutdown();
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (NetworkServer.active)
            {
                Debug.Log("Jsem host, ignoruji join request");
                return;
            }

            lobbyID = callback.m_ulSteamIDLobby;
            var lobby = new CSteamID(lobbyID);

            string hostAddress = SteamMatchmaking.GetLobbyData(lobby, HostAddressKey);
            if (string.IsNullOrEmpty(hostAddress))
            {
                Debug.LogError("Nebyla nalezena HostAddress!");
                return;
            }

            networkManager.networkAddress = hostAddress;
            Debug.Log("Vstoupil jsem do lobby: " + lobbyID);

            networkManager.StartClient();
            panelSwapper.SwapPanel("LobbyPanel");
        }

        void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            if (callback.m_ulSteamIDLobby != lobbyID) return;

            EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
            Debug.Log($"LobbyChatUpdate: {stateChange}");

            bool shouldUpdate = stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeKicked) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeBanned);

            if (shouldUpdate)
            {
                StartCoroutine(DelayedNameUpdate(0.5f));
                LobbyUIManager.Instance?.CheckAllPlayersReady();
            }
        }

        private IEnumerator DelayedNameUpdate(float delay)
        {
            if (LobbyUIManager.Instance == null)
            {
                Debug.LogWarning("Lobby UI Manager.Instance je null, pøeskoèeno");
                yield break;
            }
            yield return new WaitForSeconds(delay);
            LobbyUIManager.Instance?.UpdatePlayerLobbyUI();
        }

        public void LeaveLobby()
        {
            CSteamID currentOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyID));
            CSteamID me = SteamUser.GetSteamID();
            var lobby = new CSteamID(lobbyID);

            if (lobbyID != 0)
            {
                SteamMatchmaking.LeaveLobby(lobby);
                lobbyID = 0;
            }

            if (NetworkServer.active && currentOwner == me)
                NetworkManager.singleton.StopHost();
            else if (NetworkClient.isConnected)
                NetworkManager.singleton.StopClient();

            panelSwapper.gameObject.SetActive(true);
            this.gameObject.SetActive(true);
            panelSwapper.SwapPanel("MainPanel");
        }


        public void OnDropdownChange()
        {
            privateLobby = dropdown.value == 1; 
            
            inputFieldHost.interactable = privateLobby;
        }
    }
}
