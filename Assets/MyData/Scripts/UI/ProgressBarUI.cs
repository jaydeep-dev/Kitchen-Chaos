using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image progressBarImage;
    [SerializeField] private GameObject hasProgressGO;

    private IHasProgress hasProgress;

    private void Start()
    {
        hasProgress = hasProgressGO.GetComponent<IHasProgress>();
        if (hasProgress == null)
        {
            Debug.LogError($"IHasProgress is null! {hasProgressGO} does not implement the IHasProgress Interface!");
        }

        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;
        progressBarImage.fillAmount = 0;
        Hide();
    }

    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        progressBarImage.fillAmount = e.progressNormalized;

        if(progressBarImage.fillAmount == 0 ||  progressBarImage.fillAmount == 1)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);
}
