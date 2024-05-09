using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public override void Interact(PlayerController player)
    {
        if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                // Only accept food on plate
                DeliveryManager.Instance.DelieverRecipe(plateKitchenObject);

                KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
            }
        }
    }
}
