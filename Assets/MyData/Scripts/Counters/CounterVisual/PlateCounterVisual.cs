using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCounterVisual : MonoBehaviour
{
    [SerializeField] private PlateCounter plateCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;

    private List<GameObject> plateVisualGOList = new();

    private void Start()
    {
        plateCounter.OnPlateSpawned += PlateCounter_OnPlateSpawned;
        plateCounter.OnPlateRemoved += PlateCounter_OnPlateRemoved;
    }

    private void PlateCounter_OnPlateRemoved(object sender, System.EventArgs e)
    {
        var plateGO = plateVisualGOList[^1];
        plateVisualGOList.Remove(plateGO);
        Destroy(plateGO);
    }

    private void PlateCounter_OnPlateSpawned(object sender, System.EventArgs e)
    {
        var plate = Instantiate(plateVisualPrefab, counterTopPoint);

        float plateOffsetY = .1f;
        plate.localPosition = new Vector3(0, plateOffsetY * plateVisualGOList.Count, 0);

        plateVisualGOList.Add(plate.gameObject);
    }
}
