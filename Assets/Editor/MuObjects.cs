using UnityEngine;
using UnityEditor;
using System.Collections;

public static class MuObjects
{
	[MenuItem ("GameObject/Mu/RotatingPlatform", false, 10)]
	static void AddRotatingPlatform ( MenuCommand menuCmd )
	{
		GameObject go = new GameObject( "RotatingPlatform" );
		go.tag = "GravityPlatform";
		
		if ( menuCmd != null )
			GameObjectUtility.SetParentAndAlign( go, menuCmd.context as GameObject );
		
		go.AddComponent<RotatingPlatform>();
		
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.localScale = new Vector3( 10, 10, 10 );
		cube.transform.parent = go.transform;
		
		cube.AddComponent<GameplayCube>();
	}

	[MenuItem ("GameObject/Mu/GravityPlatform", false, 10)]
	static void AddGravityPlatform ( MenuCommand menuCmd )
	{
		GameObject go = new GameObject( "GravityPlatform" );
		go.tag = "GravityPlatform";
		
		if ( menuCmd != null )
			GameObjectUtility.SetParentAndAlign( go, menuCmd.context as GameObject );
		
		go.AddComponent<GravityPlatform>();
		
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.localScale = new Vector3( 10, 10, 10 );
		cube.transform.parent = go.transform;
		
		cube.AddComponent<GameplayCube>();
	}
	
	[MenuItem ("GameObject/Mu/GameplayCube", false, 10)]
	static void AddGameplayCube ( MenuCommand menuCmd ) {
		
		GameObject go = new GameObject( "Gameplay Cube" );
		
		if ( menuCmd != null )
			GameObjectUtility.SetParentAndAlign( go, menuCmd.context as GameObject );

		go.transform.localScale = new Vector3( 10, 10, 10 );
		
		go.AddComponent<MeshRenderer>();
		GameplayCube cube = go.AddComponent<GameplayCube>();
		
		cube.Left = TileType.Invalid;
		cube.Right = TileType.Invalid;
		cube.Up = TileType.Invalid;
		cube.Down = TileType.Invalid;
		cube.Front = TileType.Invalid;
		cube.Back = TileType.Invalid;
	}
	
	[MenuItem ("GameObject/Mu/FallingCube", false, 10)]
	static void AddFallingCube ( MenuCommand menuCmd ) {
		
		GameObject go = new GameObject( "Falling Cube" );
		
		if ( menuCmd != null )
			GameObjectUtility.SetParentAndAlign( go, menuCmd.context as GameObject );
		
		go.tag = "FallingCube";
		
		go.transform.localScale = new Vector3( 10, 10, 10 );
		
		go.AddComponent<Rigidbody>();
		go.AddComponent<BoxCollider>();
		go.AddComponent<FallingCube>();

		GameObject graphics = new GameObject ("Graphics");
		graphics.AddComponent<MeshFilter> ();
		graphics.AddComponent<MeshRenderer> ();
		graphics.transform.parent = go.transform;
		graphics.transform.localScale = Vector3.one;

		AudioSource audio = go.AddComponent<AudioSource>();
		
		audio.playOnAwake = false;
		audio.clip = Assets.bounce;
		audio.minDistance = 100;
		
	}

	[MenuItem ("GameObject/Mu/Exit", false, 10)]
	static void AddExitCube ( MenuCommand menuCmd ) {
		
		Material[] materials;
		Tile p;
		
		GameObject exit = new GameObject("exit");
		
		if ( menuCmd != null )
			GameObjectUtility.SetParentAndAlign( exit, menuCmd.context as GameObject );
		
		// #FRONT#
		
		GameObject front = GameObject.CreatePrimitive(PrimitiveType.Plane);
		front.name = "front";
		
		front.transform.parent = exit.transform;
		
		front.transform.position = new Vector3( 25, 5, 60 );
		front.transform.rotation = Quaternion.Euler( new Vector3( 90, 180, 0 ));
		
		p = front.AddComponent<Tile>();
		p.type = TileType.Exit;
		p.orientation = TileOrientation.Front;
		
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
		
		p = back.AddComponent<Tile>();
		p.type = TileType.Exit;
		p.orientation = TileOrientation.Back;
		
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
		
		p = up.AddComponent<Tile>();
		p.type = TileType.Exit;
		p.orientation = TileOrientation.Up;
		
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
		
		p = down.AddComponent<Tile>();
		p.type = TileType.Exit;
		p.orientation = TileOrientation.Down;
		
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
		
		p = left.AddComponent<Tile>();
		p.type = TileType.Exit;
		p.orientation = TileOrientation.Left;
		
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
		
		p = right.AddComponent<Tile>();
		p.type = TileType.Exit;
		p.orientation = TileOrientation.Right;
		
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
}
