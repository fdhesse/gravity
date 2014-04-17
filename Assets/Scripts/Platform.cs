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
[ExecuteInEditMode]
public class Platform : MonoBehaviour, IPathNode<Platform>
{
    public List<Platform> connections; //list of directly accessible platforms
    private HashSet<Platform> connectionSet; //auxilliary hashset used to ignore duplicates

    public PlatformType type = PlatformType.Valid; //type of this platform, can be valid, invalid or exit. Can be changed in editor
    public PlatformOrientation orientation;// orientation of this platform. Can be changed in editor but will be overriden by the scripts according to its rotation

    private bool isHighlighted = false;//whether this platform is highlighted
    private bool isFlashing = false;//wheter this platform is flashing
    private Ticker flash;//timer for platform flashing

    public Transform[] cons; //public array used for debuging, this way you can see the platform list in the editor
    public bool scanToogle = false;// debug toggle used to force rescan of nearby platforms
    private PlatformType oldType;// auxilliary variable

    // Use this for initialization
    void Start()
    {
 //       defineOrientation();
		applyPlatformMaterial();
		
#if UNITY_EDITOR
		GetComponent<Renderer>().enabled = true;
#elif UNITY_STANDALONE
		GetComponent<Renderer>().enabled = false;
#endif
    }


    // Update is called once per frame
    void Update()
    {
		#if UNITY_EDITOR
        if (Selection.Contains(gameObject))
        {
            defineOrientation();
        }
		#endif
        if (connectionSet == null) //this isn't in the start method because we have to make sure this is made after all Platforms have been initialized with the proper orientations
        {
            connectionSet = new HashSet<Platform>();
            scanNearbyPlatforms();
        }
        if (!oldType.Equals(type)) // if the type was changed in scene mode, reapply the material
        {
            applyPlatformMaterial();
        }

        if (scanToogle)
        {
            scanNearbyPlatforms();
        }

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
        Material[] materials = gameObject.renderer.sharedMaterials;
        string r = transform.rotation.eulerAngles.ToString();
        //Debug.Log(r);
        switch (r)
        {
            case "(0.0, 180.0, 0.0)":
                orientation = PlatformOrientation.Up;
                materials[0] = Assets.getUpBlockMat();
                materials[1] = Assets.getUpBlockMat();
                break;
            case "(0.0, 0.0, -180.0)":
                orientation = PlatformOrientation.Down;
                materials[0] = Assets.getDownBlockMat();
                materials[1] = Assets.getDownBlockMat();
                break;
			case "(90.0, 90.0, 0.0)":
				orientation = PlatformOrientation.Left;
				materials[0] = Assets.getLeftBlockMat();
				materials[1] = Assets.getLeftBlockMat();
                break;
			case "(90.0, 270.0, 0.0)":
				orientation = PlatformOrientation.Right;
				materials[0] = Assets.getRightBlockMat();
				materials[1] = Assets.getRightBlockMat();
                break;
            case "(90.0, 180.0, 0.0)":
                orientation = PlatformOrientation.Front;
                materials[0] = Assets.getFrontBlockMat();
                materials[1] = Assets.getFrontBlockMat();
                break;
            case "(90.0, 0.0, 0.0)":
                orientation = PlatformOrientation.Back;
                materials[0] = Assets.getBackBlockMat();
                materials[1] = Assets.getBackBlockMat();
                break;
            default:
                Debug.LogError("A block didn't update its orientation correctly, this is because its rotations is funky or not registered, rotation:" + r);
                break;
        }
        gameObject.renderer.materials = materials;
    }

    /// <summary>
    /// scans nearby platforms and puts them in the connections list, to later be used to calculates paths and so on.
    /// </summary>
    private void scanNearbyPlatforms()
    {
        connectionSet = new HashSet<Platform>();
        Collider[] hits = Physics.OverlapSphere(transform.position, 5.5f);
        //Debug.DrawLine(transform.position, transform.position + transform.up * 5f);

        foreach (Collider hit in hits)
        {
            if (hit.collider.transform != transform)
            {
                Platform p = hit.gameObject.GetComponent<Platform>();
                if (p != null && p.orientation.Equals(orientation))
                {
                    connectionSet.Add(p);


                    cons = new Transform[connectionSet.Count];
                    connections = new List<Platform>(connectionSet);
                    for (int i = 0; i != connections.Count; i++)
                    {
                        cons[i] = connections[i].transform;
                    }
                }
            }
        }
    }


    /// <summary>
    /// <para>Changes the platform material according to the charactristics of the platform.</para>
    /// <para>Although you can change the patforms materials in the editor this script will change all that.</para>
    /// <para>This is responsible for flashing, highlighting, which is also a change in materials.</para>
    /// </summary>
    private void applyPlatformMaterial()
    {
        //for some reason Unity doesn't let us change a single material, we have to change the material array
        Material[] materials = gameObject.renderer.sharedMaterials;
        switch (type)
        {
            case PlatformType.Valid:
                materials[2] = isHighlighted ? Assets.getHighlightedValidBlockMat() : Assets.getValidBlockMat();
                materials[2] = isFlashing ? Assets.getFlashingValidBlockMat() : materials[2];
                break;
            case PlatformType.Invalid:
                materials[2] = isHighlighted ? Assets.getHighlightedInvalidBlockMat() : Assets.getInvalidBlockMat();
                materials[2] = isFlashing ? Assets.getFlashingInvalidBlockMat() : materials[2];
                break;
            case PlatformType.Exit:
                materials[2] = isHighlighted ? Assets.getHighlightedExitBlockMat() : Assets.getExitBlockMat();
                materials[2] = isFlashing ? Assets.getFlashingExitBlockMat() : materials[2];
                break;
        }
        gameObject.renderer.materials = materials;
        oldType = type;
    }

    // this function is called whenever we want to highlight a platform, usually due to mousehover
    public void highlight()
    {
        isHighlighted = true;
        applyPlatformMaterial();
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
        get
        {

            return transform.position;
        }
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
