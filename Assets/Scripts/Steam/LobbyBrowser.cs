using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using TMPro;

public class LobbyBrowser : MonoBehaviour
{
    public static LobbyBrowser Instance;

    [Header("UI References")]
    public Transform contentParent;
    public GameObject lobbyPreviewPrefab;

    private Callback<LobbyMatchList_t> lobbyListCallback;
    private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;

    private List<CSteamID> currentLobbies = new List<CSteamID>();

    [SerializeField]
    TMP_Text loadingText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not initialized, can't browse lobbies!");
            return;
        }

        lobbyListCallback = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
        lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
    }

    public void RefreshLobbies()
    {
        ClearLobbyList();
        Debug.Log("Requesting lobby list...");
        SteamMatchmaking.AddRequestLobbyListStringFilter("game_id", "xXBallerXx", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamAPICall_t call = SteamMatchmaking.RequestLobbyList();
        loadingText.text = "Refreshing...";
    }

    private void OnLobbyMatchList(LobbyMatchList_t result)
    {
        Debug.Log("Found lobbies: " + result.m_nLobbiesMatching);
        if (result.m_nLobbiesMatching == 0) 
        {
            loadingText.text = "No lobbies found";
        }
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            currentLobbies.Add(lobbyID);

            // Požádáme o data lobby
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t result)
    {
        if (result.m_bSuccess != 1) return;

        CSteamID lobbyID = new CSteamID(result.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(lobbyID, "HostAddress");
        string lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, "name");

        if (string.IsNullOrEmpty(lobbyName))
            lobbyName = name + "'s lobby";

        GameObject newEntry = Instantiate(lobbyPreviewPrefab, contentParent);
        LobbyPreviewUI preview = newEntry.GetComponent<LobbyPreviewUI>();
        if (preview != null)
        {
            preview.SetLobbyInfo(lobbyID, lobbyName, hostAddress);
        }
    }

    private void ClearLobbyList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        currentLobbies.Clear();
        loadingText.text = "Loading";
    }
}
