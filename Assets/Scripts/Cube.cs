using UnityEngine;
using System.Collections;

public class Cube : MonoBehaviour {
	
	private Pawn PlayerPawn; // Player Pawn

	public Platform platform; // Platform beneath the Cube

	public Vector3 spawnPosition; // position of the Cube GameObject initial position
	
	// Use this for initialization
	void Start () {

		rigidbody.constraints = RigidbodyConstraints.FreezeAll; // .FreezeRotation | RigidbodyConstraints.FreezePositionZ;
		PlayerPawn = (Pawn) GameObject.Find ("Pawn").GetComponent<Pawn>();
	}
	
	// Update is called once per frame
	void Update () {
		if (PlayerPawn.rigidbody.useGravity) {
			rigidbody.constraints = PlayerPawn.rigidbody.constraints;
		}
		else
		{
			rigidbody.constraints = PlayerPawn.nextConstraint;
		}
	}

	public void Reset () {

		if (spawnPosition == Vector3.zero)
			spawnPosition = transform.position;
		else
			transform.position = spawnPosition;

		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
	}
	
	public void OnCollisionEnter(Collision collision)
	{
		// ADD A Control to avoid player change physic too soon
		// in order to avoir "gluing effect"
		
		if (collision.gameObject.tag == "Player")
			PlayerPawn.CubeContact (transform.position);
		else if (collision.relativeVelocity.magnitude > 2)
			audio.Play();
	}
}
