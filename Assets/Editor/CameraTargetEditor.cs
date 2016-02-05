using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(CameraTarget))]
public class CameraTargetEditor : Editor
{
	SerializedProperty constraintType;

	public void OnEnable()
	{
		constraintType = serializedObject.FindProperty("angleContraintType");
	}

	public override void OnInspectorGUI()
	{
		this.serializedObject.Update();
		this.DrawDefaultInspector();

		switch ((CameraTarget.AngleConstraint)(constraintType.enumValueIndex))
		{
		case CameraTarget.AngleConstraint.CONE:
			displayConeParameters();
			break;
		case CameraTarget.AngleConstraint.CYLINDER:
			displayCylinderParameters();
			break;
		}

		this.serializedObject.ApplyModifiedProperties();
	}

	private void displayConeParameters()
	{
	}

	private void displayCylinderParameters()
	{
	}

	[DrawGizmo (GizmoType.Selected | GizmoType.Active)]
	static void DrawGizmoForCameraTarget(CameraTarget src, GizmoType gizmoType)
	{
		const int SECTION_COUNT = 64;
		const int CONE_EDGE_COUNT = 4;
		Vector3 position = src.transform.position;
		Quaternion rotation = src.transform.rotation;

		// the size of the gizmo (heigh of the cone or cylinder)
		float size = 100f;

		// draw a cone or a cylinder depending on the type of constraints
		switch (src.angleContraintType)
		{
		case CameraTarget.AngleConstraint.CONE:
			// compute the x and y radius of the base of the cone, depending on the target limit angle
			float xRadius = size * Mathf.Sin(src.panLimitAngle * Mathf.Deg2Rad);
			float yRadius = size * Mathf.Sin(src.tiltLimitAngle * Mathf.Deg2Rad);
			// set the matrix in the center of the object
			Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
			// draw the base of the cone
			Vector3[] basePoints = drawEllipse(SECTION_COUNT, CONE_EDGE_COUNT, xRadius, yRadius, size);
			// draw the lines on the side
			foreach (Vector3 point in basePoints)
				Gizmos.DrawLine(Vector3.zero, point);
			break;

		case CameraTarget.AngleConstraint.CYLINDER:
			float cylinderRadius = size * Mathf.Cos(src.tiltLimitAngle * Mathf.Deg2Rad);
			float cylinderHeight = size * Mathf.Sin(src.tiltLimitAngle * Mathf.Deg2Rad);
			// set the matrix in the center of the object
			Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
			// draw the tow cap circles
			drawCircleArc(SECTION_COUNT, 0f, 360f, cylinderRadius, cylinderHeight, 1);
			drawCircleArc(SECTION_COUNT, 0f, 360f, cylinderRadius, -cylinderHeight, 1);
			// draw the side circles
			drawCircleArc(SECTION_COUNT/2, -src.tiltLimitAngle, src.tiltLimitAngle, size, 0f, 0);
			drawCircleArc(SECTION_COUNT/2, 180f - src.tiltLimitAngle, 180f + src.tiltLimitAngle, size, 0f, 0);
			drawCircleArc(SECTION_COUNT/2, 90f - src.tiltLimitAngle, 90f + src.tiltLimitAngle, size, 0f, 2);
			drawCircleArc(SECTION_COUNT/2, 270f - src.tiltLimitAngle, 270f + src.tiltLimitAngle, size, 0f, 2);
			break;
		}
	}

	static void drawCircleArc(int segments, float startAngle, float endAngle, float radius, float planeConstant, int planeIndex)
	{
		// rotating angle increment
		float angleIncrement = ((endAngle - startAngle) / segments);
		float angle = startAngle + angleIncrement;

		// compute the first previous position
		Vector3 previousPosition = computeArcPosition(startAngle, radius, planeConstant, planeIndex);

		for (int i = 0; i < segments; i++)
		{
			// compute the new position
			Vector3 newPosition = computeArcPosition(angle, radius, planeConstant, planeIndex);
			Gizmos.DrawLine(previousPosition, newPosition);
			previousPosition = newPosition;

			angle += angleIncrement;
		}
	}

	static private Vector3 computeArcPosition(float angle, float radius, float planeConstant, int planeIndex)
	{
		// compute the new position
		float firstCoord = Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
		float secondCoord = Mathf.Cos (Mathf.Deg2Rad * angle) * radius;
		
		if (planeIndex == 0)
			return new Vector3(planeConstant, firstCoord, secondCoord);
		else if (planeIndex == 1)
			return new Vector3(firstCoord, planeConstant, secondCoord);
		else
			return new Vector3(firstCoord, secondCoord, planeConstant);
	}

	static Vector3[] drawEllipse(int segments, int returnedPointCount, float xradius, float yradius, float distance)
	{
		float squaredDistance = distance*distance;
		float x = 0f;
		float y = 0f;
		float z = 0f;
		Vector3 previousPosition = new Vector3(0f, yradius, -Mathf.Sqrt(distance*distance - (yradius*yradius)));
		float angleIncrement = (360f / segments);
		float angle = angleIncrement;

		Vector3[] returnedPoints = null;
		int pointGapCount = 0;
		if (returnedPointCount > 0)
		{
			returnedPoints = new Vector3[returnedPointCount];
			pointGapCount = segments / returnedPointCount;
		}
		
		for (int i = 0; i < segments; i++)
		{
			//store the points in the return array if needed
			if ((returnedPointCount > 0) && ((i % pointGapCount) == 0))
				returnedPoints[i / pointGapCount] = previousPosition;

			// compute the new position
			x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
			y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

			// compute the z
			float squareHypo = (x*x) + (y*y);
			if (squareHypo < squaredDistance)
				z = -Mathf.Sqrt(squaredDistance - squareHypo);
			else
				z = 0f;

			Vector3 newPosition = new Vector3(x, y, z);
			Gizmos.DrawLine(previousPosition, newPosition);
			previousPosition = newPosition;
			
			angle += angleIncrement;
		}
		
		return returnedPoints;
	}
}
