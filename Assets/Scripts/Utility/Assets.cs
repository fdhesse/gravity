using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

/// <summary>
/// This class is responsible for Asset Loading.
/// If you want to boost performance, start here.
/// </summary>
[ExecuteInEditMode]
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class Assets
{
	private static Material highlightedValidBlockMat;
	private static Material highlightedInvalidBlockMat;
	private static Material highlightedExitBlockMat;
	private static Material flashingValidBlockMat;
	private static Material flashingInvalidBlockMat;
	private static Material flashingExitBlockMat;

	public static AudioClip invalidSound;
	public static AudioClip bounce;
	public static AudioClip bounce2;

	public static GameObject mouseCursor;

	static Assets()
	{
		invalidSound = Resources.Load("Sounds/invalidSound") as AudioClip;
		bounce = Resources.Load("Sounds/bounce") as AudioClip;
		bounce2 = Resources.Load("Sounds/bounce2") as AudioClip;
    }

	public static void SetMouseCursor()
	{
		GameObject[] cursorsObjects = GameObject.FindGameObjectsWithTag ("Mouse Cursor");
		
		foreach( GameObject cursor in cursorsObjects )
			GameObject.DestroyImmediate( cursor );

		mouseCursor = (GameObject) GameObject.Instantiate( Resources.Load( "PREFABS/Mouse Cursor" ) );
		mouseCursor.name = "Mouse Cursor";
		mouseCursor.transform.position = Vector3.one * float.MaxValue;

		mouseCursor.hideFlags = HideFlags.HideAndDontSave;
	}
	
#if UNITY_EDITOR
	
	private static Material upBlockMat;
	private static Material downBlockMat;
	private static Material leftBlockMat;
	private static Material rightBlockMat;
	private static Material frontBlockMat;
	private static Material backBlockMat;
	
	public static Material getUpBlockMat()
	{
		if ( upBlockMat == null )
			upBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/orientations/up.mat", typeof(Material)) as Material;

		return new Material( upBlockMat );
	}
	
	public static Material getDownBlockMat()
	{
		if ( downBlockMat == null )
			downBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/orientations/down.mat", typeof(Material)) as Material;
		
		return new Material( downBlockMat );
	}
	
	public static Material getLeftBlockMat()
	{
		if ( leftBlockMat == null )
			leftBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/orientations/left.mat", typeof(Material)) as Material;
		
		return new Material( leftBlockMat );
	}
	
	public static Material getRightBlockMat()
	{
		if ( rightBlockMat == null )
			rightBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/orientations/right.mat", typeof(Material)) as Material;
		
		return new Material( rightBlockMat );
	}
	
	public static Material getFrontBlockMat()
	{
		if ( frontBlockMat == null )
			frontBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/orientations/front.mat", typeof(Material)) as Material;
		
		return new Material( frontBlockMat );
	}
	
	public static Material getBackBlockMat()
	{
		if ( backBlockMat == null )
			backBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/orientations/back.mat", typeof(Material)) as Material;
		
		return new Material( backBlockMat );
	}
	
	public static Material getHighlightedValidBlockMat()
	{
		if ( highlightedValidBlockMat == null )
			highlightedValidBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/blocks/highlighted/validHighlighted.mat", typeof(Material)) as Material;
		
		return new Material( highlightedValidBlockMat );
	}
	public static Material getHighlightedInvalidBlockMat()
	{
		if ( highlightedInvalidBlockMat == null )
			highlightedInvalidBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/blocks/highlighted/invalidHighlighted.mat", typeof(Material)) as Material;
		
		return new Material( highlightedInvalidBlockMat );
	}
	public static Material getHighlightedExitBlockMat()
	{
		if ( highlightedExitBlockMat == null )
			highlightedExitBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/blocks/highlighted/exitHighlighted.mat", typeof(Material)) as Material;
		
		return new Material( highlightedExitBlockMat );
	}
	public static Material getFlashingValidBlockMat()
	{
		if ( flashingValidBlockMat == null )
			flashingValidBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/blocks/flashing/validFlashing.mat", typeof(Material)) as Material;
		
		return new Material( flashingValidBlockMat );
	}
	public static Material getFlashingInvalidBlockMat()
	{
		if ( flashingInvalidBlockMat == null )
			flashingInvalidBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/blocks/flashing/invalidFlashing.mat", typeof(Material)) as Material;
		
		return new Material( flashingInvalidBlockMat );
	}
	public static Material getFlashingExitBlockMat()
	{
		if ( flashingExitBlockMat == null )
			flashingExitBlockMat = AssetDatabase.LoadAssetAtPath ("Assets/Editor/Materials/blocks/flashing/exitFlashing.mat", typeof(Material)) as Material;
		
		return new Material( flashingExitBlockMat );
	}
#endif
}
