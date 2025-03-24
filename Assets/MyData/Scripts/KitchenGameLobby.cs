using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class KitchenGameLobby : MonoBehaviour
{
    private const string KEY_RELAY_JOIN_CODE = "relayJoinCode";

#if UNITY_WEBGL
    private const string NETWORK_ENCRYPTION_KEY = "wss";
    private bool isUsingWebSockets = true;
#else
    private const string NETWORK_ENCRYPTION_KEY = "dtls";
    private bool isUsingWebSockets = false;
#endif

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
        {
            Debug.Log("Not signed in");
            return;
        }

        if (joinedLobby != null)
            return;

        if (SceneManager.GetActiveScene().name != Loader.Scene.Lobby.ToString())
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
            Debug.Log("Initializing Unity Services...");
            InitializationOptions initilizationOption = new();
            // Uncomment below line when for testing on same device
            //initilizationOption.SetProfile(Random.Range(0, int.MaxValue).ToString());
            await UnityServices.InitializeAsync(initilizationOption);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(KitchenGameMultiplayer.MAX_PLAYER_COUNT - 1);
            return allocation;
        }
        catch (RelayServiceException error)
        {
            Debug.Log(error);
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return joinCode;
        }
        catch (RelayServiceException error)
        {
            Debug.Log(error);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;

        }
        catch (RelayServiceException error)
        {
            Debug.Log(error);
            return default;
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, KitchenGameMultiplayer.MAX_PLAYER_COUNT, new CreateLobbyOptions { IsPrivate = isPrivate });

            Debug.Log($"Lobby created with id: {joinedLobby.Id}");

            var allocation = await AllocateRelay();

            Debug.Log($"Relay allocated with id: {allocation.AllocationId}");

            var relayJoinCode = await GetRelayJoinCode(allocation);

            Debug.Log($"Relay join code: {relayJoinCode} && Lobby code: {joinedLobby.LobbyCode}");

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            Debug.Log("Lobby data updated with relay join code");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, NETWORK_ENCRYPTION_KEY));
            NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = isUsingWebSockets;

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

            await SetupJoinRelayConnection();

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

            await SetupJoinRelayConnection();

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

            await SetupJoinRelayConnection();

            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task SetupJoinRelayConnection()
    {
        var relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

        var joinAllocation = await JoinRelay(relayJoinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, NETWORK_ENCRYPTION_KEY));
        NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = isUsingWebSockets;
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
