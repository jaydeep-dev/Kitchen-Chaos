using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;

    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;
    private List<RecipeSO> waitingRecipiesList = new();

    public int SuccessfulDeliveries { get; private set; }
    private int waitingRecipeMax = 6;
    private float recipeTimer;
    private float recipeTimerMax = 4f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
            return;

        if (waitingRecipiesList.Count >= waitingRecipeMax)
            return;

        recipeTimer -= Time.deltaTime;
        if (recipeTimer < 0)
        {
            recipeTimer = recipeTimerMax;
            var waitingRecipeSO = recipeListSO.Recipes[Random.Range(0, recipeListSO.Recipes.Count)];
            Debug.Log(waitingRecipeSO.recipeName);
            waitingRecipiesList.Add(waitingRecipeSO);

            OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public void DelieverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i< waitingRecipiesList.Count; i++)
        {
            var waitingRecipeSO = waitingRecipiesList[i];
            if(waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectList().Count)
            {
                bool isValidDelievery = true;
                // Has same number of ingredient
                foreach (var requiredIngredient in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool isFound = false;
                    foreach (var currentIngredient in plateKitchenObject.GetKitchenObjectList())
                    {
                        if(currentIngredient == requiredIngredient)
                        {
                            // Matches
                            isFound = true;
                            break;
                        }
                    }

                    if (!isFound)
                    {
                        // The recipe is not found on plate
                        isValidDelievery = false;
                    }
                }

                if (isValidDelievery)
                {
                    // Player delievered correct recipe!
                    SuccessfulDeliveries++;
                    waitingRecipiesList.RemoveAt(i);
                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }

        // Player did not delievered correct recipe!
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipiesList;
    }
}
