using UnityEngine;
using System.Collections;

public class GravityPlatformBody : MonoBehaviour
{
	private bool isOutOfBounds;
	private Vector3 spawnPosition; // position of the Cube GameObject initial position

	private Pawn PlayerPawn;
	private Rigidbody body;

	[HideInInspector] public GravityPlatform LegacyParent;

	void Awake()
	{
		// Make a joint between rigidbody and legacy object
		GameObject bodyDummyParent = GameObject.Find ("RigidBody Dummies");
		
		if ( bodyDummyParent == null )
		{
			bodyDummyParent = new GameObject( "RigidBody Dummies" );
			//bodyDummyParent.hideFlags = HideFlags.HideInHierarchy;
		}
		
		PlayerPawn = (Pawn) GameObject.Find ("Pawn").GetComponent<Pawn>();
		
		body = GetComponent<Rigidbody> ();
		body.transform.parent = bodyDummyParent.transform;
		body.transform.localScale *= 9.99f;
		//GetComponent<BoxCollider>().size *= 10.001f;

		body.interpolation = RigidbodyInterpolation.Interpolate;
	}

	void Update()
	{
		// if the cube is still falling, game can't continue
		if ( Vector3.Magnitude(body.velocity) > 0f && !isOutOfBounds )
			LegacyParent.isFalling = true;
		else
			LegacyParent.isFalling = false;

		// APPAREMENT LES vitesses sont doublées ici

		LegacyParent.transform.position = transform.position;
		LegacyParent.transform.rotation = transform.rotation;
	}

	public void Reset()
	{
		if (spawnPosition == Vector3.zero)
			spawnPosition = transform.position;
		else
			transform.position = spawnPosition;

		body.velocity = Vector3.zero;
		body.angularVelocity = Vector3.zero;
		body.constraints = RigidbodyConstraints.FreezeAll; // .FreezeRotation | RigidbodyConstraints.FreezePositionZ;
		body.isKinematic = false;

		isOutOfBounds = false;
	}
	
	public void Freeze()
	{
		body.velocity = Vector3.zero;
		body.angularVelocity = Vector3.zero;
		body.constraints = RigidbodyConstraints.FreezeAll;
	}
	
	public bool Unfreeze( TileOrientation orientation )
	{
		if ( orientation == TileOrientation.Down || orientation == TileOrientation.Up )
		{
			if( LegacyParent.constrainedAxis == GravityPlatform.ConstraintAxis.Y )
			{
				body.constraints = RigidbodyConstraints.FreezeRotation | ~RigidbodyConstraints.FreezePositionY;
				return false;
			}
		}
		else if ( orientation == TileOrientation.Right || orientation == TileOrientation.Left )
		{
			if( LegacyParent.constrainedAxis == GravityPlatform.ConstraintAxis.X )
			{
				body.constraints = RigidbodyConstraints.FreezeRotation | ~RigidbodyConstraints.FreezePositionX;
				return false;
			}
		}
		else if ( orientation == TileOrientation.Front || orientation == TileOrientation.Back )
		{
			if( LegacyParent.constrainedAxis == GravityPlatform.ConstraintAxis.Z )
			{
				body.constraints = RigidbodyConstraints.FreezeRotation | ~RigidbodyConstraints.FreezePositionZ;
				return false;
			}
		}

		return true;
	}

	public void OutOfBounds()
	{
		isOutOfBounds = true;
	}
	
	void OnCollisionEnter(Collision collision)
	{
		Debug.Log(collision.gameObject.name, collision.gameObject);
		if (collision.gameObject.tag == "Player")
		{
			PlayerPawn.CubeContact (transform.position);
			return;
		}

		LegacyParent.SendMessage ("OnRigidbodyCollisionEnter", collision, SendMessageOptions.RequireReceiver );
	}
	
	void OnCollisionExit( Collision collision )
	{
		LegacyParent.SendMessage ("OnRigidbodyCollisionExit", collision, SendMessageOptions.RequireReceiver);
	}
}
