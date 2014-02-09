using UnityEngine;
using System.Collections;

/// <summary>
/// Script responsible camera movement and control.
/// </summary>
public class CameraControl : MonoBehaviour
{
    public Transform target;      // the transform of the target GameObject (has position, rotation and scale values)

    public float distance = 50f; // distance to the target
    public float xPivotingSpeed = 120.0f; // x orbiting speed
    public float yPivotingSpeed = 120.0f; // y orbiting speed

    private float yMinLimit = -360f;
    private float yMaxLimit = 360f;
    private float distanceMin = 50f; //minimum distance, changeable via zoom
    private float distanceMax = 200f; //maximum distance, changeable via zoom

    float x = 0.0f;
    float y = 0.0f;
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Make the rigid body not change rotation
        if (rigidbody)
            rigidbody.freezeRotation = true;
    }

    void LateUpdate()
    {
        

        if (target)
        {
            transform.LookAt(target);
            if (Input.GetMouseButton(0))
            {
                x += Input.GetAxis("Mouse X") * xPivotingSpeed * distance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * yPivotingSpeed * distance * 0.02f;
            }

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 50, distanceMin, distanceMax);

            //camera collisions
            //RaycastHit hit;
            //if (Physics.Linecast(target.position, transform.position, out hit))
            //{
            //    distance -= hit.distance;
            //}
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;

        }

    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

}
