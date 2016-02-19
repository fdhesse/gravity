using UnityEngine;
using System.Collections;

public class GoldTile : MonoBehaviour
{
	// all the tile will share the same material, either the active or inactive one
	public Material activeMaterial;
	public Material inactiveMaterial;

	private TileOrientation orientation;

	// Use this for initialization
	void Awake()
	{
		DefineOrientation();
		// The first material assignation doesn't really matter, because the reset will be called later
		// when the game start, so the correct orientation will be given at that time
		Reset(TileOrientation.Up);
	}
	
	public void Reset(TileOrientation startingOrientation)
	{
		ChangeGravity(startingOrientation);
	}

	public void ChangeGravity( TileOrientation gravityOrientation )
	{
		if ( gravityOrientation == orientation )
			GetComponent<MeshRenderer>().material = activeMaterial;
		else
			GetComponent<MeshRenderer>().material = inactiveMaterial;
	}

	private void DefineOrientation()
	{
		Vector3 tileDirection = transform.rotation * -Vector3.up;

		if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Up) ), 0 ) )
			orientation = TileOrientation.Up;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Down) ), 0 ) )
			orientation = TileOrientation.Down;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Right) ), 0 ) )
			orientation = TileOrientation.Right;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Left) ), 0 ) )
			orientation = TileOrientation.Left;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Front) ), 0 ) )
			orientation = TileOrientation.Front;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Back) ), 0 ) )
			orientation = TileOrientation.Back;
	}

	/// <summary>
	/// Updates the orientation of the golden tile. This function should be called if the golden tile is rotated
	/// for example when the golden tile is attached to a moving platform performing a rotation, or a rotating
	/// platform, after a change of gravity.
	/// </summary>
	public void UpdateOrientation(TileOrientation gravityOrientation)
	{
		// redefine its orientation
		DefineOrientation();
		// and update its material
		ChangeGravity( gravityOrientation );
	}
}
