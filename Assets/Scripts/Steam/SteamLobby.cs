using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;

    public class SteamLobby : NetworkBehaviour
    {
        private static SteamLobby instance;
        public static SteamLobby Instance {
            get { if (instance == null)
                    instance = FindAnyObjectByType<SteamLobby>(FindObjectsInactive.Include); 
                return instance;
            } 
        }
        public GameObject hostButton = null;
        public ulong lobbyID;
        public NetworkManager networkManager;
        public PanelSwapper panelSwapper;
        [SerializeField] TMP_Dropdown dropdown;
        [SerializeField] TMP_InputField inputFieldHost;
        [SerializeField] TMP_InputField inputFieldClient;
        [SerializeField] PasswordUI passwordUI;
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
            if (instance == null)
                instance = this;
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

            RegisterCallbacks();

            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);

            Debug.Log(privateLobby
                ? "Požadavek na vytvoření PUBLIC lobby s heslem odeslán"
                : "Požadavek na vytvoření PUBLIC lobby bez hesla odeslán");
        }

        public IEnumerator WaitForSteamAndHost()
        {
            while (!SteamManager.Initialized)
                yield return null;

            Debug.Log("Steam inicializován, registruji callbacky a vytvářím lobby");
            RegisterCallbacks();

            HostLobby();
        }

        void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Nepodařilo se vytvořit lobby: " + callback.m_eResult);
                return;
            }

            lobbyID = callback.m_ulSteamIDLobby;
            var lobby = new CSteamID(lobbyID);

            Debug.Log("Lobby vytvořeno. ID: " + lobbyID);

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
                Debug.LogWarning("Lobby UI Manager.Instance je null, přeskočeno");
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

        public void JoinLobby(CSteamID targetLobbyID)
        {
            string password = SteamMatchmaking.GetLobbyData(targetLobbyID, "password");

            if (!string.IsNullOrEmpty(password))
            {
                if(inputFieldClient.text != password)
                {
                    Debug.Log("Lobby má heslo → otevírám PasswordPanel");
                    passwordUI.SetLobbyInfo(targetLobbyID);
                    panelSwapper.SwapPanel("PasswordEnterPanel");
                }
                else
                {
                    RegisterCallbacks();
                    SteamMatchmaking.JoinLobby(targetLobbyID);
                }

            }
            else
            {
                Debug.Log("Lobby nemá heslo → rovnou join");
                SteamMatchmaking.JoinLobby(targetLobbyID);
            }
        }

    }
