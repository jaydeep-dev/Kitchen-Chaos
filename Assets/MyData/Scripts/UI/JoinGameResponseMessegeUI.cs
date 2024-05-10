using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameResponseMessegeUI : MonoBehaviour
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

        Hide();
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnFailedToJoin -= KitchenGameMultiplayer_OnFailedToJoin;
    }

    private void KitchenGameMultiplayer_OnFailedToJoin(object sender, System.EventArgs e)
    {
        Show();

        messegeText.text = NetworkManager.Singleton.DisconnectReason;

        if (messegeText.text == string.Empty)
        {
            messegeText.text = "Failed to connect!";
        }
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
