using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

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
	public TileType type = TileType.Valid;

	public Transform[] _connections; //public array used for debuging, this way you can see the platform list in the editor
	public List<Tile> connections; //list of directly accessible platforms
	[HideInInspector] protected HashSet<Tile> connectionSet; //auxilliary hashset used to ignore duplicates
	[HideInInspector] protected HashSet<Tile> siblingConnection; //auxilliary hashset used for siblings detection

	public TileOrientation orientation;
	
	[HideInInspector] public bool rescanPath = false;// debug toggle used to force rescan of nearby platforms

	// #HIGHLIGHTING#

	[HideInInspector] public bool isClickable = false; // wheter this platform can be clicked or not
    private bool isHighlighted = false;//whether this platform is highlighted
    private bool isFlashing = false;//wheter this platform is flashing
    private Ticker flash;//timer for platform flashing

	private TileType oldType;// auxilliary variable

#if UNITY_EDITOR
	private Material[] sourceMaterials;
#endif

    // Use this for initialization
    void Start()
    {
		rescanPath = true;

        defineOrientation();
		applyTileMaterial();
		
#if UNITY_EDITOR

		GetComponent<Renderer>().enabled = true;

		/*
		if ( EditorApplication.isPlaying )
		{
			int i;
			Material[] materials;
			sourceMaterials = GetComponent<Renderer>().sharedMaterials;

			List<string> compares = new List<string> { "fill", "valid", "invalid", "exit" };

			for (i = 0; i < sourceMaterials.Length; i++)
			{
				if ( compares.Contains( sourceMaterials[i].name ))
					break;
			}

			if (gameObject.GetComponent<Stairway> () != null)
				return;
			
			materials = new Material[] {
				sourceMaterials[i]
			};
			
			GetComponent<Renderer>().sharedMaterials = materials;
		}
		*/
		
#elif UNITY_STANDALONE
		//GetComponent<Renderer>().enabled = false;

		/*
		Material[] materials = new Material[] {
			gameObject.GetComponent<Renderer>().sharedMaterials[0]
		};
		*/
		Material[] materials = new Material[] {
			Assets.getBlankBlockMat()
		};
		
		gameObject.GetComponent<Renderer>().materials = materials;
#endif
    }


    // Update is called once per frame
    protected void Update()
	{
		/*
#if UNITY_EDITOR
		// update the selected platform orientation...
        if (Selection.Contains(gameObject))
            defineOrientation();
#endif
*/
		if (rescanPath)
			scanNearbyTiles();

        if (!oldType.Equals(type)) // if the type was changed in scene mode, reapply the material
            applyTileMaterial();


        handleFlashing();
    }

    /// <summary>
    /// handles the flashing of the platform
    /// </summary>
    private void handleFlashing()
    {
        if (isFlashing)
        {
            if (flash.isOver())
            {
                unFlashMe();
            }
        }
    }

    /// <summary>
    /// Used to assert the platform orientation. Changes materials accordingly
    /// It was initially used for only 6 vectors, later I added some more, but you are supposed to use the default angles
    /// </summary>
    private void defineOrientation()
    {
#if UNITY_EDITOR
		Material[] materials = new Material[] {
			new Material(Shader.Find("Transparent/Diffuse")),
			gameObject.GetComponent<Renderer>().sharedMaterials[0]
		};
#endif
		
		if ( GetComponent<MeshFilter>().sharedMesh.name == "Quad" )
			transform.Rotate( new Vector3( -90, 0, 0 ) );
		
		Vector3 tileDirection = transform.rotation * -Vector3.up;
		
		if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Up) ), 0 ) )
		{
			orientation = TileOrientation.Up;
#if UNITY_EDITOR
			materials[1] = Assets.getUpBlockMat();
#endif
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Down) ), 0 ) )
		{
			orientation = TileOrientation.Down;
#if UNITY_EDITOR
			materials[1] = Assets.getDownBlockMat();
#endif
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Left) ), 0 ) )
		{
			orientation = TileOrientation.Right;
#if UNITY_EDITOR
			materials[1] = Assets.getRightBlockMat();
#endif
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Right) ), 0 ) )
		{
			orientation = TileOrientation.Left;
#if UNITY_EDITOR
			materials[1] = Assets.getLeftBlockMat();
#endif
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Front) ), 0 ) )
		{
			orientation = TileOrientation.Front;
#if UNITY_EDITOR
			materials[1] = Assets.getFrontBlockMat();
#endif
		}
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Back) ), 0 ) )
		{
			orientation = TileOrientation.Back;
#if UNITY_EDITOR
			materials[1] = Assets.getBackBlockMat();
#endif
		}

		if ( GetComponent<MeshFilter>().sharedMesh.name == "Quad" )
			transform.Rotate( new Vector3( 90, 0, 0 ) );

