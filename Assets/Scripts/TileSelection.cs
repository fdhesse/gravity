using UnityEngine;
using System.Collections;

/// <summary>
/// This is more of a auxilliary class for handling platform selection
/// </summary>
public static class TileSelection
{
    private static Tile currentTile;//currently selected platform

	private static LayerMask tilesLayer = LayerMask.NameToLayer( "Tiles" );

	private static GameObject player;

	public static Vector3 PlayerPosition
	{
		get { if ( player == null ) player = GameObject.Find( "Pawn" ); return player.transform.position; }
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
    /// Gets the platform that is being targeted.
    /// </summary>
    public static Tile getTile()
	{
		if (currentTile != null)
			currentTile.unHighlight();

        Tile tile = null;

		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();

		if (Physics.Raycast(mouseRay, out hit, float.MaxValue, (1 << tilesLayer))) // cast a raycast ignoring all but the layer for the tiles
        {
			tile = hit.collider.gameObject.GetComponent<Tile>();

			if (tile != null && TileSelection.isClickableType( tile.type ) ) //if it is a tile
				tile.highlight();
		}

		currentTile = tile;
		return tile;
    }

}
