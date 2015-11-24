using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
//[RequireComponent(typeof(Rigidbody))]
public class GravityPlatform : MonoBehaviour {
	
/*	[HideInInspector] [SerializeField] private PlatformType m_left;
	[HideInInspector] [SerializeField] private PlatformType m_right;
	[HideInInspector] [SerializeField] private PlatformType m_up;
	[HideInInspector] [SerializeField] private PlatformType m_down;
	[HideInInspector] [SerializeField] private PlatformType m_front;
	[HideInInspector] [SerializeField] private PlatformType m_back;
	
	[ExposeProperty]
	public PlatformType Left
	{
		get { return m_left; }
		set	{ if(value != m_left) { SetFace( "left", value ); m_left = value; } }
//		set	{ if(value != m_left) { __needsUpdate = true; m_left = value; } }
	}
	[ExposeProperty]
	public PlatformType Right
	{
		get { return m_right; }
		set	{ if(value != m_right) { SetFace( "right", value ); m_right = value; } }
//		set	{ if(value != m_left) { __needsUpdate = true; m_right = value; } }
	}
	[ExposeProperty]
	public PlatformType Up
	{
		get { return m_up; }
		set	{ if(value != m_up) { SetFace( "up", value ); m_up = value; } }
//		set	{ if(value != m_up) { __needsUpdate = true; m_up = value; } }
	}
	[ExposeProperty]
	public PlatformType Down
	{
		get { return m_down; }
		set	{ if(value != m_down) { SetFace( "down", value ); m_down = value; } }
//		set	{ if(value != m_down) { __needsUpdate = true; m_down = value; } }
	}
	[ExposeProperty]
	public PlatformType Front
	{
		get { return m_front; }
		set	{ if(value != m_front) { SetFace( "front", value ); m_front = value; } }
//		set	{ if(value != m_front) { __needsUpdate = true; m_front = value; } }
	}
	[ExposeProperty]
	public PlatformType Back
	{
		get { return m_back; }
		set	{ if(value != m_back) { SetFace( "back", value ); m_back = value; } }
	}
*/	
	public enum ConstraintAxis { X, Y, Z };

	public float from = 0;
	public float to = 10;

	public ConstraintAxis constrainedAxis;

	private float startPos = 0;
	private bool freezed = false;
	
	private GravityPlatformBody body;
	public bool isFalling;
	public bool isInit;

	private Dictionary<Tile, TileType> obstructedTiles;

	private float Position
	{
		get {
			if( constrainedAxis == ConstraintAxis.X )
				return transform.position.x;
			else if( constrainedAxis == ConstraintAxis.Y )
				return transform.position.y;
			else if ( constrainedAxis == ConstraintAxis.Z )
				return transform.position.z;

			return 0f;
			}
		set	{
			if( constrainedAxis == ConstraintAxis.X )
			{
				transform.position = new Vector3( value, transform.position.y, transform.position.z );
				body.transform.position = transform.position;
			}
			else if( constrainedAxis == ConstraintAxis.Y )
			{
				transform.position = new Vector3( transform.position.x, value, transform.position.z );
				body.transform.position = transform.position;
			}
			else if ( constrainedAxis == ConstraintAxis.Z )
			{
				transform.position = new Vector3( transform.position.x, transform.position.y, value );
				body.transform.position = transform.position;
			}
		}
	}

	// Use this for initialization
	void Awake ()
	{
		obstructedTiles = new Dictionary<Tile, TileType> ();

		GameObject bodyGo = new GameObject ("rigidbody (gravity platform)");
		bodyGo.tag = gameObject.tag;
		bodyGo.layer = gameObject.layer;
		bodyGo.transform.position = transform.position;
		bodyGo.transform.rotation = transform.rotation;
		bodyGo.transform.localScale = transform.lossyScale;
		
		bodyGo.AddComponent<BoxCollider> ();
		bodyGo.AddComponent<Rigidbody> ();
		body = bodyGo.AddComponent<GravityPlatformBody> ();
		body.LegacyParent = this;
		
		isInit = true;

		startPos = from;

		if ( from > to )
		{
			float swap = from;

			from = to;
			to = swap;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		//transform.position = body.transform.position;

		if ( !freezed && (Position < from || Position > to ) )
			Freeze();
	}

	public void Reset(TileOrientation startingOrientation)
	{
		if (!isInit)
			Awake ();
		
		body.Reset ();
		
		Position = startPos;
		
		Freeze ();
		Unfreeze( startingOrientation );

		/*
		GetComponent<Rigidbody> ().isKinematic = false;
		position = startPos;
		
		Freeze ();
		*/
	}

	public void Freeze()
	{
		body.Freeze();

		if (Position > to)
			Position = to - 0.001f;
		else if (Position < from)
			Position = from + 0.001f;
		
		freezed = true;
		
		Tile[] childrenPlatforms = GetComponentsInChildren<Tile> ();
		
		for ( int i = 0, l = childrenPlatforms.Length; i < l; i++ )
			childrenPlatforms[i].rescanPath = true;
	}

	public void Unfreeze( TileOrientation orientation )
	{
		freezed = body.Unfreeze( orientation );
	}
	
	void OnRigidbodyCollisionEnter(Collision collision)
	{
		// ADD A Control to avoid player change physic too soon
		// in order to avoir "gluing effect"
		
		//if (collision.relativeVelocity.magnitude > 2)
		//	GetComponent<AudioSource>().Play();
		
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
