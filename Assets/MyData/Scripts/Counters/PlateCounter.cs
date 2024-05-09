using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
        if (!IsServer)
        {
            return;
        }

        if (!GameManager.Instance.IsGamePlaying())
            return;

        currentPlateSpawnTime += Time.deltaTime;

        if(currentPlateSpawnTime >= plateSpawnTime)
        {
            currentPlateSpawnTime = 0;
            if(plateCount < maxPlateCount)
            {
                SpawnPlateServerRpc();
            }
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        plateCount++;

        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact(PlayerController player)
    {
        if(!player.HasKitchenObject() && plateCount > 0)
        {
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            InteractLogicServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        plateCount--;

        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
