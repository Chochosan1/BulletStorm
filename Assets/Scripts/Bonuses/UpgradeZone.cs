using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeZone : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private LayerMask acceptEntitiesFromLayers;

    private void OnTriggerEnter(Collider other)
    {
        if (acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
            return;

        if (other.CompareTag("Player"))
        {
            Chochosan.CustomEventManager.OnUpgradePanelRequired?.Invoke();
            Destroy(gameObject);
        } 
    }
}
