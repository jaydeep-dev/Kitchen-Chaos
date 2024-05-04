using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    private const string PLAYER_PREF_BINDING_JSON = "Input Bindings";

    public enum Bindings
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        Interact_Alt,
        Pause,

        Move_Gamepad,
        Interact_Gamepad,
        Interact_Alt_Gamepad,
        Pause_Gamepad,
    }

    private PlayerInputMap inputActions;
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAltAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnRebindBindingCompleted;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        inputActions = new PlayerInputMap();

        if (PlayerPrefs.HasKey(PLAYER_PREF_BINDING_JSON))
        {
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREF_BINDING_JSON));
        }

        inputActions.Player.Enable();
    }

    private void OnEnable()
    {
        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Player.InteractAlt.performed += OnInteractAlt;
        inputActions.Player.Pause.performed += Pause_performed;
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnInteractAlt(InputAction.CallbackContext obj)
    {
        OnInteractAltAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.InteractAlt.performed -= OnInteractAlt;
        inputActions.Player.Pause.performed -= Pause_performed;
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    public Vector2 GetInputVector()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        return moveInput;
    }

    public void RebindBindings(Bindings bindings, Action onRebindCompleted)
    {
        InputAction inputAction;
        int bindingIndex;

        switch (bindings)
        {
            default:
            case Bindings.Move_Up:
                inputAction = inputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Bindings.Move_Down:
                inputAction = inputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Bindings.Move_Left:
                inputAction = inputActions.Player.Move;
                bindingIndex = 5;
                break;
            case Bindings.Move_Right:
                inputAction = inputActions.Player.Move;
                bindingIndex = 7;
                break;
            case Bindings.Interact:
                inputAction = inputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Bindings.Interact_Alt:
                inputAction = inputActions.Player.InteractAlt;
                bindingIndex = 0;
                break;
            case Bindings.Pause:
                inputAction = inputActions.Player.Pause;
                bindingIndex = 0;
                break;
            case Bindings.Move_Gamepad:
                inputAction = inputActions.Player.Move;
                bindingIndex = 9;
                break;
            case Bindings.Interact_Gamepad:
                inputAction = inputActions.Player.Interact;
                bindingIndex = 1;
                break;
            case Bindings.Interact_Alt_Gamepad:
                inputAction = inputActions.Player.InteractAlt;
                bindingIndex = 1;
                break;
            case Bindings.Pause_Gamepad:
                inputAction = inputActions.Player.Pause;
                bindingIndex = 1;
                break;
        }

        inputActions.Player.Disable();

        inputAction.PerformInteractiveRebinding(bindingIndex).OnComplete(callback =>
        {
            inputActions.Player.Enable();
            callback.Dispose();

            var bindingsJson = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(PLAYER_PREF_BINDING_JSON, bindingsJson);
            PlayerPrefs.Save();
            OnRebindBindingCompleted?.Invoke(this, EventArgs.Empty);
            onRebindCompleted();
        }).Start();
    }

    public void ResetToDefaultBindings()
    {
        inputActions.Player.Disable();

        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PLAYER_PREF_BINDING_JSON);

        inputActions.Player.Enable();
    }

    public string GetBindingsText(Bindings bindings)
    {
        switch (bindings)
        {
            case Bindings.Move_Up:
                return inputActions.Player.Move.bindings[1].ToDisplayString();
            case Bindings.Move_Down:
                return inputActions.Player.Move.bindings[3].ToDisplayString();
            case Bindings.Move_Left:
                return inputActions.Player.Move.bindings[5].ToDisplayString();
            case Bindings.Move_Right:
                return inputActions.Player.Move.bindings[7].ToDisplayString();
            case Bindings.Interact:
                return inputActions.Player.Interact.bindings[0].ToDisplayString();
            case Bindings.Interact_Alt:
                return inputActions.Player.InteractAlt.bindings[0].ToDisplayString();
            case Bindings.Pause:
                return inputActions.Player.Pause.bindings[0].ToDisplayString();
            case Bindings.Move_Gamepad:
                return inputActions.Player.Move.bindings[9].ToDisplayString();
            case Bindings.Interact_Gamepad:
                return inputActions.Player.Interact.bindings[1].ToDisplayString();
            case Bindings.Interact_Alt_Gamepad:
                return inputActions.Player.InteractAlt.bindings[1].ToDisplayString();
            case Bindings.Pause_Gamepad:
                return inputActions.Player.Pause.bindings[1].ToDisplayString();
            default:
                return string.Empty;
        }
    }
}
