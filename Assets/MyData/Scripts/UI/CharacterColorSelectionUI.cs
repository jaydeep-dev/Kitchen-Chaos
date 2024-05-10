using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterColorSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject colorTemplate;
    [SerializeField] private Transform colorSelectionParent;

    private void Awake()
    {
        colorTemplate.SetActive(false);

        SetSelectableColorUI();
    }

    private void SetSelectableColorUI()
    {
        for (int i = 0; i < KitchenGameMultiplayer.Instance.GetPlayerColorList().Count; i++)
        {
            var colorGO = Instantiate(colorTemplate, colorSelectionParent);
            var colorSingleUI = colorGO.GetComponent<PlayerColorSingleUI>();
            colorSingleUI.SetColorId(i);
            colorGO.SetActive(true);
        }
    }
}
