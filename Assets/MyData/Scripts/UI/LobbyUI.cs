using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;

    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button quickJoinBtn;

    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button joinByCodeBtn;

    [SerializeField] private Transform lobbyTemplate;
    [SerializeField] private Transform lobbyContainer;

    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    private void Start()
    {
        lobbyTemplate.gameObject.SetActive(false);

        mainMenuBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.LeaveLobby();
            Loader.LoadScene(Loader.Scene.MainMenu);
        });

        createLobbyBtn.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });

        quickJoinBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinLobby(string.Empty, KitchenGameLobby.JoinMethod.QuickJoin);
        });

        joinCodeInput.onValueChanged.AddListener(newText =>
        {
            joinByCodeBtn.interactable = !string.IsNullOrEmpty(joinCodeInput.text) && !string.IsNullOrWhiteSpace(joinCodeInput.text);
        });

        joinByCodeBtn.interactable = false;
        joinByCodeBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinLobby(joinCodeInput.text, KitchenGameLobby.JoinMethod.JoinByCode);
        });

        playerNameInput.text = KitchenGameMultiplayer.Instance.GetPlayerName();
        playerNameInput.onValueChanged.AddListener(newPlayerName =>
        {
            KitchenGameMultiplayer.Instance.SetPlayerName(newPlayerName);
        });

        KitchenGameLobby.Instance.OnLobbiesListChanged += KitchenGameLobby_OnLobbiesListChanged;
        UpdateLobbiesListUI(new List<Lobby>());
    }

    private void OnDestroy()
    {
        KitchenGameLobby.Instance.OnLobbiesListChanged -= KitchenGameLobby_OnLobbiesListChanged;
    }

    private void UpdateLobbiesListUI(List<Lobby> lobbies)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbies)
        {
            var lobbyGO = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyGO.GetComponent<LobbyTemplateSingleUI>().SetLobby(lobby);
            lobbyGO.gameObject.SetActive(true);
        }
    }

    private void KitchenGameLobby_OnLobbiesListChanged(object sender, KitchenGameLobby.OnLobbiesListChangedEventArgs e)
    {
        UpdateLobbiesListUI(e.lobbies);
    }
}
