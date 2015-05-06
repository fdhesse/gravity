using UnityEngine;
using System.Collections;

public class FallingCubeBody : MonoBehaviour
{
	private bool isOutOfBounds;
	private Vector3 spawnPosition; // position of the Cube GameObject initial position

	private Pawn PlayerPawn;
	private Rigidbody body;

	[HideInInspector] public FallingCube LegacyParent;

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
		body.transform.localScale *= 1.001f;

		body.interpolation = RigidbodyInterpolation.Interpolate;

		//Reset ();
	}

	void Update()
	{
		// if the cube is still falling, game can't continue
		if ( Vector3.Magnitude(body.velocity) > 0f && !isOutOfBounds )
			LegacyParent.isFalling = true;
		else
			LegacyParent.isFalling = false;
		
		if ( PlayerPawn.GetComponent<Rigidbody>().useGravity )
			body.constraints = PlayerPawn.GetComponent<Rigidbody>().constraints;
		else
			body.constraints = PlayerPawn.nextConstraint;
		
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

		isOutOfBounds = false;
	}

	public void OutOfBounds()
	{
		isOutOfBounds = true;
	}
	
	void OnCollisionEnter(Collision collision)
	{
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
