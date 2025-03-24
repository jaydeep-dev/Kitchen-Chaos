using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractObjectPlaceObjectOnCounterServerRpc();
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
            if (player.HasKitchenObject()) // Has Kitchen Object
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate)) // KitchenObject is a plate
                {
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) // Plate can add current ingredient
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
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

    [ServerRpc(RequireOwnership = false)]
    private void InteractObjectPlaceObjectOnCounterServerRpc()
    {
        InteractObjectPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractObjectPlaceObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
    }

    public override void InteractAlt(PlayerController player)
    {
        Debug.Log("Interact Alt!");
        if(HasKitchenObject() && HasCuttingRecipe(GetKitchenObject().GetKitchenObjectSO()))
        {
            CutObjectServerRpc();   
            TestCuttingProgressServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        if (HasKitchenObject() && HasCuttingRecipe(GetKitchenObject().GetKitchenObjectSO()))
        {
            CutObjectClientRpc();
        }
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        cuttingProgress += 1;
        // There is a kitchen object that can be cut
        CuttingRecipeSO recipeSO = FindCuttingRecipe(GetKitchenObject().GetKitchenObjectSO());

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = (float)cuttingProgress / recipeSO.maxCutCount });
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressServerRpc()
    {
        if (HasKitchenObject() && HasCuttingRecipe(GetKitchenObject().GetKitchenObjectSO()))
        {
            CuttingRecipeSO recipeSO = FindCuttingRecipe(GetKitchenObject().GetKitchenObjectSO());
            if (cuttingProgress >= recipeSO.maxCutCount)
            {
                // Remove previous kitchen object   
                KitchenObject.DestroyKitchenObject(GetKitchenObject());

                // Spawn it's cut version
                KitchenObject.SpawnKitchenObject(recipeSO.output, this);
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
