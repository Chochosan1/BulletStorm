using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    public static CameraFollowTarget Instance;


    [Header("References")]
    [Space]
    [SerializeField] private Transform targetToFollow;

    [Header("Properties")]
    [Space]
    [SerializeField] private float cameraFollowSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 14, -6);
    [SerializeField] private float minY = 7f, maxY = 14f;
    [Tooltip("How fast should the camera zoom in/out?")]
    [SerializeField] private float scrollSpeed = 10f;
    private Transform thisTransform;
    private Vector3 currentVelocity;
    public enum CameraUpdate { LateUpdate, Update, FixedUpdate };
    public CameraUpdate cameraUpdate = CameraUpdate.FixedUpdate;
    public bool isLerp = false;
    private bool isCameraShaking = false;
    private Transform camTransform;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        thisTransform = transform;
        camTransform = GetComponent<Camera>().transform;
    }

    void LateUpdate()
    {
        if (cameraUpdate == CameraUpdate.LateUpdate && !isCameraShaking)
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


        if (cameraUpdate == CameraUpdate.Update && !isCameraShaking)
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
        if(cameraUpdate == CameraUpdate.FixedUpdate && !isCameraShaking)
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

    public void ShakeCamera(float duration, float magnitude)
    {
        if(!isCameraShaking)
        {
            StartCoroutine(CameraShake(duration, magnitude));
        }
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        isCameraShaking = true;
        Vector3 originalPos = camTransform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(camTransform.position.x - magnitude, camTransform.position.x + magnitude);
            float y = Random.Range(camTransform.position.y - magnitude, camTransform.position.y + magnitude);

            camTransform.position = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        camTransform.position = originalPos;

        isCameraShaking = false;
    }
}
