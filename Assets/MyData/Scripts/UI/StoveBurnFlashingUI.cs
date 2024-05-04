using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurnFlashingUI : MonoBehaviour
{
    private readonly int IsFlashingHash = Animator.StringToHash("IsFlashing");

    [SerializeField] private StoveCounter stoveCounter;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        bool show = stoveCounter.IsFried() && e.progressNormalized > burnShowProgressAmount;
        animator.SetBool(IsFlashingHash, show);
    }
}