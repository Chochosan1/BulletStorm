using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attached to a start cell. Will find the player and move him to the cell's spawn point (will not instantiate, just move).
/// </summary>
public class StartCell : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;

    void Start()
    {
        FindObjectOfType<PlayerController>().gameObject.transform.position = playerSpawnPoint.transform.position;

        Destroy(this);
    }
}
