using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio Clip Collection")]
public class AudioClipCollection : ScriptableObject
{
    public AudioClip[] clips;
}
