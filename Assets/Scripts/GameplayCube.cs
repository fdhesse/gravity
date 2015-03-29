using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GameplayCube : MonoBehaviour {
	
//	private bool __needsUpdate = false;
	
	[HideInInspector] [SerializeField] private PlatformType m_left	= PlatformType.None;
	[HideInInspector] [SerializeField] private PlatformType m_right	= PlatformType.None;
	[HideInInspector] [SerializeField] private PlatformType m_up	= PlatformType.None;
	[HideInInspector] [SerializeField] private PlatformType m_down	= PlatformType.None;
	[HideInInspector] [SerializeField] private PlatformType m_front	= PlatformType.None;
	[HideInInspector] [SerializeField] private PlatformType m_back	= PlatformType.None;
	
	[ExposeProperty]
	public PlatformType Left
	{
		get { return m_left; }
		set	{ if(value != m_left) { SetFace( "left", value ); m_left = value; } }
//		set	{ if(value != m_left) { __needsUpdate = true; m_left = value; } }
	}
	[ExposeProperty]
	public PlatformType Right
	{
		get { return m_right; }
		set	{ if(value != m_right) { SetFace( "right", value ); m_right = value; } }
//		set	{ if(value != m_left) { __needsUpdate = true; m_right = value; } }
	}
	[ExposeProperty]
	public PlatformType Up
	{
		get { return m_up; }
		set	{ if(value != m_up) { SetFace( "up", value ); m_up = value; } }
//		set	{ if(value != m_up) { __needsUpdate = true; m_up = value; } }
	}
	[ExposeProperty]
	public PlatformType Down
	{
		get { return m_down; }
		set	{ if(value != m_down) { SetFace( "down", value ); m_down = value; } }
//		set	{ if(value != m_down) { __needsUpdate = true; m_down = value; } }
	}
	[ExposeProperty]
	public PlatformType Front
	{
		get { return m_front; }
		set	{ if(value != m_front) { SetFace( "front", value ); m_front = value; } }
//		set	{ if(value != m_front) { __needsUpdate = true; m_front = value; } }
	}
	[ExposeProperty]
	public PlatformType Back
	{
		get { return m_back; }
		set	{ if(value != m_back) { SetFace( "back", value ); m_back = value; } }
	}
	
	public void SetFace( string faceName, PlatformType type )
	{

		// supprimer la plateforme actuelle

		foreach (Transform child in transform)
		{
			if (child.gameObject.name == faceName)
			{
				DestroyImmediate( child.gameObject );
			}
		}

		// pas de plateforme, fin

		if ( type == PlatformType.None )
		{
			return;
		}
		
		Platform p;
		Material[] materials;
		GameObject face = GameObject.CreatePrimitive(PrimitiveType.Plane);

		// first destroy the MeshCollider to avoid tensor errors
		DestroyImmediate(face.GetComponent<MeshCollider> ());
		face.AddComponent<BoxCollider>();
		
		face.name = faceName;
		face.transform.parent = transform;
		face.transform.position = transform.position;
		
		p = face.AddComponent<Platform>();
		p.gameObject.layer = 14;
		p.type = type;
		
		// clean up the platform's connections
		p._connections = null;
		
		materials = face.GetComponent<Renderer>().sharedMaterials;
		materials = new Material[] {
			face.GetComponent<Renderer>().sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		switch( type )
		{
			case PlatformType.Valid:
				materials[0] = Assets.getValidBlockMat();
				materials[0].shader = Shader.Find("Diffuse");
				break;
			case PlatformType.Invalid:
				materials[0] = Assets.getInvalidBlockMat();
				materials[0].shader = Shader.Find("Diffuse");
				break;
			case PlatformType.Exit:
				materials[0] = Assets.getExitBlockMat();
				materials[0].shader = Shader.Find("Diffuse");
				break;
			default:
				materials[0] = Assets.getInvalidBlockMat();
				materials[0].shader = Shader.Find("Diffuse");
				break;
		}

//		materials [0].name = "fill";
		
		switch( faceName )
		{
			case "left":
				materials[1] = Assets.getLeftBlockMat();
				materials[2] = Assets.getLeftBlockMat();
				
				face.transform.rotation = Quaternion.Euler( new Vector3( 90, 90, 0 ));
				p.orientation = PlatformOrientation.Left;
				break;
			case "right":
				materials[1] = Assets.getRightBlockMat();
				materials[2] = Assets.getRightBlockMat();
				
				face.transform.rotation = Quaternion.Euler( new Vector3( 90, 270, 0 ));
				p.orientation = PlatformOrientation.Right;
					break;
			case "up":
				materials[1] = Assets.getUpBlockMat();
				materials[2] = Assets.getUpBlockMat();
				
				face.transform.rotation = Quaternion.Euler( new Vector3( 0, 180, 0 ));
				p.orientation = PlatformOrientation.Up;
					break;
			case "down":
				materials[1] = Assets.getDownBlockMat();
				materials[2] = Assets.getDownBlockMat();
				
				face.transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 180 ));
				p.orientation = PlatformOrientation.Down;
					break;
			case "front":
				materials[1] = Assets.getFrontBlockMat();
				materials[2] = Assets.getFrontBlockMat();
				
				face.transform.rotation = Quaternion.Euler( new Vector3( 90, 180, 0 ));
				p.orientation = PlatformOrientation.Front;
					break;
			case "back":
				materials[1] = Assets.getBackBlockMat();
				materials[2] = Assets.getBackBlockMat();
				
				face.transform.rotation = Quaternion.Euler( new Vector3( 90, 0, 0 ));
				p.orientation = PlatformOrientation.Back;
					break;
			default:
				materials[1] = Assets.getFrontBlockMat();
				materials[2] = Assets.getFrontBlockMat();
				
				face.transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 0 ));
				p.orientation = PlatformOrientation.Front;
				break;
		}
		
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
//		face.transform.Translate(new Vector3(0, transform.localScale.x/2 + 0.2f, 0), Space.Self);
		face.transform.Translate(new Vector3(0, transform.localScale.x/2, 0), Space.Self);
		
		face.GetComponent<Renderer>().sharedMaterials = materials;
		
		// tag accordingly to parent's parent tag

		if (transform.parent != null && transform.parent.tag != null)
		{
			face.tag = transform.parent.tag;
		}
		
		// in case we are in presence of a spike tile
		
		if ( type == PlatformType.Spikes)
		{
			face.AddComponent<Spikes>();
			
			GameObject child = GameObject.Instantiate( Resources.LoadAssetAtPath("Assets/Resources/Prefabs/spikes.prefab", typeof(GameObject)) ) as GameObject;
			
//			GameObject child = (GameObject) Resources.Load( "Prefabs/spikes.prefab" );
			child.name = "spikes";
			child.transform.parent = p.transform;
			child.transform.position = new Vector3( 0, 0, 0 );
			child.transform.localPosition = new Vector3( 0, 0, 0 );
			child.transform.GetChild(0).transform.position = new Vector3( 0, 0, 0 );
			child.transform.GetChild(0).transform.localPosition = new Vector3( 0, 0, 0 );

//			Debug.Log( child );
//			child.AddComponent<MeshFilter>().mesh = "pPlane1";
//			face.gameObject
		}
	}
	
	public void Refresh()
	{
	}
	
	// Use this for initialization
	void Start () {
		GetComponent<MeshFilter>();
	}
	
	// Update is called once per frame
	void Update () {
	
//		if ( __update == true )
	//		Debug.Log("OK !!");
		
//		__update = false;
//		if (left != null)
//			AddFace(  );
	}
	
	void AddFace(  )
	{
		foreach (Transform child in transform)
		{
			if (child.gameObject.name == "front")
			{
				return;
			}
		}
		
		GameObject face = GameObject.CreatePrimitive(PrimitiveType.Plane);
		Platform p;
		Material[] materials;
		
		face.name = "front";
		
		face.transform.parent = transform;
		
		
		face.transform.position = new Vector3( 25, 5, 60 );
		face.transform.rotation = Quaternion.Euler( new Vector3( 90, 180, 0 ));
		
		p = face.AddComponent<Platform>();
		p.type = PlatformType.Exit;
		p.orientation = PlatformOrientation.Front;
		
		materials = face.GetComponent<Renderer>().sharedMaterials;
		
		materials = new Material[] {
			face.GetComponent<Renderer>().sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		materials[0] = Assets.getExitBlockMat();
		materials[0].shader = Shader.Find("Diffuse");
		materials[1] = Assets.getFrontBlockMat();
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2] = Assets.getFrontBlockMat();
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
		face.GetComponent<Renderer>().sharedMaterials = materials;
	}

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (transform.position, transform.localScale);
	}
#endif
}
