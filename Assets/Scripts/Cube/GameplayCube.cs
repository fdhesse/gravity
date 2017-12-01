using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[SelectionBase]
public class GameplayCube : MonoBehaviour
{
	// static constants
	public const string MOVING_PLATFORM_TAG = "MovingPlatform";
	public const string GRAVITY_PLATFORM_TAG = "GravityPlatform";
	public const string FALLING_CUBE_TAG = "FallingCube";
	public const float CUBE_SIZE = 2f;
	public const float HALF_CUBE_SIZE = CUBE_SIZE * 0.5f;

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

	private bool IsFaceGlued(string faceName)
	{
		switch( faceName )
		{
		case "front":
			return (GluedSides & GlueSides.Front) != GlueSides.None;
		case "back":
			return (GluedSides & GlueSides.Back) != GlueSides.None;
		case "up":
			return (GluedSides & GlueSides.Up) != GlueSides.None;
		case "down":
			return (GluedSides & GlueSides.Down) != GlueSides.None;
		case "right":
			return (GluedSides & GlueSides.Right) != GlueSides.None;
		case "left":
			return (GluedSides & GlueSides.Left) != GlueSides.None;
		}
		return false;
	}

	public void SetFace( string faceName, TileType type )
	{
		#if UNITY_EDITOR

		// Delete existing tile
		foreach (Transform child in transform)
			if (child.gameObject.name == faceName)
				DestroyImmediate( child.gameObject );

		if ( type == TileType.None )
			return;

		GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);

		// Destroy the MeshRenderer and MeshFilter
		DestroyImmediate(face.GetComponent<MeshRenderer>());
		DestroyImmediate(face.GetComponent<MeshFilter>());

		// add a rigid body and make it kinetic (also freeze in rotation)
		Rigidbody rb = face.AddComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		face.name = faceName;
		face.transform.parent = transform;
		face.transform.position = transform.position;
		face.transform.localScale = new Vector3(0.99f, 0.99f, 0.99f);

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
		
		face.transform.Translate(new Vector3(0, 0, -transform.localScale.x * 0.495f), Space.Self);

		// propagate my tag to the face
		setTileTag(face);

		// Add the Tile component (that will add the tile mesh)
		Tile tile = face.AddComponent<Tile>();
		tile.gameObject.layer = LayerMask.NameToLayer( "Tiles" );
		tile.IsGlueTile = IsFaceGlued(faceName);
		tile.Type = type;
		tile.CheckTileOrientation();
		// set the static flag (after fixing the tag of this gameplay cube)
		tile.setStaticFlag(shouldTileMeshBeStatic());

		#endif
	}

	private bool shouldTileMeshBeStatic()
	{
		// by default the mesh tile are static unless it's a moving platform
		return (!this.gameObject.CompareTag(MOVING_PLATFORM_TAG) &&
				!this.gameObject.CompareTag(GRAVITY_PLATFORM_TAG) &&
				!this.gameObject.CompareTag(FALLING_CUBE_TAG));
	}

	private void setTileTag(GameObject childTile)
	{
		// First check if this gameplay cube has an empty tag, but has a parent, that
		// may means that this gameplay cube is part of a moving platform grouping several
		// gameplay platform, so get that tag from the parent
		if ((this.transform.parent != null) && !this.gameObject.CompareTag(transform.parent.tag) &&
		    (transform.parent.CompareTag(MOVING_PLATFORM_TAG) || transform.parent.CompareTag(GRAVITY_PLATFORM_TAG) ||
			 transform.parent.CompareTag(FALLING_CUBE_TAG)))
			this.gameObject.tag = transform.parent.tag;

		// after fixing the tag to be like my potential parent, fix the tag of my tile children
		childTile.tag = this.gameObject.tag;
	}

	#if UNITY_EDITOR
	void OnValidate()
	{
		bool isStatic = shouldTileMeshBeStatic();

		// get all my tiles
		Tile[] childTiles = GetComponentsInChildren<Tile>();

		// set the glue and static flags for all tiles
		foreach (Tile tile in childTiles)
		{
			// reset the tag of the tile like the tag of this gameobject,
			// so that if the level designer change this gameplay cube to a moving platform,
			// the tag is correctly propagated to to the children tiles, without the need to
			// recreate the faces
			setTileTag(tile.gameObject);

			// set the glue state of the tile
			tile.IsGlueTile = IsFaceGlued(tile.name);

			// set the static state of the tile
			tile.setStaticFlag(isStatic);

			// check the material (to avoid missing material when user press the "Revert" button)
			tile.updateMeshTileEditorMaterial();
		}
	}
		
	public void updateTileMesh(bool forceUpdate)
	{
		bool isStatic = shouldTileMeshBeStatic();

		for (int i = 0; i < this.transform.childCount; ++i)
		{
			Tile childTile = this.transform.GetChild(i).GetComponent<Tile>();
			if (childTile != null)
			{
				childTile.updateTileMesh(forceUpdate);
				childTile.setStaticFlag(isStatic);
			}
		}
	}

	// add a menu items to update all the tile meshes of all the gameplay cubes in the scene
	[MenuItem("Mu/Update all Tile Meshes")]
	static void UpdateTileMeshes()
	{
		// get all the gameplay cubes
		GameplayCube[] allGameCubes = FindObjectsOfType<GameplayCube>();
		// and update their meshes
		foreach (GameplayCube cube in allGameCubes)
			cube.updateTileMesh(true);

		// mark the current scene as dirty (cause Unity will not detect the change of the mesh and wont prompt for saving)
		if (allGameCubes.Length > 0)
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
	}

	private static Color s_DefaultColor = new Color(0.6f, 0.6f, 0.6f);
	private static Color s_SelectedColor = new Color(1f, 0.25f, 0f);
	private static Color s_EmptyCubeColor = Color.red;

	void OnDrawGizmosSelected()
	{
		Gizmos.color = s_SelectedColor;
		Gizmos.DrawWireCube (transform.position, transform.localScale);
	}

	void OnDrawGizmos()
	{
		// continue to draw the cube if all the faces of the cube are tiles of type None
		// (which is a bug obviously), to make it visible that it is a bug
		if ((m_left == TileType.None) && (m_right == TileType.None) &&
			(m_up == TileType.None) && (m_down == TileType.None) &&
			(m_front == TileType.None) && (m_back == TileType.None))
			Gizmos.color = s_EmptyCubeColor;
		else
			Gizmos.color = s_DefaultColor;

		// draw the wire cube
		Gizmos.DrawWireCube (transform.position, transform.localScale);
	}
	#endif
}
