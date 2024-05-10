using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[CreateAssetMenu()]
public class PlayerColorSO : ScriptableObject
{
    [ColorUsage(true, true)]
    public List<Color> playerColors;
}
