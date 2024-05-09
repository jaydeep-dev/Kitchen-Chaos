using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;


    public override void Interact(PlayerController player)
    {
        Debug.Log("Interact!");
        // No Kitchen Object
        if(!HasKitchenObject())
        {
            // Player has Kitchen Object
            if (player.HasKitchenObject())
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            else
            {
                // Player is not carrying anything
                // Don't do anything
            }
        }
        // Has Kitchen Object
        else
        {
            // Player has kitchen object
            if(player.HasKitchenObject())
            {
                // Player is holding plate kitchen object
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
                {
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
                // Player is holding something but not a plate
                else
                {
                    // This counter is holding a plate
                    if (GetKitchenObject().TryGetPlate(out plate))
                    {
                        // Try to add what is player holding onto plate
                        if (plate.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
                        }
                    }
                }
            }
            else
            {
                // Give kitchen object to player as player has nothing ATM
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}
