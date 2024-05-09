using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePausedUI : MonoBehaviour
{

    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button mainmenuBtn;

    private void Awake()
    {
        resumeBtn.onClick.AddListener(() => GameManager.Instance.ToggleGamePauseStatus());
        optionsBtn.onClick.AddListener(() => OptionsUI.Instance.Show());
        mainmenuBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.LoadScene(Loader.Scene.MainMenu);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnLocalPauseStatusChanged += GameManager_OnLocalPauseStatusChanged;

        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnLocalPauseStatusChanged -= GameManager_OnLocalPauseStatusChanged;
    }

    private void GameManager_OnLocalPauseStatusChanged(bool isPaused)
    {
        if (isPaused)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        resumeBtn.Select();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
