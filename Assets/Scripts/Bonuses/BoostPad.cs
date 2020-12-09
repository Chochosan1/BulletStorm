using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    enum BoostType { BoostForward, ObjectForward }

    [Header("Properties")]
    [Tooltip("How hard to boost objects.")]
    [SerializeField] float boostForce = 20f;
    [SerializeField] float upBoostForce = 5f;
    [Tooltip("BoostForward - objects will get boosted in the pad's forward direction. ObjectForward - objects will get boosted in their local's forward direction.")]
    [SerializeField] BoostType boostType;

    private Rigidbody currentRB;


    private void OnTriggerEnter(Collider other)
    {
        currentRB = other.gameObject.GetComponent<Rigidbody>();

        if (currentRB != null)
        {
            switch (boostType)
            {
                case BoostType.BoostForward:
                    currentRB.AddForce(transform.forward * boostForce + transform.up * upBoostForce, ForceMode.Impulse);
                    break;
                case BoostType.ObjectForward:
                    currentRB.AddForce(currentRB.transform.forward * boostForce + transform.up * upBoostForce, ForceMode.Impulse);
                    break;
            }
        }
    }
}
