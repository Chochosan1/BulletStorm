using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleEffectsContainer", menuName = "Chochosan/Particles/ParticlesContainerAsset", order = 1)]
public class ParticleEffectsContainer : ScriptableObject
{
    public GameObject deathParticle;
    public GameObject frozenParticle;
    public GameObject explodedParticle;
}
