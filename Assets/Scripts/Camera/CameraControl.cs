using UnityEngine;
using System.Collections;

/// <summary>
/// Script responsible camera movement and control.
/// </summary>
public class CameraControl : MonoBehaviour
{
	[Header("-- Target and Distance --")]
	[Tooltip("A game object in the scene that will be used as a target point for the camera to look at.")]
    public CameraTarget target;

	[Tooltip("The starting distance to the target game object, when the level starts.")]
    public float distance = 5f;

	[Tooltip("The minimum distance to the target game object. The camera won't go closer than that if you zoom in.")]
	public float distanceMin = 600f;

	[Tooltip("The maximum distance to the target game object. The camera won't go farer than that if you zoom out.")]
	public float distanceMax = 1000f;

	[Header("-- Speed --")]
	[Tooltip("The ratio that will be applied to the drag distance on the horizontal of the screen. A value of 1 will keep the original speed, a value less than 1 will slow down the camera rotation speed. A value greater than 1 will increase the rotation speed.")]
    public float xPivotingRatio = 1.0f;

	[Tooltip("The ratio that will be applied to the drag distance on the vertical of the screen. A value of 1 will keep the original speed, a value less than 1 will slow down the camera rotation speed. A value greater than 1 will increase the rotation speed.")]
	public float yPivotingRatio = 1.0f;

	[Header("-- Snapping --")]
	[Tooltip("If true, the camera will snap to the world axis when it is closed to them.")]
	public bool snapToAxis = true;

	[Tooltip("If the Snap To Axis is true, the camera will snap if its angle is less than this specified value.")]
	public float snapAngleInDegree = 10f;

	[Tooltip("If the Snap To Axis is true, the time that the camera will take to snap.")]
	public float snapTimeInSecond = 0.3f;

	private float pan = 0.0f;
	private float tilt = 0.0f;

	private float tiltSnapVelocity = 0f;
	private float panSnapVelocity = 0f;

	private Vector3 lastMousePosition = Vector3.zero;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        pan = angles.y;
        tilt = angles.x;

		// init the camera based on it's constraint (even if the player doesn't rotate the camera at first)
		if (target != null)
			target.clampAngle(ref tilt, ref pan);

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    void LateUpdate()
    {
		if (target != null)
        {
			if (target.angleContraintType != CameraTarget.AngleConstraint.FREEZE)
			{
				if ( InputManager.isClickDown() )
				{
					lastMousePosition = Input.mousePosition;
				}
				else if ( InputManager.isClickHeldDown() )
				{
					// get the drag distance
					Vector3 delta = Input.mousePosition - lastMousePosition;

					// scale the drag distance with the value set in the level
					pan += delta.x * xPivotingRatio;
					tilt += -delta.y * yPivotingRatio;

					// ask the target to limit the angle if necessary
					target.clampAngle(ref tilt, ref pan);
				}
				else if (snapToAxis)
				{
					// we do not try to snap when the user move the cam, only when he release the cam
					snapAngleToAxis(ref tilt, ref pan);
				}
			}

			Quaternion rotation = Quaternion.Euler(tilt, pan, 0f);

			distance = Mathf.Clamp(distance + InputManager.getZoomDistance(), distanceMin, distanceMax);

			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.transform.position;
			
			transform.rotation = rotation;
			transform.position = position;
			
			// save the last mouve position		
			lastMousePosition = Input.mousePosition;
		}
		else if (InputManager.isClickDown())
		{
			Debug.LogError("This camera doesn't have a target. If you want to rotate it, add a worldtarget object in the scene and assign it in the Target property of the Camera Control script.");
		}
	}

	private void snapAngleToAxis(ref float tilt, ref float pan)
	{
		// first normalize the two angles
		tilt = normalizeAngle(tilt);
		pan = normalizeAngle(pan);

		// then search if any angle is near a cardinal point
		float[] targetAngles = { 0f, 90f, 180f, 270f, 360f };
		float targetTilt = -1f;
		float targetPan = -1f;

		foreach (float target in targetAngles)
		{
			if ((tilt > target - snapAngleInDegree) && (tilt < target + snapAngleInDegree))
				targetTilt = target;

			if ((pan > target - snapAngleInDegree) && (pan < target + snapAngleInDegree))
				targetPan = target;
		}

		// check if both target was found, that means we are on a cardinal point
		if ((targetTilt != -1f) && (targetPan != -1f))
		{
			tilt = Mathf.SmoothDampAngle(tilt, targetTilt, ref tiltSnapVelocity, snapTimeInSecond);
			pan = Mathf.SmoothDampAngle(pan, targetPan, ref panSnapVelocity, snapTimeInSecond);
		}
	}
	
	private float normalizeAngle(float angle)
	{
		while (angle > 360f)
			angle -= 360f;
		while (angle < 0f)
			angle += 360f;
		return angle;
	}

	public void SetCameraCursor()
	{
		// Only show the camera cursor if the camera is not frozen
		if ((target != null) && (target.angleContraintType != CameraTarget.AngleConstraint.FREEZE))
		{
			Texture2D tex = (Texture2D)Resources.Load("HUD/cameraCursor", typeof(Texture2D));
			Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
		}
	}
	
	public void SetNormalCursor()
	{
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}
}
