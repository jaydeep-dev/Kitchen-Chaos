using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string PLAYER_PREF_SFX_VOLUME = "SFX Volume";

    [SerializeField] private AudioClipSFXSO clipSFXSO;

    private float volume;

    private void Awake()
    {
        Instance = this;

        volume = PlayerPrefs.GetFloat(PLAYER_PREF_SFX_VOLUME, .5f);
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DelieveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DelieveryManager_OnRecipeFailed;
        PlayerController.OnAnyPlayerPickedupSomething += Player_OnPickedupSomething;
        BaseCounter.OnAnythingPlaced += BaseCounter_OnAnythingPlaced;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        var trashCounter = sender as TrashCounter;
        PlaySound(clipSFXSO.trashItemClips, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnythingPlaced(object sender, System.EventArgs e)
    {
        var counter = sender as BaseCounter;
        PlaySound(clipSFXSO.objectDropClips, counter.transform.position);
    }

    private void Player_OnPickedupSomething(object sender, System.EventArgs e)
    {
        var player = sender as PlayerController;
        PlaySound(clipSFXSO.objectPickupClips, player.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        var cuttingCounter = sender as CuttingCounter;
        PlaySound(clipSFXSO.chopClips, cuttingCounter.transform.position);
    }

    private void DelieveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        PlaySound(clipSFXSO.delieveryFailedClips, Camera.main.transform.position);
    }

    private void DelieveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        PlaySound(clipSFXSO.delieverySuccessClips, Camera.main.transform.position);
    }

    public void PlayFootstepSound(Vector3 pos, float volume)
    {
        PlaySound(clipSFXSO.footstepClips, pos, volume);
    }

    private void PlaySound(List<AudioClip> clips, Vector3 pos, float volumeMultiplier = 1f)
    {
        var clip = clips[Random.Range(0, clips.Count)];
        AudioSource.PlayClipAtPoint(clip, pos, volumeMultiplier * volume);
    }

    public void PlayWarningSound(Vector3 position)
    {
        PlaySound(clipSFXSO.warningClips, position);   
    }

    public float GetVolume() => volume;

    public void ChangeVolume()
    {
        volume += .1f;
        if(volume > 1f)
        {
            volume = 0f;
        }

        PlayerPrefs.SetFloat(PLAYER_PREF_SFX_VOLUME, volume);
        PlayerPrefs.Save();
    }
}
