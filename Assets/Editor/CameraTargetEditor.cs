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
		const int CYLINDER_EDGE_COUNT = 8;
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
			float cylinderRadius = size;
			float cylinderHeight = size * Mathf.Sin(src.tiltLimitAngle * Mathf.Deg2Rad);
			float halfCylinderHeight = cylinderHeight * 0.5f;
			// set the matrix in the center of the object
			Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
			// draw the tow circles
			drawFlatEllipse(SECTION_COUNT, 0, cylinderRadius, cylinderRadius, halfCylinderHeight);
			Vector3[] bottomPoints = drawFlatEllipse(SECTION_COUNT, CYLINDER_EDGE_COUNT, cylinderRadius, cylinderRadius, -halfCylinderHeight);
			// draw the side lines
			Vector3 cylinderTranslation = new Vector3(0f, 0f, cylinderHeight);
			for (int i = 0; i < bottomPoints.Length; ++i)
				Gizmos.DrawLine(bottomPoints[i], bottomPoints[i] + cylinderTranslation);
			break;
		}
	}

	static Vector3[] drawFlatEllipse(int segments, int returnedPointCount, float xradius, float yradius, float z)
	{
		float x = 0f;
		float y = 0f;
		Vector3 previousPosition = new Vector3(0f, yradius, z);
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
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * xradius;
			y = Mathf.Cos (Mathf.Deg2Rad * angle) * yradius;

			Vector3 newPosition = new Vector3(x, y, z);
			Gizmos.DrawLine(previousPosition, newPosition);
			previousPosition = newPosition;

			angle += angleIncrement;
		}

		return returnedPoints;
	}


	static Vector3[] drawEllipse(int segments, int returnedPointCount, float xradius, float yradius, float distance)
	{
		float squaredDistance = distance*distance;
		float x = 0f;
		float y = 0f;
		float z = 0f;
		Vector3 previousPosition = new Vector3(0f, yradius, Mathf.Sqrt(distance*distance - (yradius*yradius)));
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
				z = Mathf.Sqrt(squaredDistance - squareHypo);
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
