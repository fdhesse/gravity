using UnityEngine;
using System.Collections;

/// <summary>
/// Script responsible camera movement and control.
/// </summary>
public class CameraControl : MonoBehaviour
{
    public Transform target;      // the transform of the target GameObject (has position, rotation and scale values)

	public float rotationTime = 0.1f;

    public float distance = 5f; // distance to the target

	[Tooltip("The ratio that will be applied to the drag distance on the horizontal of the screen. A value of 1 will keep the original speed, a value less than 1 will slow down the camera rotation speed. A value greater than 1 will increase the rotation speed.")]
    public float xPivotingRatio = 1.0f; // x orbiting speed

	[Tooltip("The ratio that will be applied to the drag distance on the vertical of the screen. A value of 1 will keep the original speed, a value less than 1 will slow down the camera rotation speed. A value greater than 1 will increase the rotation speed.")]
	public float yPivotingRatio = 1.0f; // y orbiting speed
	
//	[EDIT]: commented 2 lines
//    private float yMinLimit = -360f;
//    private float yMaxLimit = 360f;
    public float distanceMin = 600f; //minimum distance, changeable via zoom
    public float distanceMax = 1000f; //maximum distance, changeable via zoom
	
	[HideInInspector] public float roll = 0.0f;
	[HideInInspector] public float pan = 0.0f;
	[HideInInspector] public float tilt = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        pan = angles.y;
        tilt = angles.x;
		//roll = 0.0f;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

	public Vector3 delta = Vector3.zero;
	private Vector3 lastPos = Vector3.zero;
    void LateUpdate()
    {
		transform.LookAt(target);

		if (target && ( !target.GetComponent<Pawn>() || target.GetComponent<Pawn>().isCameraMode ))
        {
			if ( InputManager.isClickDown() )
			{
				lastPos = Input.mousePosition;
			}
			else if ( InputManager.isClickHeldDown() )
			{
				// get the drag distance
				delta = Input.mousePosition - lastPos;

				// scale the drag distance with the value set in the level
				delta.x *= xPivotingRatio;
				delta.y *= yPivotingRatio;

				if ( roll >= 90 && roll < 180 )
				{
					pan -= delta.y;
					tilt -= delta.x;
				}
				else if ( roll >= 180 && roll < 270 )
				{
					pan -= delta.x;
					tilt += delta.y;
				}/*
				else if ( roll >= 270 && roll < 360 )
				{
					x += delta.y;
					y -= delta.x;
				}*/
				else
				{
					pan += delta.x;
					tilt -= delta.y;
				}

			}

        }

		Quaternion rotation;
		
		if ( roll >= 90 && roll < 180 )
			rotation = Quaternion.Euler(pan, tilt, roll);
		else
			rotation = Quaternion.Euler(tilt, pan, roll);

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
