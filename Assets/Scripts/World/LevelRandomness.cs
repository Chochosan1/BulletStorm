using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRandomness : MonoBehaviour
{
    [Header("Atmosphere")]
    [SerializeField] private GameObject rainParticles;
    [SerializeField] private float rainChance;

    [Header("Random environment")]
    [SerializeField] private GameObject[] environmentElements;

    void Start()
    {
        CalculateRainChance();
        CalculateRandomEnvironment();
    }

    private void CalculateRainChance()
    {
        if (Random.Range(0f, 1f) <= rainChance)
        {
            rainParticles.SetActive(true);
        }
    }

    private void CalculateRandomEnvironment()
    {
        foreach(GameObject GO in environmentElements)
        {
            float chance = Random.Range(0f, 1f);

            if (chance < 0.5f)
                GO.SetActive(false);
        }
    }


}
