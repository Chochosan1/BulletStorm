using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chochosan
{
    public class RTS_CameraController : MonoBehaviour
    {
        //cached transform to avoid excessive calls
        private Transform thisTransform;

        //the new pos that will be used to move the camera there
        private Vector3 pos;

        [Header("Camera properties")]
        [SerializeField] Transform initialCameraSpawnPoint;
        [Tooltip("How fast should the camera move?")]
        [SerializeField] private float panSpeed = 0.5f;
        [Tooltip("How fast should the camera zoom in/out?")]
        [SerializeField] private float scrollSpeed = 10f;
        [Tooltip("How smoothed should the movement/zooming be?")]
        [SerializeField] private float smoothSpeed = 5f;

        [Tooltip("Offset from the screen border.")]
        public float panBorderThickness = 7.5f; //offset from the screen border

        [Header("Camera position limits")]
        [Tooltip("Maps the X and Z camera position limits. Y corresponds to Z in that case.")]
        [SerializeField] private Vector2 panLimit;
        [SerializeField] private float minY, maxY;

        private static bool cameraZoomingAllowed = true;

        private void Start()
        {
            thisTransform = gameObject.GetComponent<Transform>();
            pos = thisTransform.position;
            thisTransform.position = new Vector3(thisTransform.position.x, thisTransform.position.y * 2f, this.transform.position.z);
            Vector3 initialSpawnPos = new Vector3(initialCameraSpawnPoint.position.x, thisTransform.position.y, initialCameraSpawnPoint.position.z);
        }

        private void Update()
        {
            if (Input.mousePosition.y >= Screen.height - panBorderThickness) //top border
            {
                pos.z += panSpeed;
            }
            else if (Input.mousePosition.y <= panBorderThickness) //bottom border
            {
                pos.z -= panSpeed;
            }

            if (Input.mousePosition.x >= Screen.width - panBorderThickness) //right border
            {
                pos.x += panSpeed;
            }
            else if (Input.mousePosition.x <= panBorderThickness) //left border
            {
                pos.x -= panSpeed;
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0 && cameraZoomingAllowed)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                pos.y -= scroll * scrollSpeed; //subtract from Y so that the scrolling is not reversed
            }
            //boundaries; the position can't go beyond the min/max values
            //  pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            //  pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

            thisTransform.position = Vector3.Lerp(thisTransform.position, pos, smoothSpeed * Time.deltaTime);
        }

        public static void ToggleCameraZooming()
        {
            cameraZoomingAllowed = !cameraZoomingAllowed;
        }
    }
}
