using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private Transform iconTemplate;

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
        iconTemplate.gameObject.SetActive(false);
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach (Transform child in transform)
        {
            if (child == iconTemplate)
                continue;
            Destroy(child.gameObject);
        }

        foreach(var itemSO in plateKitchenObject.GetKitchenObjectList())
        {
            var icon = Instantiate(iconTemplate, transform);
            var iconUI = icon.GetComponent<IconTemplateSingleUI>();
            iconUI.SetVisual(itemSO);
            icon.gameObject.SetActive(true);
        }
    }
}
