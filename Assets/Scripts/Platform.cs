using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

/// <summary>
///  Class Responsible for interaction with Platforms.
///  Each platform holds the directly accessible platforms, via the List connections.
///  For pathfinding purposes it inherits IPathNode.
/// </summary>
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Platform : MonoBehaviour, IPathNode<Platform>
{
	public PlatformType type = PlatformType.Valid;

	public Transform[] _connections; //public array used for debuging, this way you can see the platform list in the editor
	public List<Platform> connections; //list of directly accessible platforms
	[HideInInspector] protected HashSet<Platform> connectionSet; //auxilliary hashset used to ignore duplicates
	[HideInInspector] protected HashSet<Platform> siblingConnection; //auxilliary hashset used for siblings detection

	[HideInInspector] public PlatformOrientation orientation;
	
	[HideInInspector] public bool rescanPath = false;// debug toggle used to force rescan of nearby platforms

	// #HIGHLIGHTING#

	[HideInInspector] public bool isClickable = false; // wheter this platform can be clicked or not
    private bool isHighlighted = false;//whether this platform is highlighted
    private bool isFlashing = false;//wheter this platform is flashing
    private Ticker flash;//timer for platform flashing

	private PlatformType oldType;// auxilliary variable
	
#if UNITY_EDITOR
	private Material[] sourceMaterials;
#endif

    // Use this for initialization
    void Start()
    {
		rescanPath = true;

        defineOrientation();
		applyPlatformMaterial();
		
#if UNITY_EDITOR

		GetComponent<Renderer>().enabled = true;

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
		
#elif UNITY_STANDALONE
		//GetComponent<Renderer>().enabled = false;

		/*
		Material[] materials = new Material[] {
			gameObject.renderer.sharedMaterials[0]
		};
		*/
		Material[] materials = new Material[] {
			Assets.getBlankBlockMat()
		};
		
		gameObject.renderer.materials = materials;
#endif
    }


    // Update is called once per frame
    protected void Update()
	{
#if UNITY_EDITOR
		// update the selected platform orientation...
        if (Selection.Contains(gameObject))
            defineOrientation();
#endif
		if (rescanPath)
			scanNearbyPlatforms();

        if (!oldType.Equals(type)) // if the type was changed in scene mode, reapply the material
            applyPlatformMaterial();


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

//		Material[] materials = gameObject.renderer.sharedMaterials;

		Material[] materials = new Material[] {
			new Material(Shader.Find("Transparent/Diffuse")),
			gameObject.GetComponent<Renderer>().sharedMaterials[0]
		};
		
		string r = transform.rotation.eulerAngles.ToString();
		//Debug.Log(r);
		switch (r)
		{
		case "(0.0, 180.0, 0.0)":
			orientation = PlatformOrientation.Up;
			materials[1] = Assets.getUpBlockMat();
			break;
		case "(0.0, 0.0, -180.0)":
			orientation = PlatformOrientation.Down;
			materials[1] = Assets.getDownBlockMat();
			break;
		case "(90.0, 90.0, 0.0)":
			orientation = PlatformOrientation.Left;
			materials[1] = Assets.getLeftBlockMat();
			break;
		case "(90.0, 270.0, 0.0)":
			orientation = PlatformOrientation.Right;
			materials[1] = Assets.getRightBlockMat();
			break;
		case "(90.0, 180.0, 0.0)":
			orientation = PlatformOrientation.Front;
			materials[1] = Assets.getFrontBlockMat();
			break;
		case "(90.0, 0.0, 0.0)":
			orientation = PlatformOrientation.Back;
			materials[1] = Assets.getBackBlockMat();
			break;
		default:
			//                Debug.LogError("A block didn't update its orientation correctly, this is because its rotations is funky or not registered, rotation:" + r);
			break;
		}
		
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
        
    }

    /// <summary>
    /// scans nearby platforms and puts them in the connections list, to later be used to calculates paths and so on.
    /// </summary>
    protected virtual void scanNearbyPlatforms()
	{
	    connectionSet = new HashSet<Platform>();

		if ( siblingConnection != null )
		{
			foreach ( Platform sibling in siblingConnection )
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
					Platform siblingPlatform = stair.LookForSiblingPlatform( this );

					if ( siblingPlatform != null )
					{
						// Jumelle trouvée, la plateforme est ajoutée
						// à la liste de la plateforme
						hitList.Push( siblingPlatform.GetComponent<Collider>() );

						if ( siblingPlatform.siblingConnection == null )
							siblingPlatform.siblingConnection = new HashSet<Platform>();

						if ( !siblingPlatform.siblingConnection.Contains( this ) )
							siblingPlatform.siblingConnection.Add( this );
					}

					continue;
				}

                Platform p = hit.gameObject.GetComponent<Platform>();

				if (p != null && p.orientation.Equals(orientation) )
                {
					if (rescanPath)
						p.rescanPath = true;
					
                    connectionSet.Add(p);

					_connections = new Transform[connectionSet.Count];
                    connections = new List<Platform>(connectionSet);
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
    private void applyPlatformMaterial()
    {
        //for some reason Unity doesn't let us change a single material, we have to change the material array
        Material[] materials = gameObject.GetComponent<Renderer>().sharedMaterials;
        switch (type)
        {
		case PlatformType.Valid:
				materials[0] = isHighlighted ? Assets.getHighlightedValidBlockMat() : Assets.getValidBlockMat();
                materials[0] = isFlashing ? Assets.getFlashingValidBlockMat() : materials[0];
                break;
		case PlatformType.Invalid:
				materials[0] = isHighlighted ? Assets.getHighlightedInvalidBlockMat() : Assets.getInvalidBlockMat();
                materials[0] = isFlashing ? Assets.getFlashingInvalidBlockMat() : materials[0];
                break;
		case PlatformType.Exit:
				materials[0] = isHighlighted ? Assets.getHighlightedExitBlockMat() : Assets.getExitBlockMat();
                materials[0] = isFlashing ? Assets.getFlashingExitBlockMat() : materials[0];
                break;
        }
        gameObject.GetComponent<Renderer>().materials = materials;
		oldType = type;
    }

    // this function is called whenever we want to highlight a platform, usually due to mousehover
    public void highlight()
    {
		if ( isClickable )
		{
	        isHighlighted = true;
	        applyPlatformMaterial();
		}
    }

    // this function is called whenever we want to unhighlight a platform, usually right after a mousehover
    public void unHighlight()
    {
        isHighlighted = false;
        applyPlatformMaterial();
    }

    /// <summary>
    /// starts the flash for this platform
    /// </summary>
    public void flashMe()
    {
        isFlashing = true;
        flash = new Ticker(0.1f, false);
        applyPlatformMaterial();
    }

    /// <summary>
    /// stops the flash for this platform
    /// </summary>
    public void unFlashMe()
    {
        isFlashing = false;
        applyPlatformMaterial();
    }

    /// <summary>
    /// Gets all accessible platforms.
    /// These are the platforms that are reachable via traversing the platforms saved in the connections list.
    /// </summary>
    public List<Platform> AllAccessiblePlatforms()
    {
        List<Platform> platformsFound = new List<Platform>();
        Queue<Platform> queue = new Queue<Platform>();
        queue.Enqueue(this);

        while (queue.Count != 0)
        {
            Platform platform = queue.Dequeue();
            if (!platformsFound.Contains(platform))
            {
                platformsFound.Add(platform);
                foreach (Platform brotherPlatform in platform.connections)
                {
                    queue.Enqueue(brotherPlatform);
                }
            }
        }

        return platformsFound;
    }

    /// <summary>
    /// Auxilliary method used to get the node in inNodes closest to the position of the reference Point.
    /// </summary>
    /// <param name="inNodes">Platforms we want to search</param>
    /// <param name="toPoint">Reference point</param>
    /// <returns></returns>
    public static Platform Closest(List<Platform> inNodes, Vector3 toPoint)
    {
        Platform closestNode = inNodes[0];
        float minDist = float.MaxValue;
        for (int i = 0; i < inNodes.Count; i++)
        {
            if (AStarHelper.Invalid(inNodes[i]))
                continue;
            float thisDist = Vector3.Distance(toPoint, inNodes[i].Position);
            if (thisDist > minDist)
                continue;

            minDist = thisDist;
            closestNode = inNodes[i];
        }

        return closestNode;
    }


    /// - IPATHNODE.CS
    public List<Platform> Connections
    {
        get { return connections; }
    }
    
    public Vector3 Position
    {
        get { return transform.position; }
    }
    
    public bool Invalid
    {
		get { return (this == null || this.type.Equals(PlatformType.Invalid)); }
    }

    public Vector3 getTargetPoint()
    {
        return transform.position;
    }
}
