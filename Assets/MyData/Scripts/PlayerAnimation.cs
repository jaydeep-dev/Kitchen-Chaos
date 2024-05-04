using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
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
        animator.SetBool(IsWalkingHash, playerController.IsWalking);
    }
}
