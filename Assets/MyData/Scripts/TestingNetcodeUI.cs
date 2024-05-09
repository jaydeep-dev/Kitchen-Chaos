using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        hostBtn.onClick.AddListener(() =>
        {
            Debug.Log("Host Clicked");
            KitchenGameMultiplayer.Instance.StartHost();
            Hide();
        });
        
        clientBtn.onClick.AddListener(() =>
        {
            Debug.Log("Client Clicked");
            KitchenGameMultiplayer.Instance.StartClient();
            Hide();
        });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
