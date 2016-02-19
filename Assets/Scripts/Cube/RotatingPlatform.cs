#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

public class RotatingPlatform : MonoBehaviour
{
	public enum ConstraintAxis { X, Y, Z };
	public ConstraintAxis constrainedAxis;

	public enum ClockwiseConstraint { None, Clockwise, AntiClockwise };
	public ClockwiseConstraint clockwiseConstraint = ClockwiseConstraint.Clockwise;

	public float delay = 1.0f;

	public enum Interpolation { Linear, Sinerp };
	public Interpolation interpolation = Interpolation.Sinerp;

	private Vector3 startPosition;
	private Quaternion startRotation;

	Vector3 lastGravity = -Vector3.up;

	private bool isRotating = false;

	public bool IsRotating
	{
		get { return isRotating; }
	}

	private float rotation
	{
		get {
			if( constrainedAxis == ConstraintAxis.X )
				return transform.rotation.eulerAngles.x;
			else if( constrainedAxis == ConstraintAxis.Y )
				return transform.rotation.eulerAngles.y;
			else if ( constrainedAxis == ConstraintAxis.Z )
				return transform.rotation.eulerAngles.z;
			
			return 0f;
		}
		set	{
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

	public void Reset(TileOrientation startingOrientation)
	{
		lastGravity = -Vector3.up;

		transform.position = startPosition;
		transform.rotation = startRotation;

		ChangeGravityImmediate( startingOrientation );
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
		
		if ( clockwiseConstraint == ClockwiseConstraint.None )
			transform.rotation = GetDesiredRotation ();
		else
			rotation = GetDesiredAngle ();

		RecomputePlatformTiles();
	}
	
	public void ChangeGravity( TileOrientation orientation )
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

		if ( clockwiseConstraint == ClockwiseConstraint.None )
			StartCoroutine (LookTowardsDirection ( GetDesiredRotation() ));
		else
			StartCoroutine (LookTowardsDirection ( GetDesiredAngle() ));
	}
	
	//<summary>
	// Returns a Quaternion pointing in the desired rotation
	//</summary
	private Quaternion GetDesiredRotation()
	{
		// Works very well with Quaternions
		float angle;
		Vector3 axis = Vector3.up;
		Quaternion tRotation = transform.rotation;

		Vector3 targetPosition = transform.position + Physics.gravity;
		
		if (constrainedAxis == ConstraintAxis.X)
			axis = Vector3.left;
		else if (constrainedAxis == ConstraintAxis.Y)
			axis = Vector3.up;
		else if (constrainedAxis == ConstraintAxis.Z)
			axis = Vector3.forward;

		transform.LookAt ( targetPosition, axis );
		transform.rotation.ToAngleAxis ( out angle, out axis );
		transform.rotation = tRotation;

		return Quaternion.AngleAxis( angle, axis );
	}
	
	//<summary>
	// Returns rotation's angle
	//</summary
	private float GetDesiredAngle()
	{
		float angleA, angleB;

		Vector3 forwardA = lastGravity;
		Vector3 forwardB = Physics.gravity;

		lastGravity = forwardB;

		// Compute the angles, constrained to their axis
		if (constrainedAxis == ConstraintAxis.X)
		{
			angleA = Mathf.Atan2( forwardA.y, forwardA.z ) * Mathf.Rad2Deg;
			angleB = Mathf.Atan2( forwardB.y, forwardB.z ) * Mathf.Rad2Deg;
		}
		else if (constrainedAxis == ConstraintAxis.Y)
		{
			angleA = Mathf.Atan2( forwardA.x, forwardA.z ) * Mathf.Rad2Deg;
			angleB = Mathf.Atan2( forwardB.x, forwardB.z ) * Mathf.Rad2Deg;
		}
		else
		{
			angleA = Mathf.Atan2( forwardA.x, forwardA.y ) * Mathf.Rad2Deg;
			angleB = Mathf.Atan2( forwardB.x, forwardB.y ) * Mathf.Rad2Deg;
		}

		float angleDiff = Mathf.DeltaAngle( angleA, angleB );
		
		// If anti-clockwise, the angle is reversed
		// expect if it equals 180 degrees
		if (clockwiseConstraint == ClockwiseConstraint.AntiClockwise && angleDiff != 180)
			angleDiff = -angleDiff;

		return angleDiff;
	}

	private IEnumerator LookTowardsDirection( float toRotation )
	{
		// set the flag to tell that the platform is rotating
		this.isRotating = true;
		
		float elapsedTime = 0;
		Vector3 axis = Vector3.left;

		bool wideRotation = false;
		
		if (toRotation < 0)
			wideRotation = true;

		// Each constraint axis
		if (constrainedAxis == ConstraintAxis.X)
			axis = Vector3.left;
		else if (constrainedAxis == ConstraintAxis.Y)
			axis = Vector3.up;
		else if (constrainedAxis == ConstraintAxis.Z)
			axis = Vector3.back;

		// Invert the angle and axis for the anti-clockwise rotation
		if (clockwiseConstraint != ClockwiseConstraint.Clockwise)
		{
			toRotation = 360 + toRotation;
			axis = -axis;

			if ( wideRotation )
				toRotation = toRotation - 360;
		}

		while ( elapsedTime < delay )
		{
			elapsedTime += Time.deltaTime;

			//float t = elapsedTime / delay;
			
			//if ( interpolation == Interpolation.Sinerp )
			//	t = Mathf.Sin( t * Mathf.PI * 0.5f );
				
			
			if ( !wideRotation )
			{
				// Regular case
				float angle = Mathf.LerpAngle( 0, toRotation, Time.deltaTime / delay );
				transform.Rotate( axis, angle );
			}
			else
			{
				// Special case of a wide rotation, the
				// rotation is discomposed in two parts
				if ( elapsedTime < ( 2 * delay / 3 ) )
				{
					float angle = Mathf.LerpAngle( 0, 180, 3 * Time.deltaTime / delay );
					angle /= 2;
					transform.Rotate( axis, angle );
				}
				else
				{
					float angle = Mathf.LerpAngle( 0, 90, 3 * Time.deltaTime / delay );
					transform.Rotate( axis, angle );
				}
			}

			yield return null;
		}

		// We want a precise result, because of
		// Rotate we must compute it manually
		rotation = Mathf.RoundToInt (rotation);

		if ( rotation > 0 && ( rotation != 90 || rotation != 180 || rotation != 270 || rotation != 360 ) )
		{
			if ( rotation < 45 )
				rotation = 0;
			else if ( rotation < 135 )
				rotation = 90;
			else if ( rotation < 225 )
				rotation = 180;
			else if ( rotation < 315 )
				rotation = 270;
			else if ( rotation < 400 )
				rotation = 360;
		}
		else if ( rotation < 0 && ( rotation != -90 || rotation != -180 || rotation != -270 || rotation != -360 ) )
		{
			if ( rotation > -45 )
				rotation = 0;
			else if ( rotation > -135 )
				rotation = -90;
			else if ( rotation > -225 )
				rotation = -180;
			else if ( rotation > -315 )
				rotation = -270;
			else if ( rotation > -400 )
				rotation = -360;
		}

		RecomputePlatformTiles();

		// clear the flag
		this.isRotating = false;
	}

	private IEnumerator LookTowardsDirection( Quaternion toRotation )
	{
		// So effective !

		// set the flag to tell that the platform is rotating
		this.isRotating = true;

		float elapsedTime = 0;

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
		RecomputePlatformTiles();
		
		// clear the flag
		this.isRotating = false;
	}

	private void RecomputePlatformTiles()
	{
		Tile[] tiles = gameObject.GetComponentsInChildren<Tile> ();

		foreach (Tile tile in tiles)
		{
			// update the orientation of the platform
			tile.CheckTileOrientation();
			// also ask to rescan the path
			tile.rescanPath = true;
		}

		// update also the golden tile material
		Pawn.Instance.world.UpdateGoldTileOrientation();
	}
	
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Vector3 from = transform.position;
		Vector3 to = transform.position;
		
		if (constrainedAxis == ConstraintAxis.X) {
			from.x -= 20;
			to.x += 20;
		} else if (constrainedAxis == ConstraintAxis.Y) {
			from.y -= 20;
			to.y += 20;
		} else if (constrainedAxis == ConstraintAxis.Z) {
			from.z -= 20;
			to.z += 20;
		}
		
		// Draw the platform pivot
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine (from, to);
		
		Gizmos.color = Color.cyan;
		// Draw the platform direction
		if (clockwiseConstraint == ClockwiseConstraint.None)
			Gizmos.DrawLine (transform.position, (transform.position + transform.forward * 30));
		else
		{
			if (constrainedAxis != ConstraintAxis.Y)
				Gizmos.DrawLine (transform.position, (transform.position + transform.up * -30));
			else
				Gizmos.DrawLine (transform.position, (transform.position + transform.forward * -30));
		}
	}
	#endif
}
