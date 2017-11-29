using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// <para>Enum used to identify and register Platform types.</para>
/// <para>A Platform can be of type Valid, Invalid or Exit</para>
/// </summary>
public enum TileType
{
	Invalid = 0,
	Valid,
	Exit,
	Spikes,
	None
}

/// <summary>
/// Enum that stores the types of orientations
/// </summary>
public enum TileOrientation
{
	None = 0,
	Up,
	Down,
	Left,
	Right,
	Front,
	Back
}

/// <summary>
///  Class Responsible for interaction with Tiles.
///  Each platform holds the directly accessible platforms, via the List connections.
///  For pathfinding purposes it inherits IPathNode.
/// </summary>
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Tile : MonoBehaviour, IPathNode<Tile>
{
	[SerializeField]
	private TileType type = TileType.Valid;
	public TileType Type
	{
		get { return this.type; }
		set	{ this.type = value; }
	}

	/// <summary>
	/// Tell if this tile is glued. This field must be serialized, because it is set by the editor during
	/// the edition (when changing the glue side of the gameplay cube), but it must be loaded in game.
	/// </summary>
	[SerializeField]
	private bool isGlueTile = false;
	public bool IsGlueTile
	{
		get	{ return isGlueTile; }
		set	{ isGlueTile = value; }
	}

	private bool isClickableToChangeGravity = false;
	public bool IsClickableToChangeGravity
	{
		get	{ return isClickableToChangeGravity; }
		set
		{
			// set the flag to myself
			isClickableToChangeGravity = value;

			// set the flag to all the mesh tile children
			if (this.transform.childCount > 0)
			{
				Transform meshGameObject = this.transform.GetChild(0);
				
				// recheck the orientation of the mesh tiles
				for (int i = 0; i < meshGameObject.transform.childCount; ++i)
				{
					// get the gold tile component in the children which has it
					GoldTile goldTileChild = meshGameObject.transform.GetChild(i).GetComponent<GoldTile>();
					if (goldTileChild != null)
						goldTileChild.IsClickableToChangeGravity = value;
				}
			}
		}
	}

	[HideInInspector] public TileOrientation orientation;
	
	public List<Tile> connections = null; //list of directly accessible platforms
	protected HashSet<Tile> siblingConnection = null; //auxilliary hashset used for siblings detection

	public bool rescanPath = true;// debug toggle used to force rescan of nearby platforms
	#if UNITY_EDITOR
	private bool isRescanPathDoneThisFrame = false;
	#endif

	// #HIGHLIGHTING#

    private bool isHighlighted = false;//whether this platform is highlighted
    private bool isFlashing = false;//wheter this platform is flashing
    private Ticker flash;//timer for platform flashing

	public bool IsHighlighted
	{
		get { return isHighlighted; }
	}

    // Use this for initialization
    void Awake()
	{
		if ( GetComponent<Stairway>() == null )
			gameObject.hideFlags = HideFlags.NotEditable;
		else
			gameObject.hideFlags = 0;

		connections = new List<Tile>();
		rescanPath = true;
	
        defineOrientation();

		// Temp code, destroy the old "graphics" child object
		Transform graphics = transform.Find("graphics");
		if ( graphics != null )
			DestroyImmediate( graphics.gameObject );	
    }

    // Update is called once per frame
    protected void Update()
	{
		// NearbyTiles changed, need a rescan
		if (rescanPath)
			scanNearbyTiles();

		/// handles the flashing of the platform
		if (isFlashing && flash.isOver())
			unFlashMe();
	}
	
	void OnCollisionEnter( Collision collision )
	{
		//if (collision.collider.gameObject.tag != "Player" || orientation != World.Pawn.orientation )
		if (collision.collider.gameObject.tag != "Player" )
			return;

		// warn the pawn that he enter on my tile
		Pawn.Instance.OnEnterTile(this);
	}

    /// <summary>
    /// Used to assert the platform orientation. Changes materials accordingly
    /// It was initially used for only 6 vectors, later I added some more, but you are supposed to use the default angles
    /// </summary>
    private void defineOrientation()
	{
		if (GetComponent<Stairway> () != null )
		{
			GetComponent<Stairway> ().hideFlags = HideFlags.NotEditable;
			hideFlags = HideFlags.NotEditable;
			orientation = TileOrientation.None;
			return;
		}

		Vector3 tileDirection = transform.rotation * Quaternion.Euler( -90f, 0, 0 ) * -Vector3.up;

		if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.GetGravityNormalizedVector(TileOrientation.Up) ), 0 ) )
		{
			orientation = TileOrientation.Up;
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.GetGravityNormalizedVector(TileOrientation.Down) ), 0 ) )
		{
			orientation = TileOrientation.Down;
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.GetGravityNormalizedVector(TileOrientation.Right) ), 0 ) )
		{
			orientation = TileOrientation.Right;
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.GetGravityNormalizedVector(TileOrientation.Left) ), 0 ) )
		{
			orientation = TileOrientation.Left;
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.GetGravityNormalizedVector(TileOrientation.Front) ), 0 ) )
		{
			orientation = TileOrientation.Front;
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.GetGravityNormalizedVector(TileOrientation.Back) ), 0 ) )
		{
			orientation = TileOrientation.Back;
		}
    }

    /// <summary>
    /// scans nearby platforms and puts them in the connections list, to later be used to calculates paths and so on.
    /// </summary>
    protected virtual void scanNearbyTiles()
	{
		// by default clear the flag since we rescan it
		rescanPath = false;

		#if UNITY_EDITOR
		// set the flag for debug draw
		isRescanPathDoneThisFrame = true;
		#endif

		HashSet<Tile> connectionSet = new HashSet<Tile>();

		if ( siblingConnection != null )
		{
			foreach ( Tile sibling in siblingConnection )
				if (sibling != null)
					connectionSet.Add( sibling );

			siblingConnection = null;
		}

		Collider[] hits = Physics.OverlapSphere(transform.position, GameplayCube.HALF_CUBE_SIZE * 1.1f);
		Stack<Collider> hitList = new Stack<Collider>(hits);
		
		while ( hitList.Count > 0 )
        {
			Collider hit = hitList.Pop();

			// check that I didn't collided with a tile of the same gameplay cube (same parent)
			if ( hit.transform.parent != transform.parent )
            {
				// Si il s'agit d'un escalier
				Stairway stair = hit.gameObject.GetComponent<Stairway>();

				if( stair != null )
				{
					Tile siblingTile = stair.LookForSiblingTile( this );

					if ( siblingTile != null )
					{
						// Jumelle trouvée, la plateforme est ajoutée
						// à la liste de la plateforme
						hitList.Push( siblingTile.GetComponent<Collider>() );

						if ( siblingTile.siblingConnection == null )
							siblingTile.siblingConnection = new HashSet<Tile>();

						if ( !siblingTile.siblingConnection.Contains( this ) )
							siblingTile.siblingConnection.Add( this );
					}

					continue;
				}

                Tile neighbourgTile = hit.gameObject.GetComponent<Tile>();

				if (neighbourgTile != null && neighbourgTile.orientation.Equals(orientation) && TileSelection.isClickableType( neighbourgTile.type ) )
                {
					//if (rescanPath)
					connectionSet.Add(neighbourgTile);
                }
            }
		}

		// Check if there is any difference
		// a flag to tell if we have the same list or not finally
		bool areListsDifferents = false;

		// Rescan previous connections
		foreach( Tile connectedTile in connections )
			if (!connectionSet.Contains(connectedTile))
			{
				if (connectedTile != null)
				{
					// we found a tile that was in the old list, but not in the new one
					// remove myself from the connection list of the old neighbors
					// (sometime the sphere collision test is not reciprocal, 
					// so this will converge, when both collision test can detect each other)
					if (connectedTile.connections != null)
						connectedTile.connections.Remove(this);
					// and ask the neighboor to scan again to be sure that he cannot find me anymore
					connectedTile.rescanPath = true;
				}
				areListsDifferents = true;
			}

		// Rescan new connections too
		foreach( Tile connectedTile in connectionSet )
			if (!connections.Contains(connectedTile))
			{
				// we found a tile that is in the new list, but was not in the old one
				// add myself to the connection list of the new tile
				if (connectedTile.connections == null)
					connectedTile.connections = new List<Tile>(1);
				connectedTile.connections.Add(this);
				// and ask the neighboor to scan again to be sure that he can find me
				// (sometime the sphere collision test is not reciprocal, 
				// so this will converge, when both collision test can detect each other)
				connectedTile.rescanPath = true;
				areListsDifferents = true;
			}

		if (areListsDifferents)
			connections = new List<Tile>(connectionSet);
    }

	/// <summary>
	/// <para>Highlight the targeted material, using the "MouseCursor" prefab.</para>
	/// </summary>
	private void highlightTile()
	{
		if ( !isHighlighted )
		{
			Assets.mouseCursor.transform.parent = null;
			Assets.mouseCursor.transform.position = Vector3.one * float.MaxValue;
			return;
		}
		Assets.mouseCursor.transform.position = transform.position;
		Assets.mouseCursor.transform.rotation = transform.rotation;

		Assets.mouseCursor.transform.Rotate( new Vector3( -90, 0, 0 ) );
		
		Assets.mouseCursor.transform.Translate (new Vector3(0, GameplayCube.CUBE_SIZE * 0.05f, 0));
		Assets.mouseCursor.transform.parent = transform;
	}

    // this function is called whenever we want to highlight a platform, usually due to mousehover
    public void highlight()
    {
        isHighlighted = true;
		highlightTile();
    }

    // this function is called whenever we want to unhighlight a platform, usually right after a mousehover
    public void unHighlight()
    {
		isHighlighted = false;
		highlightTile();
    }

	/// <summary>
	/// Call this function if you want to play the VFX for when the tile is activated
	/// </summary>
	
	public void playActivationVFX()
	{
		if (this.transform.childCount > 0)
		{
			Transform meshGameObject = this.transform.GetChild(0);

			// recheck the orientation of the mesh tiles
			for (int i = 0; i < meshGameObject.transform.childCount; ++i)
			{
				// get the gold tile component in the children which has it
				GoldTile goldTileChild = meshGameObject.transform.GetChild(i).GetComponent<GoldTile>();
				if (goldTileChild != null)
					goldTileChild.playActivationVFX();
			}
		}
	}
	
    /// <summary>
    /// starts the flash for this platform
    /// </summary>
    public void flashMe()
    {
        isFlashing = true;
		flash = new Ticker(0.1f, false);
		highlightTile();
    }

    /// <summary>
    /// stops the flash for this platform
    /// </summary>
    public void unFlashMe()
    {
		isFlashing = false;
		highlightTile();
    }

    /// <summary>
    /// Gets all accessible platforms.
    /// These are the platforms that are reachable via traversing the platforms saved in the connections list.
    /// </summary>
    public List<Tile> AllAccessibleTiles()
    {
        List<Tile> platformsFound = new List<Tile>();
		platformsFound.Add (this);

		for (int i = 0 ; i < platformsFound.Count; i++)
		{
			Tile platform = platformsFound[i];
			foreach (Tile brotherTile in platform.connections)
				if (!platformsFound.Contains(brotherTile))
					platformsFound.Add(brotherTile);
		}

        return platformsFound;
    }

    /// <summary>
    /// Auxilliary method used to get the node in inNodes closest to the position of the reference Point.
    /// </summary>
    /// <param name="inNodes">Tiles we want to search</param>
    /// <param name="toPoint">Reference point</param>
    /// <returns></returns>
    public static Tile Closest(List<Tile> inNodes, Vector3 toPoint)
    {
		// get the first tile by default (so if all tiles are invalid we return the first one anyway)
        Tile closestNode = inNodes[0];
		float minDistToPoint = float.MaxValue;
		float minDistToPlayer = float.MaxValue;

		// iterate on all tiles to check if the first one is invalid
        for (int i = 0; i < inNodes.Count; i++)
        {
            if (AStarHelper.Invalid(inNodes[i]))
                continue;

			// compute and check if the distance of the current tile to the specified point is longer
            float currentDistToPoint = Vector3.Distance(toPoint, inNodes[i].Position);
            if (currentDistToPoint > minDistToPoint)
                continue;

			// compute and check if the distance of the current tile to the player is longer
			float currentDistToPlayer = Vector3.Distance( inNodes[i].Position, TileSelection.PlayerPosition );
			if ( currentDistToPlayer > minDistToPlayer )
				continue;

            minDistToPoint = currentDistToPoint;
			minDistToPlayer = currentDistToPlayer;
			closestNode = inNodes[i];
        }

        return closestNode;
    }


    /// - IPATHNODE.CS
    public List<Tile> Connections
    {
        get { return connections; }
    }
    
    public Vector3 Position
    {
        get { return transform.position; }
    }
    
    public bool Invalid
    {
		get { return this.type.Equals(TileType.Invalid); }
    }

    public Vector3 getTargetPoint()
    {
        return transform.position;
    }

	public Vector3 getDownVector()	
	{
		return transform.forward;
	}

	public void CheckTileOrientation()
	{
		// define the orientation of this tile
		defineOrientation();

		// update also the orientation of my children mesh tile
		if ((Pawn.Instance != null) && (World.Instance != null))
			updateMeshTileOrientation(World.Instance.CurrentGravityOrientation);
	}

	private void updateMeshTileOrientation(TileOrientation worldGravityOrientation)
	{
		// get the mesh child (if already created)
		if (this.transform.childCount > 0)
		{
			Transform meshGameObject = this.transform.GetChild(0);

			// recheck the orientation of the mesh tiles
			for (int i = 0; i < meshGameObject.transform.childCount; ++i)
			{
				// get the gold tile component in the children which has it
				GoldTile goldTileChild = meshGameObject.transform.GetChild(i).GetComponent<GoldTile>();
				if (goldTileChild != null)
					goldTileChild.UpdateOrientation(worldGravityOrientation);
			}
		}
	}

	#if UNITY_EDITOR
	/// <summary>
	/// Creates the tile mesh corresponding to the specified tile type and using some prefabs for each types.
	/// </summary>
	/// <param name="forceUpdate">If true, the update will be forced, even if the tile type didn't changed.</param>
	public void updateTileMesh(bool forceUpdate)
	{
		// get the prefab name of the correct prefab to spawn
		string new_mesh_name = string.Empty;
		switch (this.type)
		{
		case TileType.Valid:
			if (this.gameObject.CompareTag(GameplayCube.FALLING_CUBE_TAG))
			{
				if (isGlueTile)
					new_mesh_name = "tile_glue_falling_cube";
				else
					new_mesh_name = "tile_walk_falling_cube";
			}
			else
			{
				if (isGlueTile)
					new_mesh_name = "tile_glue";
				else
					new_mesh_name = "tile_walk";
			}
			break;
		case TileType.Spikes:
			new_mesh_name = "tile_spikes";
			break;
		case TileType.Exit:
			new_mesh_name = "tile_exit";
			break;
		default:
			// for all other types, leave the empty sting, 
			// but continue to delete the eventual old mesh
			break;
		}
	
		// first check if an existing mesh is already present, that we may need to destroy
		if (transform.childCount > 0)
		{
			// if the existing mesh is different, delete it to create the new mesh, otherwise early exit
			Transform previousMeshTransform = transform.GetChild(0);
			if (forceUpdate || (previousMeshTransform.name != new_mesh_name))
				DestroyImmediate(previousMeshTransform.gameObject);
			else
				return;
		}

		// Now check if we need to create a new mesh, otherwise early exit
		if (new_mesh_name == string.Empty)
			return;

		// instanttiate the new mesh and set its name
		GameObject mesh = (GameObject) GameObject.Instantiate( Resources.Load( "PREFABS/" + new_mesh_name ) );
		mesh.name = new_mesh_name;

		// make the mesh uneditable
		mesh.hideFlags = HideFlags.NotEditable;
		for (int i = 0; i < mesh.transform.childCount; ++i)
			mesh.transform.GetChild(i).gameObject.hideFlags = HideFlags.NotEditable;
		
		// attach the mesh to that tile
		mesh.transform.parent = this.transform;
		// set the local position and rotation
		mesh.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		mesh.transform.localPosition = new Vector3( 0.5f, -0.5f, 0 );

		// after setting the new mesh, update the orientation of the mesh tile, giving any kind
		// of orientation, it doesn't matter, cause we are in edit mode, it's just to set the editor material
		updateMeshTileOrientation(TileOrientation.Up);
	}

	public void setStaticFlag(bool isStatic)
	{
		// set the static flag for the tile gameobject
		// (it's not really necessary, it's more important for the mesh tile, but it's for the beauty of uniformity)
		this.gameObject.isStatic = isStatic;

		// now more important, set the static flag for all the children mesh objects
		for (int i = 0; i < this.transform.childCount; ++i)
		{
			// set it for the top mesh gameobject
			Transform child = this.transform.GetChild(i);
			child.gameObject.isStatic = isStatic;
			// and also more important for the grandchildren
			for (int j = 0; j < child.transform.childCount; ++j)
				child.transform.GetChild(j).gameObject.isStatic = isStatic;
		}
	}

	public void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			// draw the path scanning while playing
			if (isRescanPathDoneThisFrame)
			{
				isRescanPathDoneThisFrame = false;
				Gizmos.DrawWireSphere(transform.position, 6.0f);
			}
		}
		else
		{
			Vector3 scale = this.transform.rotation * new Vector3(GameplayCube.CUBE_SIZE, GameplayCube.CUBE_SIZE, 0.1f);

			// if not playing draw some faces, for the invisible meshes
			switch (this.type)
			{
			case TileType.Invalid:
				Gizmos.color = new Color(1, 0, 0, 0.7f);
				Gizmos.DrawCube(this.transform.position, scale);
				break;
			}
		}
	}
	#endif
}
