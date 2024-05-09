using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyMoveUpText;
    [SerializeField] private TextMeshProUGUI keyMoveDownText;
    [SerializeField] private TextMeshProUGUI keyMoveLeftText;
    [SerializeField] private TextMeshProUGUI keyMoveRightText;
    [SerializeField] private TextMeshProUGUI keyInteractText;
    [SerializeField] private TextMeshProUGUI keyInteractAltText;
    [SerializeField] private TextMeshProUGUI keyPauseText;
    [Space(20)]
    [SerializeField] private TextMeshProUGUI keyMoveGamepadText;
    [SerializeField] private TextMeshProUGUI keyInteractGamepadText;
    [SerializeField] private TextMeshProUGUI keyInteractAltGamepadText;
    [SerializeField] private TextMeshProUGUI keyPauseGamepadText;

    private void Start()
    {
        InputHandler.Instance.OnRebindBindingCompleted += InputHandler_OnRebindBindingCompleted;
        GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
        Debug.Log("<TutorialUI> Tutorial UI Subscribed");
        Show();
        UpdateVisual();
    }

    private void GameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsLocalPlayerReady())
        {
            Hide();
        }
    }

    private void InputHandler_OnRebindBindingCompleted(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        keyMoveUpText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Up);
        keyMoveDownText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Down);
        keyMoveLeftText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Left);
        keyMoveRightText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Right);
        keyInteractText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Interact);
        keyInteractAltText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Interact_Alt);
        keyPauseText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Pause);

        keyMoveGamepadText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Gamepad);
        keyInteractGamepadText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Interact_Gamepad);
        keyInteractAltGamepadText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Interact_Alt_Gamepad);
        keyPauseGamepadText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Pause_Gamepad);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
