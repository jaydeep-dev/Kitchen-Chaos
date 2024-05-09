using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public static KitchenGameMultiplayer Instance { get; private set; }

    [SerializeField] private KitchenObjectsListSO kitchenObjectsListSO;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        SpawnKitchenObjectServerRpc(GetIndexFromKitchenObjectSO(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        var kitchenObjectSO = GetKitchenObjectFromIndex(kitchenObjectSOIndex);
        var prefab = Instantiate(kitchenObjectSO.prefab);

        var prefabNetworkObject = prefab.GetComponent<NetworkObject>();
        prefabNetworkObject.Spawn(true);

        KitchenObject kitchenObject = prefab.GetComponent<KitchenObject>();

        kitchenObjectParentNetworkObjectRef.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObjectParent = kitchenObjectNetworkObject.GetComponent<IKitchenObjectParent>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    private int GetIndexFromKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectsListSO.kitchenObjectsSOList.IndexOf(kitchenObjectSO);
    }

    private KitchenObjectSO GetKitchenObjectFromIndex(int index)
    {
        return kitchenObjectsListSO.kitchenObjectsSOList[index];
    }
}
