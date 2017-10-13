using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

/// <summary>
/// Script responsible camera movement and control.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
	public enum DeformationMode
	{
		NONE,
		TINY_FOV,
		ORTHOGRAPHIC,
	}

	[Header("-- Target and Distance --")]
	[Tooltip("A game object in the scene that will be used as a target point for the camera to look at.")]
    public CameraTarget target;

	[Tooltip("The starting distance to the target game object, when the level starts.")]
    public float distance = 100f;
	[ReadOnly]
	public float distanceWithZoom = 0f;

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

	[Header("-- Deformation after Snapping --")]
	[Tooltip("The type of camera deformation that will be apply to the camera after the snapping is complete.")]
	public DeformationMode deformationAfterSnap = DeformationMode.NONE;

	[Tooltip("The time the camera will use to fully complete the deformation.")]
	[FormerlySerializedAs("switchToOrthoTimeInSecond")]
	public float deformationTimeInSecond = 0.3f;

	[Tooltip("If you choose the FOV deformation, the new FOV to use while in deformation mode.")]
	public float deformationFOV = 10f;

	[Tooltip("The time durarion in second of touching the screen without moving, after which we show the camera icon.")]
	[SerializeField]
	private float m_HoldDownDurationForShowingCameraCursor = 0.25f;

	//[Tooltip("If you choose the FOV deformation, the distance to use while in deformation mode.")]
	private float deformationDistance = 800f;

	private float pan = 0.0f;
	private float tilt = 0.0f;

	private float tiltSnapVelocity = 0f;
	private float panSnapVelocity = 0f;
	private Matrix4x4 projectionMatrixVelocity = Matrix4x4.zero;
	private float deformationFovVelocity = 0f;

	private float undeformedFOV = 0f;
	private float undeformedDistance = 0f;

	private float playerAdjustedDistance = 0f;

	private Vector3 lastMousePosition = Vector3.zero;
	private float mouseDownTimeDuration = 0f;
	private Camera mCameraComponent = null;

	private bool m_HasCameraCapturedInput = false;
	public bool HasCameraCapturedInput
	{
		get { return m_HasCameraCapturedInput; }
	}

	void Start()
    {
		mCameraComponent = GetComponent<Camera>();

		undeformedFOV = mCameraComponent.fieldOfView;
		undeformedDistance = this.distance;

		deformationDistance = this.distance * mCameraComponent.fieldOfView / deformationFOV;

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
				// use a flag to tell if the input control the camera during this update
				// (input are captured if you hold touch down without moving for a certain duration, or if you hold and drag the touch)
				bool hasCameraCapturedInputThisFrame = false;

				if (InputManager.isClickDown())
				{
					lastMousePosition = Input.mousePosition;
					mouseDownTimeDuration = 0f;
					projectionMatrixVelocity = Matrix4x4.zero;
				}
				else if (InputManager.isClickHeldDown())
				{
					// for very low framerate, we give at least 3 frames to switch to camera mode,
					// otherwise for normal framerate, we use a fixed value of 1 quarter of second.
					float durationToShowCameraCursor = Mathf.Max(m_HoldDownDurationForShowingCameraCursor, 3.0f * Time.deltaTime);

					// increment the down time if we hold the mouse down, and show the cursor if we hold it since enough time
					mouseDownTimeDuration += Time.deltaTime;
					hasCameraCapturedInputThisFrame = (mouseDownTimeDuration > durationToShowCameraCursor);

					// now check if we moved
					if (InputManager.hasClickDownMoved())
					{
						// if we start to move, show the cursor immediately without waiting
						hasCameraCapturedInputThisFrame = true;
						mouseDownTimeDuration += durationToShowCameraCursor * 2f;

						// remove the deformation
						if (deformationAfterSnap != DeformationMode.NONE)
							unapplyCameraDeformation();

						// get the drag distance
						Vector3 delta = Input.mousePosition - lastMousePosition;

						// scale the drag distance with the value set in the level
						pan += delta.x * xPivotingRatio;
						tilt += -delta.y * yPivotingRatio;

						// ask the target to limit the angle if necessary
						target.clampAngle(ref tilt, ref pan);
					}
				}
				else if (snapToAxis)
				{
					// we do not try to snap when the user move the cam, only when he release the cam
					snapAngleToAxis(ref tilt, ref pan);
				}

				// set the correct mouse cursor depending on the capture input status this frame
				UpdateCameraCursor(hasCameraCapturedInputThisFrame);
			}

			Quaternion rotation = Quaternion.Euler(tilt, pan, 0f);

			playerAdjustedDistance = Mathf.Clamp(playerAdjustedDistance + InputManager.getZoomDistance(), distanceMin - this.undeformedDistance, distanceMax - this.undeformedDistance);
			distanceWithZoom = getCurrentDistanceToTarget();
				
			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distanceWithZoom);
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

	private float getCurrentDistanceToTarget()
	{
		return (this.distance + playerAdjustedDistance);
	}

	private void snapAngleToAxis(ref float tilt, ref float pan)
	{
		// first normalize the two angles
		tilt = normalizeAngle(tilt);
		pan = normalizeAngle(pan);

		// then search if any angle is near a cardinal point
		float[] targetAngles = { -360f, -270f, -180f, -90f, 0f, 90f, 180f, 270f, 360f };
		// the flag to tell if we found a target, and the target values
		bool shouldSmoothTilt = false;
		bool shouldSmoothPan = false;
		float targetTilt = -1f;
		float targetPan = -1f;

		foreach (float target in targetAngles)
		{
			if ((tilt > target - snapAngleInDegree) && (tilt < target + snapAngleInDegree))
			{
				shouldSmoothTilt = true;
				targetTilt = target;
			}

			if ((pan > target - snapAngleInDegree) && (pan < target + snapAngleInDegree))
			{
				shouldSmoothPan = true;
				targetPan = target;
			}
		}

		// check if both target was found, that means we are on a cardinal point
		// or if we need to smooth the tilt vertically upward or downward, then we don't care about the pan
		if (shouldSmoothTilt && (shouldSmoothPan || (targetTilt == 90f) || (targetTilt == -90f) || (targetTilt == 270f) || (targetTilt == -270f)))
		{
			// smooth the tilt
			tilt = Mathf.SmoothDampAngle(tilt, targetTilt, ref tiltSnapVelocity, snapTimeInSecond);
			// smooth the pan if needed
			if (shouldSmoothPan)
				pan = Mathf.SmoothDampAngle(pan, targetPan, ref panSnapVelocity, snapTimeInSecond);

			// if the targets have been reached, that means the snapping is finished, and we can change the cam state
			if ((deformationAfterSnap != DeformationMode.NONE) && 
				(Mathf.Abs(tilt - targetTilt) < 0.5f) && (!shouldSmoothPan || Mathf.Abs(pan - targetPan) < 0.5f))
				applyCameraDeformation();
		}
		else
		{
			unapplyCameraDeformation();
		}
	}
	
	private float normalizeAngle(float angle)
	{
		while (angle > 360f)
			angle -= 360f;
		while (angle < -360f)
			angle += 360f;
		return angle;
	}

	private void lerpFOVandDistance(float startFOV, float targetFOV, float startDistance)
	{
		// lerp the fov
		mCameraComponent.fieldOfView = Mathf.SmoothDamp(mCameraComponent.fieldOfView, targetFOV, ref deformationFovVelocity, deformationTimeInSecond);
		// compute the distance according to the current FOV
		this.distance = startDistance * startFOV / mCameraComponent.fieldOfView;
	}

	private void lerpCameraProjectionMatrix(Matrix4x4 targetMatrix)
	{
		// create a new matrix for computing the result
		Matrix4x4 lerpCameraMatrix = Matrix4x4.identity;

		// smooth damp all the values of the matrix
		for (int i = 0; i < 4; ++i)
			for (int j = 0; j < 4; ++j)
			{
				float velocity = projectionMatrixVelocity[i,j];
				lerpCameraMatrix[i,j] = Mathf.SmoothDamp(mCameraComponent.projectionMatrix[i,j], targetMatrix[i,j],
					ref velocity, deformationTimeInSecond);
				projectionMatrixVelocity[i,j] = velocity;
			}
		
		// set the result matrix
		mCameraComponent.projectionMatrix = lerpCameraMatrix;
	}

	private void applyCameraDeformation()
	{
		switch (deformationAfterSnap)
		{
		case DeformationMode.TINY_FOV:
			lerpFOVandDistance(this.undeformedFOV, this.deformationFOV, this.undeformedDistance);
			break;

		case DeformationMode.ORTHOGRAPHIC:
			float orthoSize = getCurrentDistanceToTarget() * Mathf.Tan(mCameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad);
			float halfWidth = orthoSize * mCameraComponent.aspect;
			lerpCameraProjectionMatrix( Matrix4x4.Ortho(-halfWidth, halfWidth, -orthoSize, orthoSize,
				mCameraComponent.nearClipPlane, mCameraComponent.farClipPlane) );
			break;
		}
	}

	private void unapplyCameraDeformation()
	{
		switch (deformationAfterSnap)
		{
		case DeformationMode.TINY_FOV:
			lerpFOVandDistance(this.deformationFOV, this.undeformedFOV, this.deformationDistance);
			break;

		case DeformationMode.ORTHOGRAPHIC:
			lerpCameraProjectionMatrix( Matrix4x4.Perspective(mCameraComponent.fieldOfView,
				mCameraComponent.aspect, mCameraComponent.nearClipPlane, mCameraComponent.farClipPlane) );
			break;
		}
	}

	private void UpdateCameraCursor(bool isInputCaptured)
	{
		// change the state if needed
		if (isInputCaptured)
		{
			if (!m_HasCameraCapturedInput)
			{
				Texture2D tex = (Texture2D)Resources.Load("HUD/cameraCursor", typeof(Texture2D));
				Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
			}
		}
		else
		{
			if (m_HasCameraCapturedInput)
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}

		// save the new state
		m_HasCameraCapturedInput = isInputCaptured;
	}
}
