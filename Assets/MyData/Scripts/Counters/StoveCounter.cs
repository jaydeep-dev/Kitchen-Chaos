using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class StoveCounter : BaseCounter, IHasProgress
{
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State State;
    }

    [SerializeField] private NetworkVariable<State> state = new(State.Idle);
    [SerializeField] private List<FryingRecipeSO> fryingRecipeSOList;
    [SerializeField] private List<BurningRecipeSO> burningRecipeSOList;

    private NetworkVariable<float> fryingTimer = new(0f);
    private FryingRecipeSO fryingRecipe;

    private NetworkVariable<float> burningTimer = new(0f);
    private BurningRecipeSO burningRecipe;

    private void Start()
    {
        state.Value = State.Idle;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { State = state.Value });
    }

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += OnFryingTimerValueChanged;
        burningTimer.OnValueChanged += OnBurningTimerValueChanged;
        state.OnValueChanged += OnStateValueChanged;
    }

    private void OnStateValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { State = newValue });

        if (newValue == State.Idle || newValue == State.Burned)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0 });
        }
    }

    private void OnFryingTimerValueChanged(float previousValue, float newValue)
    {
        float maxFryingTimer = fryingRecipe != null ? fryingRecipe.maxFryingTimer : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = newValue / maxFryingTimer });
    }
    
    private void OnBurningTimerValueChanged(float previousValue, float newValue)
    {
        float maxBurningTimer = burningRecipe != null ? burningRecipe.maxBurningTimer : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = newValue / maxBurningTimer });
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer.Value += Time.deltaTime;

                    if (fryingTimer.Value >= fryingRecipe.maxFryingTimer)
                    {
                        fryingTimer.Value = 0;
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(fryingRecipe.output, this);

                        burningTimer.Value = 0;
                        var kitchenObjectSOIndex = KitchenGameMultiplayer.Instance.GetIndexFromKitchenObjectSO(fryingRecipe.output);
                        SetBurningRecipeSOClientRpc(kitchenObjectSOIndex);

                        state.Value = State.Fried;
                    }
                    break;
                case State.Fried:
                    burningTimer.Value += Time.deltaTime;

                    if (burningTimer.Value >= burningRecipe.maxBurningTimer)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(burningRecipe.output, this);

                        state.Value = State.Burned;

                    }
                    break;
                case State.Burned:
                    break;
                default:
                    break;
            }

        }

    }

    public override void Interact(PlayerController player)
    {
        Debug.Log("Interact!");
        if (!HasKitchenObject())
        {
            // No Kitchen Object placed on counter
            if (player.HasKitchenObject())
            {
                // Player has Kitchen Object
                if (HasFryingRecipe(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // Player can fry the kitchen object
                    var kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    int kitchenObjectSOIndex = KitchenGameMultiplayer.Instance.GetIndexFromKitchenObjectSO(kitchenObject.GetKitchenObjectSO());
                    InteractLogicServerRpc(kitchenObjectSOIndex);
                }
            }
            else
            {
                // Player is not carrying anything
                // Don't do anything
            }
        }
        else
        {
            if (player.HasKitchenObject()) // Has Kitchen Object
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate)) // KitchenObject is a plate
                {
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) // Plate can add currently cooking ingredient
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        SetStateIdleServerRpc();
                    }
                }
            }
            else
            {
                // Give kitchen object to player as player has nothing ATM
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc(int kitchenObjectSOIndex)
    {
        fryingTimer.Value = 0;
        state.Value = State.Frying;

        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        var kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        fryingRecipe = FindFryingRecipe(kitchenObjectSO);
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        var kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        burningRecipe = FindBurningRecipe(kitchenObjectSO);
    }


    private bool HasFryingRecipe(KitchenObjectSO inputKitchenObjectSO)
    {
        return FindFryingRecipe(inputKitchenObjectSO) != null;
    }

    private FryingRecipeSO FindFryingRecipe(KitchenObjectSO inputKitchenObjectSO)
    {
        // Get Output object
        return fryingRecipeSOList.Find(recipeSO => recipeSO.input == inputKitchenObjectSO);
    }
    
    private BurningRecipeSO FindBurningRecipe(KitchenObjectSO inputKitchenObjectSO)
    {
        // Get Output object
        return burningRecipeSOList.Find(recipeSO => recipeSO.input == inputKitchenObjectSO);
    }

    public bool IsFried()
    {
        return state.Value == State.Fried;
    }
}
