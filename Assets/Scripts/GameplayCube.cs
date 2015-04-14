﻿using UnityEngine;
using System.Collections;

[SelectionBase]
public class GameplayCube : MonoBehaviour
{
	//[System.Flags]
	public enum GlueSides
	{
		None = 0x00,
		Up = 0x01,
		Down = 0x02,
		Left = 0x04,
		Right = 0x08,
		Front = 0x10,
		Back = 0x20
	}

	[HideInInspector] [SerializeField] private TileType m_left	= TileType.None;
	[HideInInspector] [SerializeField] private TileType m_right	= TileType.None;
	[HideInInspector] [SerializeField] private TileType m_up	= TileType.None;
	[HideInInspector] [SerializeField] private TileType m_down	= TileType.None;
	[HideInInspector] [SerializeField] private TileType m_front	= TileType.None;
	[HideInInspector] [SerializeField] private TileType m_back	= TileType.None;
	
	[ExposeProperty]
	public TileType All
	{
		get { return m_up; }
		set	{
			SetFace( "left", value ); m_left = value;
			SetFace( "right", value ); m_right = value;
			SetFace( "up", value ); m_up = value;
			SetFace( "down", value ); m_down = value;
			SetFace( "front", value ); m_front = value;
			SetFace( "back", value ); m_back = value;
		}
	}
	[ExposeProperty]
	public TileType Left
	{
		get { return m_left; }
		set	{ if(value != m_left) { SetFace( "left", value ); m_left = value; } }
	}
	[ExposeProperty]
	public TileType Right
	{
		get { return m_right; }
		set	{ if(value != m_right) { SetFace( "right", value ); m_right = value; } }
	}
	[ExposeProperty]
	public TileType Up
	{
		get { return m_up; }
		set	{ if(value != m_up) { SetFace( "up", value ); m_up = value; } }
	}
	[ExposeProperty]
	public TileType Down
	{
		get { return m_down; }
		set	{ if(value != m_down) { SetFace( "down", value ); m_down = value; } }
	}
	[ExposeProperty]
	public TileType Front
	{
		get { return m_front; }
		set	{ if(value != m_front) { SetFace( "front", value ); m_front = value; } }
	}
	[ExposeProperty]
	public TileType Back
	{
		get { return m_back; }
		set	{ if(value != m_back) { SetFace( "back", value ); m_back = value; } }
	}

	//public GluedSides GlueSides;
	[Header("Glue")]
	[BitMask(typeof(GlueSides))]
	public GlueSides GluedSides;
	
	void OnValidate()
	{
		bool isUp = (GluedSides & GlueSides.Up) != GlueSides.None;
		bool isDown = (GluedSides & GlueSides.Down) != GlueSides.None;
		bool isRight = (GluedSides & GlueSides.Right) != GlueSides.None;
		bool isLeft = (GluedSides & GlueSides.Left) != GlueSides.None;
		bool isFront = (GluedSides & GlueSides.Front) != GlueSides.None;
		bool isBack = (GluedSides & GlueSides.Back) != GlueSides.None;
		
		Transform up = transform.FindChild ("up");
		Transform down = transform.FindChild ("down");
		Transform right = transform.FindChild ("right");
		Transform left = transform.FindChild ("left");
		Transform front = transform.FindChild ("front");
		Transform back = transform.FindChild ("back");

		if ( up != null )
		{
			if (isUp)
				up.GetComponent<Tile> ().IsGlueTile = true;
			else
				up.GetComponent<Tile> ().IsGlueTile = false;
		}
		
		if ( down != null )
		{
			if (isDown)
				down.GetComponent<Tile> ().IsGlueTile = true;
			else
				down.GetComponent<Tile> ().IsGlueTile = false;
		}
		
		if ( right != null )
		{
			if (isRight)
				right.GetComponent<Tile> ().IsGlueTile = true;
			else
				right.GetComponent<Tile> ().IsGlueTile = false;
		}
		
		if ( left != null )
		{
			if (isLeft)
				left.GetComponent<Tile> ().IsGlueTile = true;
			else
				left.GetComponent<Tile> ().IsGlueTile = false;
		}
		
		if ( front != null )
		{
			if (isFront)
				front.GetComponent<Tile> ().IsGlueTile = true;
			else
				front.GetComponent<Tile> ().IsGlueTile = false;
		}
		
		if ( back != null )
		{
			if (isBack)
				back.GetComponent<Tile> ().IsGlueTile = true;
			else
				back.GetComponent<Tile> ().IsGlueTile = false;
		}
	}

	public void SetFace( string faceName, TileType type )
	{
		// Delete existing tile
		foreach (Transform child in transform)
			if (child.gameObject.name == faceName)
				DestroyImmediate( child.gameObject );

		if ( type == TileType.None )
			return;

		Tile tile;
		GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);

		// Destroy the MeshCollider to avoid tensor errors
		//DestroyImmediate(face.GetComponent<MeshCollider> ());
		//face.AddComponent<BoxCollider>();
		
		face.name = faceName;
		face.transform.parent = transform;
		face.transform.position = transform.position;
		face.transform.localScale = Vector3.one;
		
		tile = face.AddComponent<Tile>();
		tile.gameObject.layer = LayerMask.NameToLayer( "Tiles" );
		tile.type = type;

		switch( faceName )
		{
		case "front":
			face.transform.rotation = Quaternion.LookRotation( Vector3.forward, Vector3.up );
			break;
		case "back":
			face.transform.rotation = Quaternion.LookRotation( Vector3.back, Vector3.up );
			break;
		case "up":
			face.transform.rotation = Quaternion.LookRotation( Vector3.down, Vector3.forward );
			break;
		case "down":
			face.transform.rotation = Quaternion.LookRotation( Vector3.up, Vector3.forward );
			break;
		case "right":
			face.transform.rotation = Quaternion.LookRotation( Vector3.right, Vector3.up );
			break;
		case "left":
			face.transform.rotation = Quaternion.LookRotation( Vector3.left, Vector3.up );
			break;
		}

		tile.CheckTileOrientation();

		face.transform.Translate(new Vector3(0, 0, -transform.localScale.x * 0.5f), Space.Self);

		// Tag up with parent's tag
		if (transform.parent != null && transform.parent.tag != null)
			face.tag = transform.parent.tag;

		// Case of a spike tile
		if ( type == TileType.Spikes)
		{
			//face.AddComponent<Spikes>();
			
			//GameObject child = GameObject.Instantiate( Resources.LoadAssetAtPath("Assets/Resources/PREFABS/spikes.prefab", typeof(GameObject)) ) as GameObject;
			GameObject child = (GameObject) GameObject.Instantiate( Resources.Load( "PREFABS/spikes" ) );

			child.name = "spikes";
			child.transform.parent = tile.transform;
			child.transform.position = new Vector3( 0, 0, 0 );
			child.transform.localPosition = new Vector3( 0.5f, -0.5f, 0 );
			//child.transform.GetChild(0).transform.position = new Vector3( 0, 0, 0 );
			//child.transform.GetChild(0).transform.localPosition = new Vector3( 0, 0, 0 );
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (transform.position, transform.localScale);
	}
#endif
}
