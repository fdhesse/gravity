﻿using UnityEngine;
using System.Collections;

public class CameraTarget : MonoBehaviour
{
	public enum AngleConstraint
	{
		NONE = 0,
		CONE,
		CYLINDER,
		FREEZE,
		FRUSTRUM,
	}

	[Tooltip("The type of constraint for the camera rotation around this target.\nNONE=no constraint.\nCYLINDER=vertical constraint only.\nCONE=constraint on two axis.")]
	public AngleConstraint angleContraintType = AngleConstraint.NONE;

	[Range(0,90)][Tooltip("The maximum half angle limitation on the horizontal plane (xz) of the target game object.")]
	public float panLimitAngle = 90f;

	[Range(0,90)][Tooltip("The maximum half angle limitation on the vertical plane (yz) of the target game object.")]
	public float tiltLimitAngle = 60f;

	public void clampAngle(ref float tilt, ref float pan)
	{
		switch (angleContraintType)
		{
		case AngleConstraint.NONE:
			// nothing to do
			break;
		case AngleConstraint.FREEZE:
			clampFreeze(ref tilt, ref pan);
			break;
		case AngleConstraint.CONE:
			clampConeAndCylinder(ref tilt, ref pan, true);
			break;
		case AngleConstraint.CYLINDER:
			clampConeAndCylinder(ref tilt, ref pan, false);
			break;
		case AngleConstraint.FRUSTRUM:
			clampFrustrum(ref tilt, ref pan);
			break;
		}
	}

	private void clampFreeze(ref float tilt, ref float pan)
	{
		// in freeze mode, juste return the angle of the target orientation
		Vector3 targetAngles = this.transform.rotation.eulerAngles;
		if (targetAngles.z == 180f)
		{
			tilt = 180f - targetAngles.x;
			pan =  targetAngles.y - 180f;
		}
		else
		{
			tilt = targetAngles.x;
			pan =  targetAngles.y;
		}
	}

	private void clampConeAndCylinder(ref float tilt, ref float pan, bool limitPan)
	{
		// compute the orientation in the local coordinate of that target
		Quaternion rotation = Quaternion.Inverse(this.transform.rotation) * Quaternion.Euler(tilt, pan, 0f);
		Vector3 localAngles = rotation.eulerAngles;
		if (localAngles.x > 180f)
			localAngles.x -= 360f;
		if (localAngles.y > 180f)
			localAngles.y -= 360f;

		// now limit the angle if necessary in local coordinate
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

	private void clampFrustrum(ref float tilt, ref float pan)
	{
		// get the angle of this current target
		Vector3 target = this.transform.rotation.eulerAngles;
		
		// limit the angle in world coordinate
		tilt = Mathf.Clamp(tilt, -tiltLimitAngle + target.x, tiltLimitAngle + target.x);
		pan = Mathf.Clamp(pan, -panLimitAngle + target.y, panLimitAngle + target.y);
	}
}
