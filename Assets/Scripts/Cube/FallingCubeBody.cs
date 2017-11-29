using UnityEngine;
using System.Collections;

public class FallingCubeBody : MonoBehaviour
{
	private bool m_IsOutOfBounds = false;
	private Vector3 m_SpawnPosition = Vector3.zero; // position of the Cube GameObject initial position
	private Vector3 m_LastPosition = Vector3.zero; // position of the game object at the previous frame	
	private Rigidbody m_RigidBody;

	[HideInInspector]
	public FallingCube LegacyParent;

	void Awake()
	{
		// Make a joint between rigidbody and legacy object
		GameObject bodyDummyParent = GameObject.Find("RigidBody Dummies");
		
		if ( bodyDummyParent == null )
		{
			bodyDummyParent = new GameObject( "RigidBody Dummies" );
			bodyDummyParent.hideFlags = HideFlags.HideInHierarchy;
		}
		
		// configure the rigid body
		m_RigidBody = GetComponent<Rigidbody>();
		m_RigidBody.transform.parent = bodyDummyParent.transform;
		m_RigidBody.transform.localScale *= 1.001f;
		m_RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		m_RigidBody.interpolation = RigidbodyInterpolation.Interpolate;

		// memorize the spawn position
		m_SpawnPosition = transform.position;
	}

	void Update()
	{
		// if the cube is still falling, game can't continue
		if ( Vector3.Magnitude(transform.position - m_LastPosition) > 0.001f && !m_IsOutOfBounds )
			LegacyParent.isFalling = true;
		else
			LegacyParent.isFalling = false;

		// memorise the last position for testing at the next frame
		m_LastPosition = transform.position;
			
		LegacyParent.transform.position = transform.position;
		LegacyParent.transform.rotation = transform.rotation;
	}

	public void Reset()
	{
		// reinit the position to the spawn position
		transform.position = m_SpawnPosition;

		// reset the dynamics of the rigid body
		m_RigidBody.velocity = Vector3.zero;
		m_RigidBody.angularVelocity = Vector3.zero;
		m_RigidBody.constraints = RigidbodyConstraints.FreezeAll; // .FreezeRotation | RigidbodyConstraints.FreezePositionZ;

		// reset the internal flags
		m_IsOutOfBounds = false;
	}

	public void OutOfBounds()
	{
		m_IsOutOfBounds = true;
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			collision.gameObject.GetComponent<Pawn>().CubeContact(transform.position);
			return;
		}

		LegacyParent.SendMessage("OnRigidbodyCollisionEnter", collision, SendMessageOptions.RequireReceiver );
	}
	
	void OnCollisionExit( Collision collision )
	{
		LegacyParent.SendMessage("OnRigidbodyCollisionExit", collision, SendMessageOptions.RequireReceiver);
	}
}
