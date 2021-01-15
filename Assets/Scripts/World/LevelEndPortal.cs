using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndPortal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            int randomLevel = Random.Range(0, 2);
            SceneManager.LoadScene(randomLevel);
        }
    }
}
