using UnityEngine;
using System.Collections;

public class CameraTarget : MonoBehaviour
{
	public enum AngleConstraint
	{
		NONE = 0,
		CONE,
		CYLINDER,
	}

	public AngleConstraint angleContraintType = AngleConstraint.NONE;

	[Range(0,90)]
	public float panLimitAngle = 90f;

	[Range(0,90)]
	public float tiltLimitAngle = 60f;
	
	public void clampAngle(ref float tilt, ref float pan)
	{
		bool limitPan = false;
		bool limitTilt = false;

		switch (angleContraintType)
		{
		case AngleConstraint.NONE:
			// nothing to do
			return;
		case AngleConstraint.CONE:
			limitPan = true;
			limitTilt = true;
			break;
		case AngleConstraint.CYLINDER:
			limitTilt = true;
			break;
		}

		// now limit the angle if necessary
		Vector3 angles = this.transform.rotation.eulerAngles;
		if (angles.x > 180f)
			angles.x -= 360f;
		if (angles.y > 180f)
			angles.y -= 360f;

		Vector3 worldMinAngles = angles + new Vector3(-tiltLimitAngle, -panLimitAngle, 0f);
		Vector3 worldMaxAngles = angles + new Vector3(tiltLimitAngle, panLimitAngle, 0f);

		// now limit the angle if necessary in local coordinate
		if (limitTilt)
			tilt = Mathf.Clamp(tilt, worldMinAngles.x, worldMaxAngles.x);
		
		if (limitPan)
			pan = Mathf.Clamp(pan, worldMinAngles.y, worldMaxAngles.y);
	

//		Quaternion rotation = Quaternion.Inverse(this.transform.rotation) * Quaternion.Euler(tilt, pan, 0f);
//		Vector3 localAngles = rotation.eulerAngles;
//		if (localAngles.x > 180f)
//			localAngles.x -= 360f;
//		if (localAngles.y > 180f)
//			localAngles.y -= 360f;
//				
//		// now limit the angle if necessary in local coordinate
//		if (limitTilt)
//			localAngles.x = Mathf.Clamp(localAngles.x, -tiltLimitAngle, tiltLimitAngle);
//
//		if (limitPan)
//			localAngles.y = Mathf.Clamp(localAngles.y, -panLimitAngle, panLimitAngle);
//
//		// recompute the world angle after clamp in local
//		rotation = this.transform.rotation * Quaternion.Euler(localAngles.x, localAngles.y, 0f);
//		Vector3 worldAngles = rotation.eulerAngles;
//		tilt = worldAngles.x;
//		pan =  worldAngles.y;
//		roll = worldAngles.z;

	}
}
