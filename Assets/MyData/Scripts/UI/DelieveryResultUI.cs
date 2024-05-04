using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DelieveryResultUI : MonoBehaviour
{
    private readonly int PopupHash = Animator.StringToHash("Popup");

    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI delieveryResultText;

    [SerializeField] private Color successColor;
    [SerializeField] private Color failureColor;

    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failureSprite;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;

        gameObject.SetActive(false);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);

        backgroundImage.color = failureColor;
        iconImage.sprite = failureSprite;
        delieveryResultText.text = "Delivery\nFailed";
        animator.SetTrigger(PopupHash);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);

        backgroundImage.color = successColor;
        iconImage.sprite = successSprite;
        delieveryResultText.text = "Delivery\nSuccess";
        animator.SetTrigger(PopupHash);
    }
}
