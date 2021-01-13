using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public static LevelEnd Instance;

    [SerializeReference] private GameObject levelEndPortal;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void Start()
    {
        levelEndPortal.SetActive(false);
    }
  
    public void ActivateLevelEndPortal()
    {
        levelEndPortal.SetActive(true);
    }
}
