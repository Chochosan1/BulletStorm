using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    enum BoostType { BoostForward, ObjectForward }

    [Header("Properties")]
    [SerializeField] private LayerMask acceptEntitiesFromLayers;
    [Tooltip("BoostForward - objects will get boosted in the pad's forward direction. ObjectForward - objects will get boosted in their local's forward direction.")]
    [SerializeField] BoostType boostType;


    private void OnTriggerEnter(Collider other)
    {
        if (acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)) || !other.CompareTag("Player"))
            return;

        switch (boostType)
        {
            case BoostType.BoostForward:
                other.GetComponent<PlayerController>().Dash(true, transform.forward);
                break;
            case BoostType.ObjectForward:
                other.GetComponent<PlayerController>().Dash(true, other.transform.forward);
                break;
        }
    }
}
