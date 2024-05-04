using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform recipeIconContainer;
    [SerializeField] private Transform recipeIconTemplate;

    private void Awake()
    {
        recipeIconTemplate.gameObject.SetActive(false);
    }

    public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.recipeName;

        foreach (Transform child in recipeIconContainer)
        {
            if (child == recipeIconTemplate)
                continue;

            Destroy(child.gameObject);
        }

        foreach(var recipe in recipeSO.kitchenObjectSOList)
        {
            var iconUI = Instantiate(recipeIconTemplate, recipeIconContainer);
            iconUI.gameObject.SetActive(true);
            iconUI.GetChild(0).GetComponent<Image>().sprite = recipe.icon;
        }
    }
}
