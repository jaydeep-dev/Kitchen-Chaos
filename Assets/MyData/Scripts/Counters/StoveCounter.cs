using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private State state;
    [SerializeField] private List<FryingRecipeSO> fryingRecipeSOList;
    [SerializeField] private List<BurningRecipeSO> burningRecipeSOList;

    private float fryingTimer;
    private FryingRecipeSO fryingRecipe;

    private float burningTimer;
    private BurningRecipeSO burningRecipe;

    private void Start()
    {
        state = State.Idle;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { State = state });
    }

    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer += Time.deltaTime;
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = fryingTimer / fryingRecipe.maxFryingTimer });

                    if (fryingTimer >= fryingRecipe.maxFryingTimer)
                    {
                        fryingTimer = 0;
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(fryingRecipe.output, this);

                        burningTimer = 0;
                        burningRecipe = FindBurningRecipe(fryingRecipe.output);

                        state = State.Fried;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { State = state });
                    }
                    break;
                case State.Fried:
                    burningTimer += Time.deltaTime;
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = burningTimer / burningRecipe.maxBurningTimer });

                    if (burningTimer >= burningRecipe.maxBurningTimer)
                    {
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipe.output, this);

                        state = State.Burned;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { State = state });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0 });

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
                    fryingTimer = 0;
                    state = State.Frying;
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    fryingRecipe = FindFryingRecipe(GetKitchenObject().GetKitchenObjectSO());

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { State = state });
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = fryingTimer/fryingRecipe.maxFryingTimer });
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
            // Has Kitchen Object
            if (player.HasKitchenObject())
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
                {
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                        state = State.Idle;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { State = state });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0 });
                    }
                }
            }
            else
            {
                // Give kitchen object to player as player has nothing ATM
                GetKitchenObject().SetKitchenObjectParent(player);
                state = State.Idle;
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { State = state });
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0 });
            }
        }
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
        return state == State.Fried;
    }
}
