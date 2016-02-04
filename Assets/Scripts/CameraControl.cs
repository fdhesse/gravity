using UnityEngine;
using System.Collections;

/// <summary>
/// Script responsible camera movement and control.
/// </summary>
public class CameraControl : MonoBehaviour
{
	[Tooltip("A game object in the scene that will be used as a target point for the camera to look at.")]
    public Transform target;      // the transform of the target GameObject (has position, rotation and scale values)

	[Tooltip("The starting distance to the target game object, when the level starts.")]
    public float distance = 5f; // distance to the target

	[Tooltip("The minimum distance to the target game object. The camera won't go closer than that if you zoom in.")]
	public float distanceMin = 600f; //minimum distance, changeable via zoom

	[Tooltip("The maximum distance to the target game object. The camera won't go farer than that if you zoom out.")]
	public float distanceMax = 1000f; //maximum distance, changeable via zoom

	[Tooltip("The ratio that will be applied to the drag distance on the horizontal of the screen. A value of 1 will keep the original speed, a value less than 1 will slow down the camera rotation speed. A value greater than 1 will increase the rotation speed.")]
    public float xPivotingRatio = 1.0f; // x orbiting speed

	[Tooltip("The ratio that will be applied to the drag distance on the vertical of the screen. A value of 1 will keep the original speed, a value less than 1 will slow down the camera rotation speed. A value greater than 1 will increase the rotation speed.")]
	public float yPivotingRatio = 1.0f; // y orbiting speed

	[HideInInspector] public float roll = 0.0f;
	[HideInInspector] public float pan = 0.0f;
	[HideInInspector] public float tilt = 0.0f;

	private Vector3 lastMousePosition = Vector3.zero;

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

    void LateUpdate()
    {
		Vector3 delta = Vector3.zero;

		transform.LookAt(target);

		// check that the target is not the pawn, because the pawn can move, or if the target
		// is the pawn, then the pawn should be in camera mode (so probably won't move)
		if (target && ( !target.GetComponent<Pawn>() || target.GetComponent<Pawn>().isCameraMode ))
        {
			if ( InputManager.isClickDown() )
			{
				lastMousePosition = Input.mousePosition;
			}
			else if ( InputManager.isClickHeldDown() )
			{
				// get the drag distance
				delta = Input.mousePosition - lastMousePosition;

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

		distance = Mathf.Clamp(distance + InputManager.getZoomDistance(), distanceMin, distanceMax);

		Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
		Vector3 position = rotation * negDistance + target.position;
		
		transform.rotation = rotation;
		transform.position = position;
		
		// save the last mouve position		
		lastMousePosition = Input.mousePosition;
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
