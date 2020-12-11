using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    //debug
    public bool isSpawning = true;

    [Header("Prefabs")]
    [SerializeField] private GameObject aiToSpawnPrefab;

    [Header("Properties")]
    [SerializeField] private float spawnCooldownMin;
    [SerializeField] private float spawnCooldownMax;

    private float currentSpawnCooldown;

    private float spawnTimestamp;


    private void Update()
    {
        if (!isSpawning)
            return;

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (Time.time >= spawnTimestamp)
        {
            GameObject tempEnemy = Instantiate(aiToSpawnPrefab, transform.position, aiToSpawnPrefab.transform.rotation);

            currentSpawnCooldown = Random.Range(spawnCooldownMin, spawnCooldownMax);
            spawnTimestamp = Time.time + currentSpawnCooldown;
        }
    }
}
