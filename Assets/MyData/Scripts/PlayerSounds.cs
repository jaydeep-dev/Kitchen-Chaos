using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private PlayerController playerController;
    private float footstepTimer;
    private float maxFootstepTimer = .1f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        footstepTimer += Time.deltaTime;
        if(footstepTimer > maxFootstepTimer)
        {
            footstepTimer = 0;
            if (playerController.IsWalking)
            {
                float volume = 1f;
                AudioManager.Instance.PlayFootstepSound(playerController.transform.position, volume);
            }
        }
    }
}
