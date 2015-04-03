using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GameplayCube))]
[ExecuteInEditMode]
public class FallingCube : MonoBehaviour {
	
	private Pawn PlayerPawn; // Player Pawn

	public Tile platform; // Tile beneath the Cube

	public Vector3 spawnPosition; // position of the Cube GameObject initial position
	
	public bool isFalling;
	private bool isDestroyed = false;
	
	// Use this for initialization
	void Start ()
	{
		GameplayCube cube = GetComponent<GameplayCube>();
		
		cube.Left = TileType.Valid;
		cube.Right = TileType.Valid;
		cube.Up = TileType.Valid;
		cube.Down = TileType.Valid;
		cube.Front = TileType.Valid;
		cube.Back = TileType.Valid;
		
		transform.FindChild( "left" ).gameObject.GetComponent<Tile>().rescanPath = true;
		transform.FindChild( "right" ).gameObject.GetComponent<Tile>().rescanPath = true;
		transform.FindChild( "up" ).gameObject.GetComponent<Tile>().rescanPath = true;
		transform.FindChild( "down" ).gameObject.GetComponent<Tile>().rescanPath = true;
		transform.FindChild( "front" ).gameObject.GetComponent<Tile>().rescanPath = true;
		transform.FindChild( "back" ).gameObject.GetComponent<Tile>().rescanPath = true;
		
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; // .FreezeRotation | RigidbodyConstraints.FreezePositionZ;
		PlayerPawn = (Pawn) GameObject.Find ("Pawn").GetComponent<Pawn>();
	}
	
	// Update is called once per frame
	void Update () {
	
		// if the cube is still falling, game can't continue
		if ( Vector3.Magnitude(GetComponent<Rigidbody>().velocity) > 0f && !isDestroyed )
			isFalling = true;
		else
			isFalling = false;
			
		if (PlayerPawn.GetComponent<Rigidbody>().useGravity) {
			GetComponent<Rigidbody>().constraints = PlayerPawn.GetComponent<Rigidbody>().constraints;
		}
		else
		{
			GetComponent<Rigidbody>().constraints = PlayerPawn.nextConstraint;
		}
	}

	public void Reset () {

		if (spawnPosition == Vector3.zero)
			spawnPosition = transform.position;
		else
			transform.position = spawnPosition;

		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		
		isDestroyed = false;
	}
	
	public void Destroy()
	{
		isDestroyed = true;
	}
	
	public void OnCollisionEnter(Collision collision)
	{
		// ADD A Control to avoid player change physic too soon
		// in order to avoir "gluing effect"
		
		if (collision.gameObject.tag == "Player")
			PlayerPawn.CubeContact (transform.position);
		else if (collision.relativeVelocity.magnitude > 2)
			GetComponent<AudioSource>().Play();
	}
}
