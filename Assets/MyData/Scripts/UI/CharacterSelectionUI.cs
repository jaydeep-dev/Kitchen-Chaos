using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button readyBtn;

    private void Awake()
    {
        readyBtn.onClick.AddListener(() =>
        {
            PlayerReadyHandler.Instance.SetPlayerReady();
            readyBtn.interactable = false;
        });

        mainMenuBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.LoadScene(Loader.Scene.MainMenu);
        });
    }
}
