using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnythingPlaced;
    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    public static void ResetStaticData()
    {
        OnAnythingPlaced = null;
    }

    public virtual void Interact(PlayerController player) 
    {
        Debug.LogError("Base Interact()! This should never happen");
    }

    public virtual void InteractAlt(PlayerController player) 
    {
        //Debug.LogError("Base InteractAlt()! This should never happen");
    }

    public Transform GetKitchenObjectFollowTransform() { return counterTopPoint; }

    public KitchenObject GetKitchenObject() { return kitchenObject; }

    public void SetKitchenObject(KitchenObject kitchenObject) 
    {
        this.kitchenObject = kitchenObject; 

        if (kitchenObject != null )
        {
            OnAnythingPlaced?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ClearKitchenObject() { kitchenObject = null; }

    public bool HasKitchenObject() { return kitchenObject != null; }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
