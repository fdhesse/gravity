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

	[Tooltip("The type of constraint for the camera rotation around this target.\nNONE=no constraint.\nCYLINDER=vertical constraint only.\nCONE=constraint on two axis.")]
	public AngleConstraint angleContraintType = AngleConstraint.NONE;

	[Range(0,90)][Tooltip("The maximum half angle limitation on the horizontal plane (xz) of the target game object.")]
	public float panLimitAngle = 90f;

	[Range(0,90)][Tooltip("The maximum half angle limitation on the vertical plane (yz) of the target game object.")]
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

		// compute the orientation in the local coordinate of that target
		Quaternion rotation = Quaternion.Inverse(this.transform.rotation) * Quaternion.Euler(tilt, pan, 0f);
		Vector3 localAngles = rotation.eulerAngles;
		if (localAngles.x > 180f)
			localAngles.x -= 360f;
		if (localAngles.y > 180f)
			localAngles.y -= 360f;

		// now limit the angle if necessary in local coordinate
		if (limitTilt)
			localAngles.x = Mathf.Clamp(localAngles.x, -tiltLimitAngle, tiltLimitAngle);

		if (limitPan)
			localAngles.y = Mathf.Clamp(localAngles.y, -panLimitAngle, panLimitAngle);

		// recompute the world angle after clamp in local
		rotation = this.transform.rotation * Quaternion.Euler(localAngles.x, localAngles.y, localAngles.z);
		Vector3 worldAngles = rotation.eulerAngles;

		// because of the gimbal lock pb, the angles given in world angle may have been rotated and not like
		// what they were before the limitation, so we rotate them back
		Vector3 rotatedAngles = new Vector3(180f - worldAngles.x, worldAngles.y - 180f, worldAngles.z - 180f);

		// then we take the angles with the minimum of difference (the normal one or the rotated ones)
		float diffAngle = Mathf.Abs(Mathf.DeltaAngle(tilt, worldAngles.x)) + Mathf.Abs(Mathf.DeltaAngle(pan, worldAngles.y));
		float diffRotatedAngle = Mathf.Abs(Mathf.DeltaAngle(tilt, rotatedAngles.x)) + Mathf.Abs(Mathf.DeltaAngle(pan, rotatedAngles.y));

		if (diffAngle < diffRotatedAngle)
		{
			tilt = worldAngles.x;
			pan =  worldAngles.y;
		}
		else
		{
			tilt = rotatedAngles.x;
			pan =  rotatedAngles.y;
		}
	}
}
