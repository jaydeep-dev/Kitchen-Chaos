using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    public static event EventHandler OnAnyCut;
    public event EventHandler OnCut;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    [SerializeField] private List<CuttingRecipeSO> cutKitchenObjectSOList;

    private int cuttingProgress;

    public static new void ResetStaticData()
    {
        OnAnyCut = null;
    }

    public override void Interact(PlayerController player)
    {
        Debug.Log("Interact!");
        if (!HasKitchenObject())
        {
            // No Kitchen Object placed on counter
            if (player.HasKitchenObject())
            {
                // Player has Kitchen Object
                if (HasCuttingRecipe(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // Player can cut the kitchen object
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    cuttingProgress = 0;
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = cuttingProgress });
                }
            }
            else
            {
                // Player is not carrying anything
                // Don't do anything
            }
        }
        else
        {
            // Has Kitchen Object
            if (player.HasKitchenObject())
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
                {
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
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

    public override void InteractAlt(PlayerController player)
    {
        Debug.Log("Interact Alt!");
        if(HasKitchenObject() && HasCuttingRecipe(GetKitchenObject().GetKitchenObjectSO()))
        {
            cuttingProgress += 1;
            // There is a kitchen object that can be cut
            KitchenObjectSO currentKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();
            CuttingRecipeSO recipe = FindCuttingRecipe(currentKitchenObjectSO);

            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = (float)cuttingProgress / recipe.maxCutCount });

            if (cuttingProgress >= recipe.maxCutCount)
            {
                // Remove previous kitchen object   
                GetKitchenObject().DestroySelf();

                // Spawn it's cut version
                KitchenObject.SpawnKitchenObject(recipe.output, this);
            }
        }
    }

    private bool HasCuttingRecipe(KitchenObjectSO inputKitchenObjectSO)
    {
        return FindCuttingRecipe(inputKitchenObjectSO) != null;
    }

    private CuttingRecipeSO FindCuttingRecipe(KitchenObjectSO inputKitchenObjectSO)
    {
        // Get Output object
        return cutKitchenObjectSOList.Find(recipeSO => recipeSO.input == inputKitchenObjectSO);
    }
}
