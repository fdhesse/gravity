#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

public class RotatingPlatform : MonoBehaviour
{
	public enum ConstraintAxis { X, Y, Z };
	public ConstraintAxis constrainedAxis;

	public bool Clockwise = true;

	public float delay = 1.0f;

	public enum Interpolation { Linear, Sinerp };
	public Interpolation interpolation;

	private Vector3 startPosition;
	private Quaternion startRotation;


	private float rotation
	{
		get
		{
			if( constrainedAxis == ConstraintAxis.X )
				return transform.rotation.eulerAngles.x;
			if( constrainedAxis == ConstraintAxis.Y )
				return transform.rotation.eulerAngles.y;
			if ( constrainedAxis == ConstraintAxis.Z )
				return transform.rotation.eulerAngles.z;

			return 0f;
			}
		set
		{
			if( constrainedAxis == ConstraintAxis.X )
				transform.rotation = Quaternion.Euler( new Vector3( value, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z ) );
			else if( constrainedAxis == ConstraintAxis.Y )
				transform.rotation = Quaternion.Euler( new Vector3( transform.rotation.eulerAngles.x, value, transform.rotation.eulerAngles.z ) );
			else if ( constrainedAxis == ConstraintAxis.Z )
				transform.rotation = Quaternion.Euler( new Vector3( transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, value ) );
		}
	}
	void Awake()
	{
		startPosition = transform.position;
		startRotation = transform.rotation;
	}

	void Start()
	{
		rotation = GetDesiredAngle ();
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

	private float GetDesiredAngle()
	{
		//transform.rotation = tLocalRotation;
		Quaternion tRotation = transform.rotation;

		Vector3 lookAtPos = transform.position + Physics.gravity;

		transform.LookAt (lookAtPos);

		//Vector3 localTarget = transform.InverseTransformPoint (lookAtPos);
		//float angle = Mathf.RoundToInt( Mathf.Atan2( localTarget.x, localTarget.z ) * Mathf.Rad2Deg );

		Quaternion desiredRotation = transform.rotation;

		//if (constrainedAxis == ConstraintAxis.X)
		//	Debug.Log ("desiredRotation: " + desiredRotation.eulerAngles);
		
		//transform.rotation = tLocalRotation;
		transform.rotation = tRotation;

		float desiredAngle = 0;

		
		//desiredAngle = desiredRotation.eulerAngles.x + desiredRotation.eulerAngles.y + desiredRotation.eulerAngles.z;

		if (constrainedAxis == ConstraintAxis.X)
			desiredAngle = desiredRotation.eulerAngles.x;
		else if (constrainedAxis == ConstraintAxis.Y)
			desiredAngle = desiredRotation.eulerAngles.y;
		else if (constrainedAxis == ConstraintAxis.Z)
			desiredAngle = desiredRotation.eulerAngles.y;
			
		

		return Vector3.Angle( tRotation.eulerAngles, desiredRotation.eulerAngles );
		//return 0f;
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

		StartCoroutine (LookTowardsGravity ( GetDesiredAngle() ));
	}

	private IEnumerator LookTowardsGravity( float desiredAngle )
	{
		if ( constrainedAxis == ConstraintAxis.X )
		Debug.Log ("angle: " + desiredAngle);
		float elapsedTime = 0;

		int rotationDirection = 1;

		if (!Clockwise)
			rotationDirection = -1;

		float fromAngle = rotation;

		while ( elapsedTime < delay )
		{
			float t = elapsedTime / delay;

			if ( interpolation == Interpolation.Sinerp )
				t = Mathf.Sin( t * Mathf.PI * 0.5f );

			rotation = Mathf.Lerp( fromAngle, desiredAngle, t );

			elapsedTime += Time.deltaTime;

			yield return null;
		}

		rotation = desiredAngle;

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
			from.x -= 10;
			to.x += 10;
		}
		else if ( constrainedAxis == ConstraintAxis.Y )
		{
			from.y -= 10;
			to.y += 10;
		}
		else if ( constrainedAxis == ConstraintAxis.Z )
		{
			from.z -= 10;
			to.z += 10;
		}
		
		// Draw the platform pivot
		Gizmos.color = Color.red;
		Gizmos.DrawLine ( from, to );

		// Draw the platform direction
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine ( transform.position, ( transform.position + transform.forward * 30 ) );
		//Handles.DrawLine ( from, ( from + transform.forward ) );
	}
	#endif
}
