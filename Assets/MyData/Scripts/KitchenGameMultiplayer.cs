using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public static KitchenGameMultiplayer Instance { get; private set; }

    public PlayerColorSO playerColorSO;
    private const int MAX_PLAYER_COUNT = 4;

    public event EventHandler OnTryingToJoin;
    public event EventHandler OnFailedToJoin;
    public event EventHandler OnPlayerDataNetworkListChanged;

    [SerializeField] private KitchenObjectsListSO kitchenObjectsListSO;

    private NetworkList<PlayerData> playerDataNetworkList;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_OnConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData { clientId = clientId, colorId = GetFirstUsusedColorId(), });
    }

    private void NetworkManager_OnConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if(SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelection.ToString())
        {
            response.Approved = false;
            response.Reason = "Game is already started!";
            return;
        }

        if(NetworkManager.Singleton.ConnectedClientsIds.Count > MAX_PLAYER_COUNT)
        {
            response.Approved = false;
            response.Reason = "Game is FULL!";
            return;
        }

        response.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoin?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        OnFailedToJoin?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        SpawnKitchenObjectServerRpc(GetIndexFromKitchenObjectSO(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }

    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        ClearKitchenParentClientRpc(networkObjectReference);

        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    private void ClearKitchenParentClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        kitchenObject.ClearKitchenObjectOnParent();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        var kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        var prefab = Instantiate(kitchenObjectSO.prefab);

        var prefabNetworkObject = prefab.GetComponent<NetworkObject>();
        prefabNetworkObject.Spawn(true);

        KitchenObject kitchenObject = prefab.GetComponent<KitchenObject>();

        kitchenObjectParentNetworkObjectRef.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObjectParent = kitchenObjectNetworkObject.GetComponent<IKitchenObjectParent>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    public List<Color> GetPlayerColorList()
    {
        return playerColorSO.playerColors;
    }

    public int GetIndexFromKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectsListSO.kitchenObjectsSOList.IndexOf(kitchenObjectSO);
    }

    public KitchenObjectSO GetKitchenObjectSOFromIndex(int index)
    {
        return kitchenObjectsListSO.kitchenObjectsSOList[index];
    }

    public bool IsPlayerIndexConnected(int index)
    {
        return index < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int index)
    {
        return playerColorSO.playerColors[index];
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (var data in playerDataNetworkList)
        {
            if (data.clientId == clientId)
            {
                return data;
            }
        }

        return default;
    }
    
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    public PlayerData GetLocalPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            // Color is not available
            return;
        }

        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.colorId = colorId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (var playerData in playerDataNetworkList)
        {
            if (playerData.colorId == colorId)
            {
                // Already In Use
                return false;
            }
        }

        return true;
    }

    private int GetFirstUsusedColorId()
    {
        for (int i = 0; i < playerColorSO.playerColors.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }

        return -1;
    }
}
