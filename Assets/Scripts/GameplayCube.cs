using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GameplayCube : MonoBehaviour {
	
//	private bool __needsUpdate = false;
	
	[HideInInspector] [SerializeField] private PlatformType m_left;
	[HideInInspector] [SerializeField] private PlatformType m_right;
	[HideInInspector] [SerializeField] private PlatformType m_up;
	[HideInInspector] [SerializeField] private PlatformType m_down;
	[HideInInspector] [SerializeField] private PlatformType m_front;
	[HideInInspector] [SerializeField] private PlatformType m_back;
	
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
		if ( type == PlatformType.Spikes)
		{
			Debug.LogWarning( "PlatformType.Spikes not implemented yet" );
			return;
		}
		
		foreach (Transform child in transform)
		{
			if (child.gameObject.name == faceName)
			{
				DestroyImmediate( child.gameObject );
			}
		}
		
		if ( type == PlatformType.None)
		{
			return;
		}
		
		GameObject face = GameObject.CreatePrimitive(PrimitiveType.Plane);
		Platform p;
		Material[] materials;
		
		face.name = faceName;
		face.transform.parent = transform;
		face.transform.position = transform.position;
		
		p = face.AddComponent<Platform>();
		
		materials = face.renderer.sharedMaterials;
		materials = new Material[] {
			face.renderer.sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		p.type = type;
		
		
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
		
		face.transform.Translate(new Vector3(0, transform.localScale.x/2 + 0.01f, 0), Space.Self);
		
		// TODO -- fallover: Invalid is discarded
	//	if ( type != PlatformType.Invalid )
			face.renderer.sharedMaterials = materials;
	}
	
	public void Refresh()
	{
	}
	
	// Use this for initialization
	void Start () {
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
		
		materials = face.renderer.sharedMaterials;
		
		materials = new Material[] {
			face.renderer.sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		materials[0] = Assets.getExitBlockMat();
		materials[0].shader = Shader.Find("Diffuse");
		materials[1] = Assets.getFrontBlockMat();
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2] = Assets.getFrontBlockMat();
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
		face.renderer.sharedMaterials = materials;
	}
}
