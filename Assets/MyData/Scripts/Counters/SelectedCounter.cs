using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounter : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] selectedVisualGO;

    private void Start()
    {
        PlayerController.Instance.OnSelectedCounterChanged += OnSelectedCounterChanged;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnSelectedCounterChanged -= OnSelectedCounterChanged;
    }

    private void OnSelectedCounterChanged(object sender, PlayerController.OnSelectedCounterChangedEventArgs e)
    {
        if(e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach(GameObject go in selectedVisualGO)
        {
            go.SetActive(true);
        }
    }

    private void Hide()
    {
        foreach (GameObject go in selectedVisualGO)
        {
            go.SetActive(false);
        }
    }
}
