using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioClipSFXSO : ScriptableObject
{
    public List<AudioClip> chopClips;
    public List<AudioClip> delieverySuccessClips;
    public List<AudioClip> delieveryFailedClips;
    public List<AudioClip> footstepClips;
    public List<AudioClip> objectDropClips;
    public List<AudioClip> objectPickupClips;
    public List<AudioClip> stoveSizzleClips;
    public List<AudioClip> trashItemClips;
    public List<AudioClip> warningClips;
}
