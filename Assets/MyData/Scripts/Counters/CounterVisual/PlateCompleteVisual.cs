using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [System.Serializable]
    public struct KitchenObjectSO_Gameobject
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObject;
    }

    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSO_Gameobject> kitchenObjectSO_GameobjectsList;

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
        foreach (KitchenObjectSO_Gameobject item in kitchenObjectSO_GameobjectsList)
        {
            item.gameObject.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach(KitchenObjectSO_Gameobject item in kitchenObjectSO_GameobjectsList)
        {
            if(item.kitchenObjectSO == e.kitchenObjectSO)
            {
                item.gameObject.SetActive(true);
            }
        }
    }


}
