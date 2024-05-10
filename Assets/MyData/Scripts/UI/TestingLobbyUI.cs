using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameBtn;
    [SerializeField] private Button JoinGameBtn;

    private void Awake()
    {
        createGameBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadSceneViaNetwork(Loader.Scene.CharacterSelection);
        });

        JoinGameBtn.onClick.AddListener(() => 
        {
            KitchenGameMultiplayer.Instance.StartClient();
        });
    }
}
