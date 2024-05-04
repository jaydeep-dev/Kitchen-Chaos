using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IKitchenObjectParent
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private InputHandler inputHandler;
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


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than 1 player in the scene");
        }
        Instance = this;
    }

    private void OnEnable()
    {
        inputHandler.OnInteractAction += OnInteractAction;
        inputHandler.OnInteractAltAction += OnInteractAltAction;
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
        inputHandler.OnInteractAction -= OnInteractAction;
        inputHandler.OnInteractAltAction -= OnInteractAltAction;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();

        HandleInteraction();
    }

    private void HandleInteraction()
    {
        var moveInput = inputHandler.GetInputVector();

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
        var moveInput = inputHandler.GetInputVector();

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
        }
    }

    public void ClearKitchenObject() { kitchenObject = null; }

    public bool HasKitchenObject() { return kitchenObject != null; }
}
