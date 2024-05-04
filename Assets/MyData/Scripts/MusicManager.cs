using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private const string PLAYER_PREF_MUSIC_VOLUME = "Music Volume";

    private float volume = .3f;
    private AudioSource musicSource;

    private void Awake()
    {
        Instance = this;
        musicSource = GetComponent<AudioSource>();
        volume = PlayerPrefs.GetFloat(PLAYER_PREF_MUSIC_VOLUME, .3f);
        musicSource.volume = volume;
    }

    public float GetVolume() => volume;

    public void ChangeVolume()
    {
        volume += .1f;
        if (volume > 1f)
        {
            volume = 0f;
        }

        musicSource.volume = volume;

        PlayerPrefs.SetFloat(PLAYER_PREF_MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
    }
}
