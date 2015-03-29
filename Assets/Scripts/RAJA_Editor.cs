using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[ExecuteInEditMode]
public class RAJA_Editor : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	#if UNITY_EDITOR
	
	/*	[ContextMenu("Context-Menu")] 
	public void Deb()
	{
		Debug.Log("Do...");
	}*/
	
	[MenuItem ("GameObject/Create Other/RAJA_GravityPlatform")]
	static void AddGravityPlatform () {
		
		GameObject go = new GameObject("GravityPlatform");
		go.AddComponent<GravityPlatform>();
		go.tag = "GravityPlatform";

		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.localScale = new Vector3( 10, 10, 10 );
		cube.transform.parent = go.transform;
		
//		cube.AddComponent<MeshRenderer>();
		cube.AddComponent<GameplayCube>();
	}
	
	[MenuItem ("GameObject/Create Other/RAJA_GameplayCube")]
	static void AddGameplayCube () {
		
//		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		GameObject go = new GameObject();
		go.name = "Gameplay Cube";
		
		go.transform.localScale = new Vector3( 10, 10, 10 );
		
		//		go.AddComponent<MeshRenderer>();
		GameplayCube cube = go.AddComponent<GameplayCube>();
		
		cube.Left = PlatformType.Invalid;
		cube.Right = PlatformType.Invalid;
		cube.Up = PlatformType.Invalid;
		cube.Down = PlatformType.Invalid;
		cube.Front = PlatformType.Invalid;
		cube.Back = PlatformType.Invalid;
	}
	
	[MenuItem ("GameObject/Create Other/RAJA_FallingCube")]
	static void AddStandardCube () {
		
//		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		GameObject go = new GameObject();
		
		go.name = "Falling Cube";
		go.tag = "FallingCube";
		
		go.transform.localScale = new Vector3( 10, 10, 10 );
		
		go.AddComponent<Rigidbody>();
		go.AddComponent<BoxCollider>();
		go.AddComponent<FallingCube>();
		
		AudioSource audio = go.AddComponent<AudioSource>();
		
		audio.playOnAwake = false;
		audio.clip = Assets.bounce;
		audio.minDistance = 100;
		
	}
	
	[MenuItem ("GameObject/Create Other/RAJA_Exit")]
	static void AddExitCube () {
		
		Material[] materials;
		Platform p;
		
		GameObject exit = new GameObject("exit");
		
		// #FRONT#
		
		GameObject front = GameObject.CreatePrimitive(PrimitiveType.Plane);
		front.name = "front";
		
		front.transform.parent = exit.transform;
		
		front.transform.position = new Vector3( 25, 5, 60 );
		front.transform.rotation = Quaternion.Euler( new Vector3( 90, 180, 0 ));
		
		p = front.AddComponent<Platform>();
		p.type = PlatformType.Exit;
		p.orientation = PlatformOrientation.Front;
		
		materials = front.GetComponent<Renderer>().sharedMaterials;
		
		materials = new Material[] {
			front.GetComponent<Renderer>().sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		materials[0] = Assets.getExitBlockMat();
		materials[0].shader = Shader.Find("Diffuse");
		materials[1] = Assets.getFrontBlockMat();
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2] = Assets.getFrontBlockMat();
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
		front.GetComponent<Renderer>().sharedMaterials = materials;
		
		// #BACK#
		
		GameObject back = GameObject.CreatePrimitive(PrimitiveType.Plane);
		back.name = "back";
		
		back.transform.parent = exit.transform;
		
		back.transform.position = new Vector3( 25, 5, 70 );
		back.transform.rotation = Quaternion.Euler( new Vector3( 90, 0, 0 ));
		
		p = back.AddComponent<Platform>();
		p.type = PlatformType.Exit;
		p.orientation = PlatformOrientation.Back;
		
		materials = back.GetComponent<Renderer>().sharedMaterials;
		
		materials = new Material[] {
			back.GetComponent<Renderer>().sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		materials[0] = Assets.getExitBlockMat();
		materials[0].shader = Shader.Find("Diffuse");
		materials[1] = Assets.getBackBlockMat();
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2] = Assets.getBackBlockMat();
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
		back.GetComponent<Renderer>().sharedMaterials = materials;
		
		// #UP#
		
		GameObject up = GameObject.CreatePrimitive(PrimitiveType.Plane);
		up.name = "up";
		
		up.transform.parent = exit.transform;
		
		up.transform.position = new Vector3( 25, 10, 65 );
		up.transform.rotation = Quaternion.Euler( new Vector3( 0, 180, 0 ));
		
		p = up.AddComponent<Platform>();
		p.type = PlatformType.Exit;
		p.orientation = PlatformOrientation.Up;
		
		materials = up.GetComponent<Renderer>().sharedMaterials;
		
		materials = new Material[] {
			up.GetComponent<Renderer>().sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		materials[0] = Assets.getExitBlockMat();
		materials[0].shader = Shader.Find("Diffuse");
		materials[1] = Assets.getUpBlockMat();
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2] = Assets.getUpBlockMat();
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
		up.GetComponent<Renderer>().sharedMaterials = materials;
		
		// #DOWN#
		
		GameObject down = GameObject.CreatePrimitive(PrimitiveType.Plane);
		down.name = "down";
		
		down.transform.parent = exit.transform;
		
		down.transform.position = new Vector3( 25, 0, 65 );
		down.transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 180 ));
		
		p = down.AddComponent<Platform>();
		p.type = PlatformType.Exit;
		p.orientation = PlatformOrientation.Down;
		
		materials = down.GetComponent<Renderer>().sharedMaterials;
		
		materials = new Material[] {
			down.GetComponent<Renderer>().sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		materials[0] = Assets.getExitBlockMat();
		materials[0].shader = Shader.Find("Diffuse");
		materials[1] = Assets.getDownBlockMat();
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2] = Assets.getDownBlockMat();
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
		down.GetComponent<Renderer>().sharedMaterials = materials;
		
		// #LEFT#
		
		GameObject left = GameObject.CreatePrimitive(PrimitiveType.Plane);
		left.name = "left";
		
		left.transform.parent = exit.transform;
		
		left.transform.position = new Vector3( 30, 5, 65 );
		left.transform.rotation = Quaternion.Euler( new Vector3( 90, 90, 0 ));
		
		p = left.AddComponent<Platform>();
		p.type = PlatformType.Exit;
		p.orientation = PlatformOrientation.Left;
		
		materials = left.GetComponent<Renderer>().sharedMaterials;
		
		materials = new Material[] {
			left.GetComponent<Renderer>().sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		materials[0] = Assets.getExitBlockMat();
		materials[0].shader = Shader.Find("Diffuse");
		materials[1] = Assets.getLeftBlockMat();
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2] = Assets.getLeftBlockMat();
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
		left.GetComponent<Renderer>().sharedMaterials = materials;
		
		// #RIGHT#
		
		GameObject right = GameObject.CreatePrimitive(PrimitiveType.Plane);
		right.name = "right";
		
		right.transform.parent = exit.transform;
		
		right.transform.position = new Vector3( 20, 5, 65 );
		right.transform.rotation = Quaternion.Euler( new Vector3( 90, 270, 0 ));
		
		p = right.AddComponent<Platform>();
		p.type = PlatformType.Exit;
		p.orientation = PlatformOrientation.Right;
		
		materials = right.GetComponent<Renderer>().sharedMaterials;
		
		materials = new Material[] {
			right.GetComponent<Renderer>().sharedMaterial,
			new Material(Shader.Find("Transparent/Diffuse")),
			new Material(Shader.Find("Transparent/Diffuse"))
		};
		
		materials[0] = Assets.getExitBlockMat();
		materials[0].shader = Shader.Find("Diffuse");
		materials[1] = Assets.getRightBlockMat();
		materials[1].shader = Shader.Find("Transparent/Diffuse");
		materials[2] = Assets.getRightBlockMat();
		materials[2].shader = Shader.Find("Transparent/Diffuse");
		
		right.GetComponent<Renderer>().sharedMaterials = materials;
	}
	#endif
}
