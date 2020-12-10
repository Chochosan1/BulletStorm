using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundMechanic : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float speed;
    public float angle;
    public Vector3 rotationAxis;
    Vector3 direction;

    public Transform center;
    public Vector3 axis = Vector3.up;
 public float radius = 2.0f;
    public float radiusSpeed = 0.5f;
    public float rotationSpeed = 80.0f;

    private void Start()
    {
        direction = transform.position - target.position;

        transform.position = (transform.position - target.position).normalized * radius + target.position;
    }
    // Update is called once per frame
    void Update()
    {
        //transform.RotateAround(target.position + offset, Vector3.up, speed * Time.deltaTime);
        //var dir = transform.position - target.position;
        //dir = Vector3.ClampMagnitude(dir, 7);
        //transform.position = target.position + dir;
        //Quaternion rot = Quaternion.AngleAxis(angle, rotationAxis);
        //transform.position = target.position + rot * direction;
        //transform.localRotation = rot;
        //if ((target.position - transform.position).magnitude >= 10f)
        //{
        //    transform.position = target.position;
        //}
        //else
        //{
        //    transform.RotateAround(target.position + offset, Vector3.up, speed * Time.deltaTime);
        //}

        transform.RotateAround(target.position, axis, rotationSpeed * Time.deltaTime);
        var desiredPosition = (transform.position - target.position).normalized * radius + target.position;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * radiusSpeed);
    }
}
