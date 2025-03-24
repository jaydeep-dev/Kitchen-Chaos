using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessegeUI : MonoBehaviour
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI messegeText;

    private void Awake()
    {
        closeBtn.onClick.AddListener(() => Hide());
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnFailedToJoin += KitchenGameMultiplayer_OnFailedToJoin;

        KitchenGameLobby.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnCreateLobbyFailed += KitchenGameLobby_OnCreateLobbyFailed;

        KitchenGameLobby.Instance.OnJoinStarted += KitchenGameLobby_OnJoinStarted;
        KitchenGameLobby.Instance.OnJoinFailed += KitchenGameLobby_OnJoinFailed;
        KitchenGameLobby.Instance.OnQuickJoinFailed += KitchenGameLobby_OnQuickJoinFailed;
        Hide();
    }

    private void KitchenGameLobby_OnQuickJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessege("Can't find a Lobby to Quick Join!");
    }

    private void KitchenGameLobby_OnJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessege("Failed to join Lobby!");
    }

    private void KitchenGameLobby_OnJoinStarted(object sender, System.EventArgs e)
    {
        ShowMessege("Joining Lobby...");
    }

    private void KitchenGameLobby_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessege("Failed to create Lobby!");
    }

    private void KitchenGameLobby_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessege("Creating Lobby...");
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnFailedToJoin -= KitchenGameMultiplayer_OnFailedToJoin;
        
        KitchenGameLobby.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnCreateLobbyFailed -= KitchenGameLobby_OnCreateLobbyFailed;

        KitchenGameLobby.Instance.OnJoinStarted -= KitchenGameLobby_OnJoinStarted;
        KitchenGameLobby.Instance.OnJoinFailed -= KitchenGameLobby_OnJoinFailed;
        KitchenGameLobby.Instance.OnQuickJoinFailed -= KitchenGameLobby_OnQuickJoinFailed;
    }

    private void KitchenGameMultiplayer_OnFailedToJoin(object sender, System.EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason != string.Empty)
            ShowMessege(NetworkManager.Singleton.DisconnectReason);
        else
            ShowMessege("Failed to connect!");
    }

    private void ShowMessege(string messege)
    {
        Show();
        messegeText.text = messege;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
