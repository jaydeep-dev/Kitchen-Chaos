using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using Random = UnityEngine.Random;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;

    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinFailed;

    public event EventHandler<OnLobbiesListChangedEventArgs> OnLobbiesListChanged;
    public class OnLobbiesListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbies;
    }

    private Lobby joinedLobby;

    private float lobbyUpdateRate;
    private float lobbyHeartBeatTimer;

    public enum JoinMethod
    {
        QuickJoin,
        JoinById,
        JoinByCode,
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitilizeUnityAuthentication();
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleUpdateLobbyListUI();
    }
    
    private void HandleUpdateLobbyListUI()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
            return;

        if (joinedLobby != null)
            return;

        lobbyUpdateRate -= Time.deltaTime;

        if (lobbyUpdateRate < 0)
        {
            float lobbyUpdateRateMax = 5f;
            lobbyUpdateRate = lobbyUpdateRateMax;

            ListLobbies();
        }
    }

    private async void ListLobbies()
    {
        QueryLobbiesOptions lobbiesOptions = new()
        {
            Filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
        };

        var queryResult = await LobbyService.Instance.QueryLobbiesAsync(lobbiesOptions);
        OnLobbiesListChanged?.Invoke(this, new OnLobbiesListChangedEventArgs { lobbies = queryResult.Results });
    }

    private void HandleLobbyHeartBeat()
    {
        if (!IsLobbyHost())
            return;

        lobbyHeartBeatTimer -= Time.deltaTime;

        if (lobbyHeartBeatTimer < 0)
        {
            float lobbyHeartBeatTimerMax = 15f;
            lobbyHeartBeatTimer = lobbyHeartBeatTimerMax;

            LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
        }
    }

    private async void InitilizeUnityAuthentication()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            InitializationOptions initilizationOption = new();
            initilizationOption.SetProfile(Random.Range(int.MinValue, int.MaxValue).ToString());
            await UnityServices.InitializeAsync(initilizationOption);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, KitchenGameMultiplayer.MAX_PLAYER_COUNT, new CreateLobbyOptions() { IsPrivate = isPrivate });

            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadSceneViaNetwork(Loader.Scene.CharacterSelection);
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void JoinLobby(string lobbyParam, JoinMethod joinMethod)
    {
        switch (joinMethod)
        {
            case JoinMethod.QuickJoin:
                QuickJoin();
                break;
            case JoinMethod.JoinById:
                JoinById(lobbyParam);
                break;
            case JoinMethod.JoinByCode:
                JoinByCode(lobbyParam);
                break;
        }
    }

    private async void QuickJoin()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    private async void JoinByCode(string lobbyCode)
    {
        OnJoinStarted.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    
    private async void JoinById(string lobbyId)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
            }
            catch (LobbyServiceException error)
            {
                Debug.Log(error);
            }
        }
    }
    
    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
            }
            catch (LobbyServiceException error)
            {
                Debug.Log(error);
            }
        }
    }
    
    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException error)
            {
                Debug.Log(error);
            }
        }
    }

    public bool IsLobbyHost() => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    public Lobby GetLobby() => joinedLobby;
}
