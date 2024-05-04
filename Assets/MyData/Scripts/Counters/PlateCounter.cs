using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    [SerializeField] private float plateSpawnTime;

    private float currentPlateSpawnTime;

    private int plateCount;
    private int maxPlateCount = 5;


    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
            return;

        currentPlateSpawnTime += Time.deltaTime;

        if(currentPlateSpawnTime >= plateSpawnTime)
        {
            currentPlateSpawnTime = 0;
            if(plateCount < maxPlateCount)
            {
                plateCount++;

                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public override void Interact(PlayerController player)
    {
        if(!player.HasKitchenObject() && plateCount > 0)
        {
            plateCount--;
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);

            OnPlateRemoved?.Invoke(this, EventArgs.Empty);
        }
    }
}
