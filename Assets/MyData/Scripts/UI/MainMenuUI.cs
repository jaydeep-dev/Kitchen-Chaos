using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button singlePlayerBtn;
    [SerializeField] private Button multiPlayerBtn;
    [SerializeField] private Button quitBtn;

    private void Awake()
    {
        singlePlayerBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.playMultiplayer = false;
            Loader.LoadScene(Loader.Scene.Lobby);
        });
        multiPlayerBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.playMultiplayer = true;
            Loader.LoadScene(Loader.Scene.Lobby);
        });

        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        Time.timeScale = 1.0f;
    }
}
