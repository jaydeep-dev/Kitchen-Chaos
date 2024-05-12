using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTemplateSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;

    private Lobby lobby;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinLobby(lobby.Id, KitchenGameLobby.JoinMethod.JoinById);
        });
    }

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyName.text = lobby.Name;
    }
}
