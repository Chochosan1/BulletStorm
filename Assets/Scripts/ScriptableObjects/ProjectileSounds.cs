using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSounds", menuName = "Chochosan/Sounds/ProjectileSoundsAsset", order = 1)]
public class ProjectileSounds : ScriptableObject
{
    public AudioClip shootProjectileSound;
    public AudioClip hitProjectileSound;
}
