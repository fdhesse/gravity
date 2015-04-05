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
	private static Material sphereMat;

	public static AudioClip invalidSound;
	public static AudioClip bounce;
	public static AudioClip bounce2;

	public static GameObject mouseCursor;

    //public static Material upBlockMat;
    //public static Material downBlockMat;
    //public static Material leftBlockMat;
    //public static Material rightBlockMat;
    //public static Material frontBlockMat;
    //public static Material backBlockMat;
	static Assets()
	{
		mouseCursor = GameObject.Find ("Mouse Cursor");

		if ( mouseCursor == null )
		{
			mouseCursor = (GameObject) GameObject.Instantiate( Resources.Load( "PREFABS/Mouse Cursor" ) );
			mouseCursor.name = "Mouse Cursor";
			mouseCursor.transform.position = Vector3.one * float.MaxValue;
		}

		invalidSound = Resources.Load("Sounds/invalidSound") as AudioClip;
		bounce = Resources.Load("Sounds/bounce") as AudioClip;
		bounce2 = Resources.Load("Sounds/bounce2") as AudioClip;

        // -- ORIENTATION MATERIALS
        //upBlockMat = new Material(Resources.Load("Resources/Materials/orientations/up") as Material);
        //downBlockMat = new Material(Resources.Load("Resources/Materials/orientations/down") as Material);
        //leftBlockMat = new Material(Resources.Load("Resources/Materials/orientations/left") as Material);
        //rightBlockMat = new Material(Resources.Load("Resources/Materials/orientations/right") as Material);
        //frontBlockMat = new Material(Resources.Load("Resources/Materials/orientations/front") as Material);
        //backBlockMat = new Material(Resources.Load("Resources/Materials/orientations/back") as Material);
    }
	
#if UNITY_EDITOR
	
	private static Material upBlockMat;
	private static Material downBlockMat;
	private static Material leftBlockMat;
	private static Material rightBlockMat;
	private static Material frontBlockMat;
	private static Material backBlockMat;
	
	private static Material validBlockMat;
	private static Material invalidBlockMat;
	private static Material exitBlockMat;

    public static Material getValidBlockMat()
	{
		if ( validBlockMat == null )
			validBlockMat = Resources.Load("Materials/blocks/valid") as Material;
		
		return new Material( validBlockMat );
	}
	public static Material getInvalidBlockMat()
	{
		if ( invalidBlockMat == null )
			invalidBlockMat = Resources.Load("Materials/blocks/invalid") as Material;
		
		return new Material( invalidBlockMat );
	}
	public static Material getExitBlockMat()
	{
		if ( exitBlockMat == null )
			exitBlockMat = Resources.Load("Materials/blocks/exit") as Material;
		
		return new Material( exitBlockMat );
	}
	
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
#elif UNITY_STANDALONE

	private static Material blankBlockMat;

	public static Material getBlankBlockMat()
	{
		if ( blankBlockMat == null )
			blankBlockMat = Resources.Load("Materials/blocks/blank") as Material;
		
		return new Material( blankBlockMat );
	}
	public static Material getValidBlockMat()
	{
		return getBlankBlockMat();
	}
	public static Material getInvalidBlockMat()
	{
		return getBlankBlockMat();
	}
	public static Material getExitBlockMat()
	{
		return getBlankBlockMat();
	}
#endif

    public static Material getHighlightedValidBlockMat()
	{
		if ( highlightedValidBlockMat == null )
			highlightedValidBlockMat = Resources.Load("Materials/blocks/highlighted/validHighlighted") as Material;
		
		return new Material( highlightedValidBlockMat );
    }
    public static Material getHighlightedInvalidBlockMat()
	{
		if ( highlightedInvalidBlockMat == null )
			highlightedInvalidBlockMat = Resources.Load("Materials/blocks/highlighted/invalidHighlighted") as Material;
		
		return new Material( highlightedInvalidBlockMat );
    }
    public static Material getHighlightedExitBlockMat()
	{
		if ( highlightedExitBlockMat == null )
			highlightedExitBlockMat = Resources.Load("Materials/blocks/highlighted/exitHighlighted") as Material;
		
		return new Material( highlightedExitBlockMat );
    }
    public static Material getFlashingValidBlockMat()
	{
		if ( flashingValidBlockMat == null )
			flashingValidBlockMat = Resources.Load("Materials/blocks/flashing/validFlashing") as Material;
		
		return new Material( flashingValidBlockMat );
    }
    public static Material getFlashingInvalidBlockMat()
	{
		if ( flashingInvalidBlockMat == null )
			flashingInvalidBlockMat = Resources.Load("Materials/blocks/flashing/invalidFlashing") as Material;
		
		return new Material( flashingInvalidBlockMat );
    }
    public static Material getFlashingExitBlockMat()
	{
		if ( flashingExitBlockMat == null )
			flashingExitBlockMat = Resources.Load("Materials/blocks/flashing/exitFlashing") as Material;
		
		return new Material( flashingExitBlockMat );
    }

    internal static Material getSphereMat()
	{
		if ( sphereMat == null )
			sphereMat = Resources.Load("Materials/sphere") as Material;
		
		return new Material( sphereMat );
    }
}
