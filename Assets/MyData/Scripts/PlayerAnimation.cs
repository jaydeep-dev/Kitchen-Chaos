using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    [SerializeField] private PlayerController playerController;

    private readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        animator.SetBool(IsWalkingHash, playerController.IsWalking);
    }
}
