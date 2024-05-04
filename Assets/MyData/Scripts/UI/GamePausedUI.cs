using System.Collections;
using System.Collections.Generic;
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
        mainmenuBtn.onClick.AddListener(() => Loader.LoadScene(Loader.Scene.MainMenu));
    }

    private void Start()
    {
        GameManager.Instance.OnPauseStatusChanged += GameManager_OnPauseStatusChanged;

        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPauseStatusChanged -= GameManager_OnPauseStatusChanged;
    }

    private void GameManager_OnPauseStatusChanged(bool isPaused)
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
