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
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        
        clientBtn.onClick.AddListener(() =>
        {
            Debug.Log("Client Clicked");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
