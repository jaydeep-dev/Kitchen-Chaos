using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform recipeContainer;
    [SerializeField] private Transform recipeTemplate;

    private void Start()
    {
        recipeTemplate.gameObject.SetActive(false);
        DeliveryManager.Instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeCompleted += DeliveryManager_OnRecipeCompleted;
    }

    private void DeliveryManager_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void DeliveryManager_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        foreach (Transform child in recipeContainer.transform)
        {
            if(child == recipeTemplate)
                continue;

            Destroy(child.gameObject);
        }

        var waitingRecipiesList = DeliveryManager.Instance.GetWaitingRecipeSOList();
        foreach (var waitingRecipe in waitingRecipiesList)
        {
            var waitingRecipeUI = Instantiate(recipeTemplate, recipeContainer);
            waitingRecipeUI.gameObject.SetActive(true);
            waitingRecipeUI.GetComponent<RecipeSingleUI>().SetRecipeSO(waitingRecipe);
        }
    }
}
