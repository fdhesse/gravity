#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

public class RotatingPlatform : MonoBehaviour
{
	public enum ConstraintAxis { X, Y, Z };
	public ConstraintAxis constrainedAxis;

	public enum ClockwiseConstraint { None, Clockwise, CounterClockwise };
	public ClockwiseConstraint clockwiseConstraint = ClockwiseConstraint.Clockwise;

	public float delay = 1.0f;

	public enum Interpolation { Linear, Sinerp };
	public Interpolation interpolation = Interpolation.Sinerp;

	private Vector3 startPosition;
	private Quaternion startRotation;

	void Awake()
	{
		startPosition = transform.position;
		startRotation = transform.rotation;
	}

	void Start()
	{
		ChangeGravityImmediate (TileOrientation.Down);
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	public void Reset()
	{
		transform.position = startPosition;
		transform.rotation = startRotation;
	}
	
	private void ChangeGravityImmediate( TileOrientation orientation )
	{
		if ( constrainedAxis == ConstraintAxis.X )
		{
			if ( orientation == TileOrientation.Right || orientation == TileOrientation.Left )
				return;
		}
		else if ( constrainedAxis == ConstraintAxis.Y )
		{
			if ( orientation == TileOrientation.Up || orientation == TileOrientation.Down )
				return;
		}
		else if ( constrainedAxis == ConstraintAxis.Z )
		{
			if ( orientation == TileOrientation.Front || orientation == TileOrientation.Back )
				return;
		}
		
		transform.rotation = GetDesiredRotation ();
	}
	
	private void ChangeGravity( TileOrientation orientation )
	{
		if ( constrainedAxis == ConstraintAxis.X )
		{
			if ( orientation == TileOrientation.Right || orientation == TileOrientation.Left )
				return;
		}
		else if ( constrainedAxis == ConstraintAxis.Y )
		{
			if ( orientation == TileOrientation.Up || orientation == TileOrientation.Down )
				return;
		}
		else if ( constrainedAxis == ConstraintAxis.Z )
		{
			if ( orientation == TileOrientation.Front || orientation == TileOrientation.Back )
				return;
		}

		StartCoroutine (LookTowardsDirection ( GetDesiredRotation() ));
	}
	
	private Quaternion GetDesiredRotation()
	{
		float angle;
		Vector3 axis = Vector3.up;
		
		Quaternion tRotation = transform.rotation;
		Vector3 targetPosition = transform.position + Physics.gravity;
		
		if (constrainedAxis == ConstraintAxis.X)
			axis = Vector3.right;
		else if (constrainedAxis == ConstraintAxis.Y)
			axis = Vector3.up;
		else if (constrainedAxis == ConstraintAxis.Z)
			axis = Vector3.forward;
		
		transform.LookAt (targetPosition, axis);
		transform.rotation.ToAngleAxis (out angle, out axis);
		transform.rotation = tRotation;
		
		return Quaternion.AngleAxis( angle, axis );
	}

	private IEnumerator LookTowardsDirection( Quaternion toRotation )
	{
		float elapsedTime = 0;
		
		if ( clockwiseConstraint == ClockwiseConstraint.Clockwise )
		{
		}
		else if ( clockwiseConstraint == ClockwiseConstraint.CounterClockwise )
		{
			//toRotation = -toRotation;
		}

		Quaternion fromAngle = transform.rotation;
		
		while ( elapsedTime < delay )
		{
			float t = elapsedTime / delay;
			
			if ( interpolation == Interpolation.Sinerp )
				t = Mathf.Sin( t * Mathf.PI * 0.5f );
			
			transform.rotation = Quaternion.Lerp( fromAngle, toRotation, t );
			
			elapsedTime += Time.deltaTime;
			
			yield return null;
		}
		
		transform.rotation = toRotation;
		
		// Recompute the platform's tiles directions
		RecomputePlatformTiles ();
	}

	private void RecomputePlatformTiles()
	{
		Tile[] tiles = gameObject.GetComponentsInChildren<Tile> ();

		foreach (Tile tile in tiles)
			tile.CheckTileOrientation ();
	}
	
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Vector3 from = transform.position;
		Vector3 to = transform.position;
		
		if ( constrainedAxis == ConstraintAxis.X )
		{
			from.x -= 20;
			to.x += 20;
		}
		else if ( constrainedAxis == ConstraintAxis.Y )
		{
			from.y -= 20;
			to.y += 20;
		}
		else if ( constrainedAxis == ConstraintAxis.Z )
		{
			from.z -= 20;
			to.z += 20;
		}
		
		// Draw the platform pivot
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine ( from, to );

		// Draw the platform direction
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine ( transform.position, ( transform.position + transform.forward * 30 ) );
		//Handles.DrawLine ( from, ( from + transform.forward ) );
	}
	#endif
}
