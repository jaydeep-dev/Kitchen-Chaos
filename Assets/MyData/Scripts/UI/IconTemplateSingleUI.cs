using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconTemplateSingleUI : MonoBehaviour
{
    [SerializeField] private Image iconImg;

    public void SetVisual(KitchenObjectSO kitchenObjectSO)
    {
        iconImg.sprite = kitchenObjectSO.icon;
    }
}
