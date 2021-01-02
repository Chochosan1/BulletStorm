using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRandomness : MonoBehaviour
{
    [Header("Atmosphere")]
    [SerializeField] private GameObject rainParticles;
    [SerializeField] private float rainChance;

    void Start()
    {
        if(Random.Range(0f, 1f) <= rainChance)
        {
            rainParticles.SetActive(true);
        }
    }
}
