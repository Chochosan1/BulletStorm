using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundMechanic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Properties")]
    [SerializeField] private float radius = 2.0f;
    [SerializeField] private float radiusSpeed = 0.5f;
    [SerializeField] private float rotationSpeed = 80.0f;
    private Vector3 axis = Vector3.up;
    private Transform thisTransform;

    private void Start()
    {
        thisTransform = transform;
        thisTransform.position = (thisTransform.position - target.position).normalized * radius + target.position;
    }
 
    void Update()
    {
        thisTransform.RotateAround(target.position, axis, rotationSpeed * Time.deltaTime);
        var desiredPosition = (thisTransform.position - target.position).normalized * radius + target.position;
        thisTransform.position = Vector3.MoveTowards(thisTransform.position, desiredPosition, Time.deltaTime * radiusSpeed);
    }
}
