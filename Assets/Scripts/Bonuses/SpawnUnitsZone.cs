using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnUnitsZone : MonoBehaviour
{
    [SerializeField] private LayerMask affectableLayers;

    [Header("References")]
    [SerializeField] private GameObject objectHolder;
    [SerializeField] private GameObject preSpawnEffect;

    [Header("Spawn settings")]
    [SerializeField] private GameObject[] spawnPoints;
    [SerializeField] private GameObject[] unitsToSpawn;
    [SerializeField] private float spawnUnitsAfter = 2f;

   
    void Start()
    {
        objectHolder.SetActive(false);
        preSpawnEffect.SetActive(true);
    }

    private void ActivateObjectHolder()
    {
        objectHolder.SetActive(true);
        preSpawnEffect.SetActive(false);

        StartCoroutine(SpawnUnitsAfter(spawnUnitsAfter));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (affectableLayers == (affectableLayers | (1 << other.gameObject.layer)))
        {
            ActivateObjectHolder();
        }
    }

    private IEnumerator SpawnUnitsAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        foreach (GameObject GO in spawnPoints)
        {
            int unitToSpawn = Random.Range(0, unitsToSpawn.Length);
            Instantiate(unitsToSpawn[unitToSpawn], GO.transform.position, unitsToSpawn[unitToSpawn].transform.rotation);

            yield return new WaitForSeconds(0.25f);
        }

        GetComponent<Collider>().enabled = false;
        Destroy(this);
    }
}
