using UnityEngine;
using System.Collections;

/// <summary>
/// Class in charge of the "World" 
/// </summary>
public class World : MonoBehaviour {
	
	public static float G = 40.0f;	// 9.81f		// constante gravité
	private bool isGameOver = false;		//Game state
	private FallingCube[] fallingCubes;
	private GravityPlatform[] gravityPlatforms;
	private RotatingPlatform[] rotatingPlatforms;
	private MovingPlatform[] movingPlatforms;
	private GoldTile[] goldTiles;
    private TileOrientation currentGravityOrientation = TileOrientation.Up;

	public TileOrientation CurrentGravityOrientation
	{
		get { return currentGravityOrientation; }
	}

	public void Init()
	{
		fallingCubes = FindObjectsOfType<FallingCube>();
		gravityPlatforms = FindObjectsOfType<GravityPlatform>();
		rotatingPlatforms = FindObjectsOfType<RotatingPlatform>();
		movingPlatforms = FindObjectsOfType<MovingPlatform>();
		goldTiles = FindObjectsOfType<GoldTile> ();
	}
	
	public void Restart(TileOrientation startingOrientation)
	{
		Pawn.Instance.respawn( startingOrientation );
		
		for (int i = 0; i < fallingCubes.Length; i++)
			((FallingCube) fallingCubes[i]).Reset( startingOrientation );
		
		for (int i = 0; i < gravityPlatforms.Length; i++)
			((GravityPlatform) gravityPlatforms[i]).Reset( startingOrientation );

		for (int i = 0; i < rotatingPlatforms.Length; i++)
			((RotatingPlatform) rotatingPlatforms[i]).Reset( startingOrientation );

		for (int i = 0; i < movingPlatforms.Length; i++)
			((MovingPlatform) movingPlatforms[i]).Reset( startingOrientation );

		for (int i = 0; i < goldTiles.Length; i++)
			((GoldTile)goldTiles[i]).Reset( startingOrientation );

		SetGravity( startingOrientation );
	}
	
	public bool IsGameOver()
	{
		return isGameOver;
	}
	
	public void GameOver()
	{
		isGameOver = true;
	}
	
	public void GameStart()
	{
		isGameOver = false;
		Restart(TileOrientation.Up);
	}
	
	public bool FallingCubes()
	{
		for (int i = 0; i != fallingCubes.Length; i++)
		{
			FallingCube cube = (FallingCube) fallingCubes[i];

			if ( cube.isFalling )
				return true;
				
		}
		
		return false;
	}

	
	/// <summary>
	/// Gets the gravitational orientation vector.
	/// </summary>
	public static Vector3 getGravityVector( TileOrientation vec )
	{
		switch (vec)
		{
		case TileOrientation.Up:
			return new Vector3(0, -1, 0);
		case TileOrientation.Down:
			return new Vector3(0, 1, 0);
		case TileOrientation.Left:
			return new Vector3(-1, 0, 0);
		case TileOrientation.Right:
			return new Vector3(1, 0, 0);
		case TileOrientation.Front:
			return new Vector3(0, 0, 1);
		case TileOrientation.Back:
			return new Vector3(0, 0, -1);
		default:
			return new Vector3(0, -1, 0);
		}
	}
	
	/// <summary>
	/// Sets the gravitational orientation.
	/// </summary>
	public void SetGravity(TileOrientation orientation)
	{
		// save the new gravity orientaiton
		currentGravityOrientation = orientation;

		// change the gravity vector
		Physics.gravity = getGravityVector(orientation) * G;

		// inform all the platforms of the gravity change
		for (int i = 0; i < gravityPlatforms.Length; i++)
			gravityPlatforms[i].Unfreeze( orientation );
		
		for (int i = 0; i < rotatingPlatforms.Length; i++)
			rotatingPlatforms[i].ChangeGravity( orientation );
		
		for (int i = 0; i < goldTiles.Length; i++)
			goldTiles[i].ChangeGravity( orientation );

	    foreach ( var fallingCube in fallingCubes )
	    {
	        fallingCube.ChangeGravity( orientation );
	    }
	}
}
