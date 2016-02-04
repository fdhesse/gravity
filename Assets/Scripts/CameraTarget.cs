using UnityEngine;
using System.Collections;

public class CameraTarget : MonoBehaviour
{
	public enum AngleConstraint
	{
		NONE = 0,
		FREEZE,
		CONE,
		CYLINDER,
	}

	public AngleConstraint angleContraintType = AngleConstraint.NONE;

	[Range(0,90)]
	public float panLimitAngle = 90f;

	[Range(0,90)]
	public float tiltLimitAngle = 60f;


	public void clampAngle(ref float pan, ref float tilt)
	{
		bool limitPan = false;
		bool limitTilt = false;

		switch (angleContraintType)
		{
		case AngleConstraint.NONE:
			// nothing to do
			return;
		case AngleConstraint.FREEZE:
			// in freeze mode, pan and tilt cannot move
			pan = 0.0f;
			tilt = 0.0f;
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
		if (limitPan)
			pan = Mathf.Clamp(pan, -panLimitAngle, panLimitAngle);

		if (limitTilt)
			tilt = Mathf.Clamp(tilt, -tiltLimitAngle, tiltLimitAngle);
	}
}
