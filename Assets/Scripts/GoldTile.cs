using UnityEngine;
using System.Collections;

public class GoldTile : MonoBehaviour {

	Material mat;
	Color matColor;

	TileOrientation orientation;
	
	public Color spiralColorActive;
	public Color spiralColorInactive;

	// Use this for initialization
	void Awake ()
	{
		mat = GetComponent<MeshRenderer> ().material;
		matColor = spiralColorInactive;

		DefineOrientation ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		mat.color = matColor;
	}

	public void Reset()
	{
		matColor = spiralColorInactive;
	}

	private void ChangeGravity( TileOrientation gravityOrientation )
	{
		if ( gravityOrientation == orientation )
			matColor = spiralColorActive;
		else
			matColor = spiralColorInactive;
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
