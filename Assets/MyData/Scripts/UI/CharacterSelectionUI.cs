using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button readyBtn;

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    private void Awake()
    {
        readyBtn.onClick.AddListener(() =>
        {
            PlayerReadyHandler.Instance.SetPlayerReady();
            readyBtn.interactable = false;
        });

        mainMenuBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            Loader.LoadScene(Loader.Scene.MainMenu);
        });
    }

    private void Start()
    {
        var lobby = KitchenGameLobby.Instance.GetLobby();
        lobbyNameText.text = $"Lobby Name: {lobby.Name}";
        lobbyCodeText.text = $"Lobby Code: {lobby.LobbyCode}";
    }
}
