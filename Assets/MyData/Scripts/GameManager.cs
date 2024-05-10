using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event Action<bool> OnLocalPauseStatusChanged;
    public event Action<bool> OnMultiplayerPauseStateChanged;   
    public event EventHandler OnLocalPlayerReadyChanged;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private Transform playerPrefab;

    private NetworkVariable<State> state = new(State.WaitingToStart);
    private bool isLocalPlayerReady;

    private NetworkVariable<float> countdownToStartCountdown = new(3f); // Default
    //private float countdownToStartCountdown = 1f; // Testing
    private NetworkVariable<float> gameplayTimer = new(0f);
    private float maxGameplayTimer = 5 * 60f; // Default
    //private float maxGameplayTimer = 15 * 60f; // Testing
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new(false);

    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private bool autoTestPauseStateAfterDisconnect;

    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new();
        playerPausedDictionary = new();
    }

    private void Start()
    {
        InputHandler.Instance.OnPauseAction += InputHandler_OnPauseAction;
        InputHandler.Instance.OnInteractAction += InputHandler_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += OnStateValueChanged;
        isGamePaused.OnValueChanged += OnGamePausedValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var player = Instantiate(playerPrefab);
            var playerNetworkObject = player.GetComponent<NetworkObject>();
            playerNetworkObject.SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (IsServer)
        {
            autoTestPauseStateAfterDisconnect = true;
        }
    }

    private void OnGamePausedValueChanged(bool previousValue, bool newValue)
    {
        Time.timeScale = newValue ? 0f : 1f;
        OnMultiplayerPauseStateChanged?.Invoke(newValue);
    }

    private void OnStateValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void InputHandler_OnInteractAction(object sender, EventArgs e)
    {
        if(state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsAreReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsAreReady = false;
                break;
            }
        }

        if (allClientsAreReady)
        {
            state.Value = State.CountdownToStart;
        }
    }

    private void InputHandler_OnPauseAction(object sender, EventArgs e)
    {
        ToggleGamePauseStatus();
    }

    public void ToggleGamePauseStatus()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            SetGamePauseServerRpc();
        }
        else
        {
            SetGameUnpauseServerRpc();
        }
        OnLocalPauseStatusChanged?.Invoke(isLocalGamePaused);
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;

            case State.CountdownToStart:
                countdownToStartCountdown.Value -= Time.deltaTime;
                if (countdownToStartCountdown.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gameplayTimer.Value = maxGameplayTimer;
                }
                break;
            case State.GamePlaying:
                gameplayTimer.Value -= Time.deltaTime;
                if (gameplayTimer.Value < 0f)
                {
                    state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        if (autoTestPauseStateAfterDisconnect)
        {
            autoTestPauseStateAfterDisconnect = false;
            TestPauseState();
        }
    }

    public bool IsGamePlaying() => state.Value == State.GamePlaying;

    public bool IsGameCountdownToStart() => state.Value == State.CountdownToStart;

    public bool IsGameOver() => state.Value == State.GameOver;

    public bool IsWaitingToStart() => state.Value == State.WaitingToStart;

    public float GetCountdownToStartTimer() => countdownToStartCountdown.Value;

    public bool IsLocalPlayerReady() => isLocalPlayerReady;

    public float GetPlayingTimerNormalized() => 1 - (gameplayTimer.Value / maxGameplayTimer);

    [ServerRpc(RequireOwnership = false)]
    private void SetGamePauseServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        TestPauseState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetGameUnpauseServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;

        TestPauseState();
    }

    private void TestPauseState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                // atleast 1 player paused the game
                isGamePaused.Value = true;
                return;
            }
        }

        isGamePaused.Value = false;
        // all players are unpaused
    }
}
