using UnityEngine;
using System.Collections;

/// <summary>
/// This is more of a auxilliary class for handling platform selection
/// </summary>
public static class TileSelection
{
    private static Tile platform;//currently selected platform
	private static Camera camera;//camera for this scene
	//private static GameObject cam;//camera for this scene

	private static LayerMask tilesLayer = LayerMask.NameToLayer( "Tiles" );

	private static GameObject player;

	public static Vector3 PlayerPosition
	{
		get { if ( player == null ) player = GameObject.Find( "Pawn" ); return player.transform.position; }
	}

    static TileSelection()
    {
		//cam = GameObject.FindGameObjectWithTag("MainCamera"); //initialize the camera
		camera = Camera.main;
	}
	
	/// <summary>
	/// Is the platform of a Clickable Type ?
	/// </summary>
	public static bool isClickableType( TileType type )
	{
		if ( type == TileType.Valid || type == TileType.Exit )
			return true;
		
		return false;
	}

    /// <summary>
    /// Highlights target platform
    /// </summary>
    public static void highlightTargetTile()
    {
        if (platform != null)
            platform.unHighlight();

        platform = getTile();
    }

    /// <summary>
    /// Gets the platform that is being targeted.
    /// </summary>
    public static Tile getTile()
    {
        Tile p = null;

		if (camera == null)//if there isn't a camera associated with this script, get the main camera
			camera = Camera.main;

		Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();

		if (Physics.Raycast(mouseRay, out hit, float.MaxValue, (1 << tilesLayer))) // cast a raycast ignoring all but the layer for the platforms
        {
            p = hit.collider.gameObject.GetComponent<Tile>();

			if (p != null && TileSelection.isClickableType( p.type ) ) //if it is a platform
                p.highlight();
		}
		return p;
    }

}
