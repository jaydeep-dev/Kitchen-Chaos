using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnTryingToJoin += KitchenGameMultiplayer_OnTryingToJoin;
        KitchenGameMultiplayer.Instance.OnFailedToJoin += KitchenGameMultiplayer_OnFailedToJoin;
        Hide();
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnTryingToJoin -= KitchenGameMultiplayer_OnTryingToJoin;
        KitchenGameMultiplayer.Instance.OnFailedToJoin -= KitchenGameMultiplayer_OnFailedToJoin;
    }

    private void KitchenGameMultiplayer_OnFailedToJoin(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void KitchenGameMultiplayer_OnTryingToJoin(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
