using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event Action<bool> OnPauseStatusChanged;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state;

    private float countdownToStartCountdown = 3f;
    private float gameplayTimer;
    private float maxGameplayTimer = 5 * 60f;
    private bool isGamePaused = false;

    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }

    private void Start()
    {
        InputHandler.Instance.OnPauseAction += InputHandler_OnPauseAction;
        InputHandler.Instance.OnInteractAction += InputHandler_OnInteractAction;
    }

    private void InputHandler_OnInteractAction(object sender, EventArgs e)
    {
        if(state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void InputHandler_OnPauseAction(object sender, EventArgs e)
    {
        ToggleGamePauseStatus();
    }

    public void ToggleGamePauseStatus()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        OnPauseStatusChanged?.Invoke(isGamePaused);
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                break;

            case State.CountdownToStart:
                countdownToStartCountdown -= Time.deltaTime;
                if (countdownToStartCountdown < 0f)
                {
                    state = State.GamePlaying;
                    gameplayTimer = maxGameplayTimer;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                gameplayTimer -= Time.deltaTime;
                if (gameplayTimer < 0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
            default:
                break;
        }
    }

    public bool IsGamePlaying() => state == State.GamePlaying;

    public bool IsGameCountdownToStart() => state == State.CountdownToStart;

    public float GetCountdownToStartTimer() => countdownToStartCountdown;

    public bool IsGameOver() => state == State.GameOver;

    public float GetPlayingTimerNormalized() => 1 - (gameplayTimer / maxGameplayTimer);
}