#if UNITY_EDITOR
		materials[1].SetFloat( "_Mode", 2 );
		materials[1].SetInt( "_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha );
		materials[1].SetInt( "_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha );
		materials[1].DisableKeyword( "_ALPHATEST_ON" );
		materials[1].EnableKeyword( "_ALPHABLEND_ON" );
		materials[1].DisableKeyword( "_ALPHAPREMULTIPLY_ON" );
		materials[1].renderQueue = 3000;

		Mesh sharedMesh = gameObject.GetComponent<MeshFilter> ().sharedMesh;
		sharedMesh.subMeshCount = 2;
		int[] tri = sharedMesh.GetTriangles (0);
		sharedMesh.SetTriangles (tri, 0);
		sharedMesh.SetTriangles (tri, 1);
		
		gameObject.GetComponent<Renderer>().materials = materials;
#endif
    }

    /// <summary>
    /// scans nearby platforms and puts them in the connections list, to later be used to calculates paths and so on.
    /// </summary>
    protected virtual void scanNearbyTiles()
	{
	    connectionSet = new HashSet<Tile>();

		if ( siblingConnection != null )
		{
			foreach ( Tile sibling in siblingConnection )
				connectionSet.Add( sibling );

			siblingConnection = null;
		}

		Collider[] hits = Physics.OverlapSphere(transform.position, 5.5f);

		Stack<Collider> hitList = new Stack<Collider> ();

		foreach( Collider hit in hits )
			hitList.Push (hit);

		while ( hitList.Count > 0 )
        {
			Collider hit = hitList.Pop();

			if ( hit.GetComponent<Collider>().transform != transform )
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

                Tile p = hit.gameObject.GetComponent<Tile>();

				if (p != null && p.orientation.Equals(orientation) )
                {
					if (rescanPath)
						p.rescanPath = true;
					
                    connectionSet.Add(p);

					_connections = new Transform[connectionSet.Count];
                    connections = new List<Tile>(connectionSet);
                    for (int i = 0; i != connections.Count; i++)
                    {
						_connections[i] = connections[i].transform;
                    }
                }
            }
        }
    }

	
	/// <summary>
	/// <para>Changes the platform material according to the characteristics of the platform.</para>
	/// <para>Although you can change the patforms materials in the editor this script will change all that.</para>
	/// <para>This is responsible for flashing, highlighting, which is also a change in materials.</para>
	/// </summary>
	private void applyTileMaterial()
	{
		//for some reason Unity doesn't let us change a single material, we have to change the material array
		Material[] materials = gameObject.GetComponent<Renderer>().sharedMaterials;
		switch (type)
		{
		case TileType.Valid:
			materials[0] = isHighlighted ? Assets.getHighlightedValidBlockMat() : Assets.getValidBlockMat();
			materials[0] = isFlashing ? Assets.getFlashingValidBlockMat() : materials[0];
			break;
		case TileType.Invalid:
			materials[0] = isHighlighted ? Assets.getHighlightedInvalidBlockMat() : Assets.getInvalidBlockMat();
			materials[0] = isFlashing ? Assets.getFlashingInvalidBlockMat() : materials[0];
			break;
		case TileType.Exit:
			materials[0] = isHighlighted ? Assets.getHighlightedExitBlockMat() : Assets.getExitBlockMat();
			materials[0] = isFlashing ? Assets.getFlashingExitBlockMat() : materials[0];
			break;
		}
		gameObject.GetComponent<Renderer>().materials = materials;
		oldType = type;
	}

	/// <summary>
	/// <para>Highlight the targeted material, using the "MouseCursor" prefab.</para>
	/// </summary>
	private void highlightTile()
	{
		if ( !isHighlighted )
		{
			Assets.mouseCursor.transform.position = Vector3.one * float.MaxValue;
			return;
		}
		//Vector3 cursorPosition = transform.position;
		Assets.mouseCursor.transform.position = transform.position;
		Assets.mouseCursor.transform.rotation = transform.rotation;

		if ( GetComponent<MeshFilter>().sharedMesh.name == "Quad" )
			Assets.mouseCursor.transform.Rotate( new Vector3( -90, 0, 0 ) );
		
		Assets.mouseCursor.transform.Translate (new Vector3 (0, 0.5f, 0));

		/*
		switch (type)
		{
		case TileType.Valid:
			break;
		case TileType.Invalid:
			materials[0] = isHighlighted ? Assets.getHighlightedInvalidBlockMat() : Assets.getInvalidBlockMat();
			materials[0] = isFlashing ? Assets.getFlashingInvalidBlockMat() : materials[0];
			break;
		case TileType.Exit:
			materials[0] = isHighlighted ? Assets.getHighlightedExitBlockMat() : Assets.getExitBlockMat();
			materials[0] = isFlashing ? Assets.getFlashingExitBlockMat() : materials[0];
			break;
		}
		gameObject.GetComponent<Renderer>().materials = materials;
		*/
		oldType = type;
	}

    // this function is called whenever we want to highlight a platform, usually due to mousehover
    public void highlight()
    {
		if ( isClickable )
		{
	        isHighlighted = true;
			highlightTile();

	        //applyTileMaterial();
		}
    }

    // this function is called whenever we want to unhighlight a platform, usually right after a mousehover
    public void unHighlight()
    {
		isHighlighted = false;
		highlightTile();
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
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(this);

        while (queue.Count != 0)
        {
            Tile platform = queue.Dequeue();
            if (!platformsFound.Contains(platform))
            {
                platformsFound.Add(platform);
                foreach (Tile brotherTile in platform.connections)
                {
                    queue.Enqueue(brotherTile);
                }
            }
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
        Tile closestNode = inNodes[0];
        float minDist = float.MaxValue;
        for (int i = 0; i < inNodes.Count; i++)
        {
            if (AStarHelper.Invalid(inNodes[i]))
                continue;

            float thisDist = Vector3.Distance(toPoint, inNodes[i].Position);

            if (thisDist > minDist)
                continue;

			if ( Vector3.Distance( inNodes[i].Position, TileSelection.PlayerPosition ) > Vector3.Distance( closestNode.Position, TileSelection.PlayerPosition ) )
				continue;

            minDist = thisDist;
			Debug.Log( "new min: " + minDist );
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
		get { return (this == null || this.type.Equals(TileType.Invalid)); }
    }

    public Vector3 getTargetPoint()
    {
        return transform.position;
    }

	public void CheckTileOrientation()
	{
		defineOrientation ();
	}
}
