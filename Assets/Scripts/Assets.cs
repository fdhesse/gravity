using UnityEngine;
using System.Collections;

/// <summary>
/// This class is responsible for Asset Loading.
/// If you want to boost performance, start here.
/// </summary>
static class Assets
{
	public static AudioClip invalidSound;
	public static AudioClip bounce;
	public static AudioClip bounce2;

    //public static Material upBlockMat;
    //public static Material downBlockMat;
    //public static Material leftBlockMat;
    //public static Material rightBlockMat;
    //public static Material frontBlockMat;
    //public static Material backBlockMat;
    static Assets()
    {
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
    public static Material getValidBlockMat()
    {
        return new Material(Resources.Load("Materials/blocks/valid") as Material);
	}
	public static Material getInvalidBlockMat()
	{
		return new Material(Resources.Load("Materials/blocks/invalid") as Material);
	}
	public static Material getExitBlockMat()
	{
		return new Material(Resources.Load("Materials/blocks/exit") as Material);
	}
#elif UNITY_STANDALONE
	public static Material getBlankBlockMat()
	{
		return new Material(Resources.Load("Materials/blocks/blank") as Material);
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
        return new Material(Resources.Load("Materials/blocks/highlighted/validHighlighted") as Material);
    }
    public static Material getHighlightedInvalidBlockMat()
    {
        return new Material(Resources.Load("Materials/blocks/highlighted/invalidHighlighted") as Material);
    }
    public static Material getHighlightedExitBlockMat()
    {
        return new Material(Resources.Load("Materials/blocks/highlighted/exitHighlighted") as Material);
    }
    public static Material getFlashingValidBlockMat()
    {
        return new Material(Resources.Load("Materials/blocks/flashing/validFlashing") as Material);
    }
    public static Material getFlashingInvalidBlockMat()
    {
        return new Material(Resources.Load("Materials/blocks/flashing/invalidFlashing") as Material);
    }
    public static Material getFlashingExitBlockMat()
    {
        return new Material(Resources.Load("Materials/blocks/flashing/exitFlashing") as Material);
    }

    public static Material getUpBlockMat()
    {
        return new Material(Resources.Load("Materials/orientations/up") as Material);
    }

    public static Material getDownBlockMat()
    {
        return new Material(Resources.Load("Materials/orientations/down") as Material);
    }

    public static Material getLeftBlockMat()
    {
        return new Material(Resources.Load("Materials/orientations/left") as Material);
    }

    public static Material getRightBlockMat()
    {
        return new Material(Resources.Load("Materials/orientations/right") as Material);
    }

    public static Material getFrontBlockMat()
    {
        return new Material(Resources.Load("Materials/orientations/front") as Material);
    }

    public static Material getBackBlockMat()
    {
        return new Material(Resources.Load("Materials/orientations/back") as Material);
    }

    internal static Material getSphereMat()
    {
        return new Material(Resources.Load("Materials/sphere") as Material);
    }
}
