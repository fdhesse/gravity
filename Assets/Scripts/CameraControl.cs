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

	public Vector3 delta = Vector3.zero;
	private Vector3 lastPos = Vector3.zero;
    void LateUpdate()
    {
		transform.LookAt(target);

		if (target && ( !target.GetComponent<Pawn>() || target.GetComponent<Pawn>().isCameraMode ))
        {
            

			if ( Input.GetMouseButtonDown(0) )
			{
				lastPos = Input.mousePosition;
			}
			else if ( Input.GetMouseButton(0) )
			{
				delta = Input.mousePosition - lastPos;
				
				// Do Stuff here
				
				Debug.Log( "delta X : " + delta.x );
				Debug.Log( "delta Y : " + delta.y );
				
				Debug.Log( "delta distance : " + delta.magnitude );

				delta.y = ClampAngle(delta.y, yMinLimit, yMaxLimit);

				x += delta.x;
				y -= delta.y;
			}

        }

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
		
		// End do stuff
		
		lastPos = Input.mousePosition;

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
