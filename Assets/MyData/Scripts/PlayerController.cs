using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IKitchenObjectParent
{
    public static PlayerController LocalInstance { get; private set; }

    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPlayerPickedupSomething;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
        OnAnyPlayerPickedupSomething = null;
    }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    public event EventHandler OnPickedupSomething;

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    public bool IsWalking { get; private set; }
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            LocalInstance = this;
        }

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void OnEnable()
    {
        InputHandler.Instance.OnInteractAction += OnInteractAction;
        InputHandler.Instance.OnInteractAltAction += OnInteractAltAction;
    }

    private void OnInteractAltAction(object sender, EventArgs e)
    {
        if(!GameManager.Instance.IsGamePlaying()) { return; }

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlt(this);
        }
    }

    private void OnInteractAction(object sender, EventArgs e)
    {
        if(!GameManager.Instance.IsGamePlaying()) { return; }

        if(selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void OnDisable()
    {
        InputHandler.Instance.OnInteractAction -= OnInteractAction;
        InputHandler.Instance.OnInteractAltAction -= OnInteractAltAction;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        HandleMovement();

        HandleInteraction();
    }

    private void HandleInteraction()
    {
        var moveInput = InputHandler.Instance.GetInputVector();

        var moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if(moveDir != Vector3.zero)
            lastInteractDir = moveDir;

        const float maxInteractionDistance = 2f;
        if(Physics.Raycast(transform.position, lastInteractDir, out RaycastHit hit, maxInteractionDistance, counterLayerMask))
        {
            if(hit.transform.TryGetComponent<BaseCounter>(out var baseCounter))
            {
                SetSelectedCounter(baseCounter);
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        var moveInput = InputHandler.Instance.GetInputVector();

        var moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        float playerRadius = .7f;
        float playerHeight = 2f;
        float moveDistance = moveSpeed * Time.deltaTime;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            // Attempt Only X Movement
            var moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                // Attempt Only Z Movement
                var moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove)
                {
                    moveDir = moveDirZ;
                }
                else
                {
                    // Can't Move In Any Direction
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDistance * moveDir;
        }

        IsWalking = moveDir != Vector3.zero;

        float rotationSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotationSpeed);
    }

    private void SetSelectedCounter(BaseCounter counter)
    {
        selectedCounter = counter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs { selectedCounter = selectedCounter });
    }

    public Transform GetKitchenObjectFollowTransform() { return kitchenObjectHoldPoint; }

    public KitchenObject GetKitchenObject() { return kitchenObject; }

    public void SetKitchenObject(KitchenObject kitchenObject) 
    {
        this.kitchenObject = kitchenObject; 

        if(kitchenObject !=  null)
        {
            OnPickedupSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerPickedupSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ClearKitchenObject() { kitchenObject = null; }

    public bool HasKitchenObject() { return kitchenObject != null; }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
