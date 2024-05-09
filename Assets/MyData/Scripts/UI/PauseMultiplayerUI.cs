using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMultiplayerUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnMultiplayerPauseStateChanged += GameManager_OnMultiplayerPauseStateChanged;

        Hide();
    }

    private void GameManager_OnMultiplayerPauseStateChanged(bool isPaused)
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

    private void Show()
    {
        gameObject.SetActive(true);
    }   
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
