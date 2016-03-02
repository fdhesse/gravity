using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameplayCube))]
[RequireComponent(typeof(AudioSource))]
public class FallingCube : MonoBehaviour
{
	public bool isInit;
	public bool isFalling;
	
	private Tile[] tiles;
	private Dictionary<Tile, TileType> obstructedTiles;

	private FallingCubeBody body;

	void Awake()
	{
		GameObject bodyGo = new GameObject ("rigidbody (falling cube)");
		bodyGo.tag = gameObject.tag;
		bodyGo.layer = gameObject.layer;
		bodyGo.transform.position = transform.position;
		bodyGo.transform.rotation = transform.rotation;
		bodyGo.transform.localScale = transform.lossyScale;

		bodyGo.AddComponent<BoxCollider> ();
		bodyGo.AddComponent<Rigidbody> ();
		body = bodyGo.AddComponent<FallingCubeBody> ();
		body.LegacyParent = this;

		isInit = true;
	}
	
	// Use this for initialization
	void Start ()
	{
		obstructedTiles = new Dictionary<Tile, TileType> ();

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

	public void Reset (TileOrientation startingOrientation)
	{
		if (!isInit)
			Awake ();

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
					
					obstructedTiles.Add( tile, tile.Type );
					tile.Type = TileType.Invalid;
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
			tile.Type = entry.Value;
			tile.rescanPath = true;
		}
		
		obstructedTiles.Clear ();
	}
}
