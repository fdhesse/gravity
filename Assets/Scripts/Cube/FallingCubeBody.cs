using UnityEngine;

public class FallingCubeBody : MonoBehaviour
{
	private bool isOutOfBounds;
	private Vector3 spawnPosition; // position of the Cube GameObject initial position
	private Vector3 lastPosition = Vector3.zero; // position of the game object at the previous frame	

	Pawn playerPawn;
    Rigidbody playerPawnRigidbody;
	Rigidbody body;

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
		
		playerPawn = GameObject.Find ("Pawn").GetComponent<Pawn>();
	    playerPawnRigidbody = playerPawn.GetComponent<Rigidbody>();

        body = GetComponent<Rigidbody> ();
		body.transform.parent = bodyDummyParent.transform;
		body.transform.localScale *= 1.001f;

		body.interpolation = RigidbodyInterpolation.Interpolate;
	}

	public void Update()
	{
		// if the cube is still falling, game can't continue
		if ( Vector3.Magnitude(transform.position - lastPosition) > 0.001f && !isOutOfBounds ) {
			LegacyParent.isFalling = true;
        }
        else { 
			LegacyParent.isFalling = false;
        }

        // memorise the last position for testing at the next frame
        lastPosition = transform.position;

	    if ( playerPawnRigidbody.useGravity )
	    {
            if(body.constraints != playerPawnRigidbody.constraints ) { 
	            body.constraints = playerPawnRigidbody.constraints;
            }
        }
	    else
	    {
	        body.constraints = playerPawn.nextConstraint;
        }
		
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
			playerPawn.CubeContact (transform.position);
			return;
		}

		LegacyParent.SendMessage ("OnRigidbodyCollisionEnter", collision, SendMessageOptions.RequireReceiver );
	}
	
	void OnCollisionExit( Collision collision )
	{
		LegacyParent.SendMessage ("OnRigidbodyCollisionExit", collision, SendMessageOptions.RequireReceiver);
	}
}
