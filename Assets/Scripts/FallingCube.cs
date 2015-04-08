using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameplayCube))]
public class FallingCube : MonoBehaviour
{
	public bool isFalling;
	
	private Tile[] tiles;
	private Dictionary<Tile, TileType> obstructedTiles;

	private FallingCubeBody body;

	void Awake()
	{
		GameObject bodyGo = new GameObject ("rigidbody");
		bodyGo.tag = gameObject.tag;
		bodyGo.layer = gameObject.layer;
		bodyGo.transform.position = transform.position;
		bodyGo.transform.rotation = transform.rotation;
		bodyGo.transform.localScale = transform.lossyScale;

		bodyGo.AddComponent<BoxCollider> ();
		bodyGo.AddComponent<Rigidbody> ();
		body = bodyGo.AddComponent<FallingCubeBody> ();
		body.LegacyParent = this;

	}
	
	// Use this for initialization
	void Start ()
	{
		obstructedTiles = new Dictionary<Tile, TileType> ();

		GameplayCube cube = GetComponent<GameplayCube>();
		
		cube.Left = TileType.Valid;
		cube.Right = TileType.Valid;
		cube.Up = TileType.Valid;
		cube.Down = TileType.Valid;
		cube.Front = TileType.Valid;
		cube.Back = TileType.Valid;
		
		// Change the Colliders in order to make boxColliders;
		GameObject[] faces = new GameObject[6];
		tiles = new Tile[6];
		
		faces[0] = transform.FindChild ("left").gameObject;
		faces[1] = transform.FindChild ("right").gameObject;
		faces[2] = transform.FindChild ("up").gameObject;
		faces[3] = transform.FindChild ("down").gameObject;
		faces[4] = transform.FindChild ("back").gameObject;
		faces[5] = transform.FindChild ("front").gameObject;
		
		for ( int i = 0, l = faces.Length; i < l; i++ )
		{
			tiles[i] = faces[i].GetComponent<Tile>();
			tiles[i].rescanPath = true;

			if ( !tiles[i].isQuad )
			{
				Object.DestroyImmediate( faces[i].GetComponent<MeshCollider> () );
				
				BoxCollider box = faces[i].AddComponent<BoxCollider> ();
				box.isTrigger = true;
			}
		}
	}

	public void Reset ()
	{
		body.Reset ();

		//isDestroyed = false;
	}

	void OnRigidbodyCollisionEnter(Collision collision)
	{
		// ADD A Control to avoid player change physic too soon
		// in order to avoir "gluing effect"

		if (collision.relativeVelocity.magnitude > 2)
			GetComponent<AudioSource>().Play();
		
		// Detect all the obstructed tiles
		foreach ( TileOrientation orientation in System.Enum.GetValues( typeof( TileOrientation ) ) )
		{
			RaycastHit[] hitInfos = Physics.RaycastAll( transform.position, World.getGravityVector( orientation ), transform.localScale.x * 0.55f, 1 << LayerMask.NameToLayer( "Tiles" ) );
			
			foreach ( RaycastHit hitInfo in hitInfos )
			{
				if ( !hitInfo.collider.transform.IsChildOf( transform ) )
				{
					Tile tile = hitInfo.collider.GetComponent<Tile>();
					
					if ( obstructedTiles.ContainsKey( tile ) )
						continue;
					
					obstructedTiles.Add( tile, tile.type );
					tile.type = TileType.Invalid;
					tile.rescanPath = true;
				}
			}
		}
		
		foreach (Tile tile in tiles)
		{
			tile.rescanPath = true;
			
			foreach ( Tile t in tile.connections )
				t.rescanPath = true;
		}
	}
	
	void OnRigidbodyCollisionExit( Collision collision )
	{
		if (collision.gameObject.tag == "Player")
			return;
		
		foreach ( KeyValuePair<Tile, TileType > entry in obstructedTiles )
		{
			Tile tile = entry.Key;
			tile.type = entry.Value;
			tile.rescanPath = true;
		}
		
		obstructedTiles.Clear ();
	}
}
