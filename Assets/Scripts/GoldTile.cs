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
		Reset();
	}
	
	public void Reset()
	{
		// get the world orientation to choose the correct material
		Pawn PlayerPawn = GameObject.Find("Pawn").GetComponent<Pawn>() as Pawn;
		if (PlayerPawn != null)
			ChangeGravity(PlayerPawn.GetWorldGravity());
		else
			GetComponent<MeshRenderer>().material = inactiveMaterial;
	}

	private void ChangeGravity( TileOrientation gravityOrientation )
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
}
