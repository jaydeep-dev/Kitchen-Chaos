using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private FollowTransform followTransform;

    private IKitchenObjectParent kitchenObjectParent;

    public KitchenObjectSO GetKitchenObjectSO() { return kitchenObjectSO; }

    public IKitchenObjectParent GetKitchenObjectParent() { return kitchenObjectParent; }

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent) 
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());   
    }

    public void DestroySelf()
    {
        kitchenObjectParent.ClearKitchenObject();

        Destroy(gameObject);
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject= null;
            return false;
        }
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectRef);
    }

    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        kitchenObjectParentNetworkObjectRef.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObjectParent = kitchenObjectNetworkObject.GetComponent<IKitchenObjectParent>();

        this.kitchenObjectParent?.ClearKitchenObject();

        this.kitchenObjectParent = kitchenObjectParent;
        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("Counter already has kitchen object");
        }

        kitchenObjectParent.SetKitchenObject(this);

        followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }
}
