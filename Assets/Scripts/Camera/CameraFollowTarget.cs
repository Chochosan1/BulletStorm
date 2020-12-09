using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    [Header("References")]
    [Space]
    [SerializeField] private Transform targetToFollow;

    [Header("Properties")]
    [Space]
    [SerializeField] private float cameraFollowSpeed = 10f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float minY, maxY;
    [Tooltip("How fast should the camera zoom in/out?")]
    [SerializeField] private float scrollSpeed = 10f;
    private Transform thisTransform;
    private Vector3 currentVelocity;
    public enum CameraUpdate { LateUpdate, Update, FixedUpdate };
    public CameraUpdate cameraUpdate;
    public bool isLerp = false;

    void Start()
    {
        thisTransform = transform;
    }

    void LateUpdate()
    {
        if (cameraUpdate == CameraUpdate.LateUpdate)
        {
            if(isLerp)
            {
                thisTransform.position = Vector3.Lerp(thisTransform.position, targetToFollow.position + offset, Time.deltaTime * cameraFollowSpeed);
            }
            else
            {
                thisTransform.position = Vector3.SmoothDamp(thisTransform.position, targetToFollow.position + offset, ref currentVelocity, cameraFollowSpeed * Time.deltaTime);
            }
        }
                   
    }

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            offset.y -= scroll * scrollSpeed; //subtract from Y so that the scrolling is not reversed
        }
        offset.y = Mathf.Clamp(offset.y, minY, maxY);


        if (cameraUpdate == CameraUpdate.Update)
        {
            if (isLerp)
            {
                thisTransform.position = Vector3.Lerp(thisTransform.position, targetToFollow.position + offset, Time.deltaTime * cameraFollowSpeed);
            }
            else
            {
                thisTransform.position = Vector3.SmoothDamp(thisTransform.position, targetToFollow.position + offset, ref currentVelocity, cameraFollowSpeed * Time.deltaTime);
            }
        }
    }

    private void FixedUpdate()
    {
        if(cameraUpdate == CameraUpdate.FixedUpdate)
        {
            if (isLerp)
            {
                thisTransform.position = Vector3.Lerp(thisTransform.position, targetToFollow.position + offset, Time.deltaTime * cameraFollowSpeed);
            }
            else
            {
                thisTransform.position = Vector3.SmoothDamp(thisTransform.position, targetToFollow.position + offset, ref currentVelocity, cameraFollowSpeed * Time.deltaTime);
            }
        }       
    }
}
