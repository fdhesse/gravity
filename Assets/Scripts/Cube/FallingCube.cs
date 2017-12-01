using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(GameplayCube))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class FallingCube : MonoBehaviour
{
	public bool IsFalling
	{
		get { return !m_RigidBody.IsSleeping(); }
	}

	private Vector3 m_SpawnPosition = Vector3.zero; // position of the Cube GameObject initial position
	private Vector3 m_LastPosition = Vector3.zero; // position of the game object at the previous frame	
	private Rigidbody m_RigidBody;

	private Tile[] m_Tiles;
	private Dictionary<Tile, TileType> m_ObstructedTiles;

	void Awake()
	{
		m_ObstructedTiles = new Dictionary<Tile, TileType>();

		// configure the rigid body
		m_RigidBody = GetComponent<Rigidbody>();
//		m_RigidBody.transform.localScale *= 1.001f; // why?
		m_RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		m_RigidBody.interpolation = RigidbodyInterpolation.Interpolate;

		// memorize the spawn position
		m_SpawnPosition = transform.position;
	}

	// Use this for initialization
	void Start()
	{
		GameplayCube cube = GetComponent<GameplayCube>();

		// make sure we have a face on all the six faces of the cube,
		// so if the type is none transform it into invalid
		if (cube.Left == TileType.None)
			cube.Left = TileType.Invalid;
		
		if (cube.Right == TileType.None)
			cube.Right = TileType.Invalid;

		if (cube.Up == TileType.None)
			cube.Up = TileType.Invalid;

		if (cube.Down == TileType.None)
			cube.Down = TileType.Invalid;

		if (cube.Front == TileType.None)
			cube.Front = TileType.Invalid;

		if (cube.Back == TileType.None)
			cube.Back = TileType.Invalid;
		
		// reset the rescan path
		m_Tiles = GetComponentsInChildren<Tile>();
		foreach (Tile tile in m_Tiles)
			tile.rescanPath = true;
	}

	public void Reset(TileOrientation startingOrientation)
	{
		// reinit the position to the spawn position
		transform.position = m_SpawnPosition;

		// reset the dynamics of the rigid body
		m_RigidBody.velocity = Vector3.zero;
		m_RigidBody.angularVelocity = Vector3.zero;
		m_RigidBody.useGravity = true;
		m_RigidBody.WakeUp();
	}

	public void ChangeGravity(TileOrientation gravityOrientation)
	{
		// when the gravity just got changed, wake the rigid body
		m_RigidBody.WakeUp();
	}

	public void OutOfBounds()
	{
		// reset the dynamic and stop making me sensible to the gravity (to stop falling)
		m_RigidBody.velocity = Vector3.zero;
		m_RigidBody.angularVelocity = Vector3.zero;
		m_RigidBody.useGravity = false;
		// then go to sleep so that the IsFalling flag becomes false
		m_RigidBody.Sleep();
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			collision.gameObject.GetComponent<Pawn>().CubeContact(transform.position);
			return;
		}

		if (collision.relativeVelocity.magnitude > 2)
			GetComponent<AudioSource>().Play();
		
		// Detect all the obstructed tiles
		foreach ( TileOrientation orientation in System.Enum.GetValues( typeof( TileOrientation ) ) )
		{
			RaycastHit[] hitInfos = Physics.RaycastAll( transform.position, World.GetGravityNormalizedVector( orientation ), transform.localScale.x * 0.55f, 1 << LayerMask.NameToLayer( "Tiles" ) );
			
			foreach ( RaycastHit hitInfo in hitInfos )
			{
				if ( !hitInfo.collider.transform.IsChildOf( transform ) )
				{
					Tile tile = hitInfo.collider.GetComponent<Tile>();
					
					if ( m_ObstructedTiles.ContainsKey( tile ) )
						continue;
					
					m_ObstructedTiles.Add( tile, tile.Type );
					tile.Type = TileType.Invalid;
					tile.rescanPath = true;
				}
			}
		}
		
		foreach (Tile tile in m_Tiles)
		{
			tile.rescanPath = true;
			
			foreach ( Tile t in tile.connections )
				t.rescanPath = true;
		}
	}

	void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
			return;
		
		foreach ( KeyValuePair<Tile, TileType > entry in m_ObstructedTiles )
		{
			Tile tile = entry.Key;
			tile.Type = entry.Value;
			tile.rescanPath = true;
		}
		
		m_ObstructedTiles.Clear();
	}
}
