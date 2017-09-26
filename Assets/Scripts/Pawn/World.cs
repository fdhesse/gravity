using UnityEngine;

/// <summary>
/// Class in charge of the "World" 
/// </summary>
public class World : MonoBehaviour
{
	private static World s_Instance = null;
	public static World Instance { get { return s_Instance; } }

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

	void Awake()
	{
		s_Instance = this;

		// init the various list of element of the world
		fallingCubes = FindObjectsOfType<FallingCube>();
		gravityPlatforms = FindObjectsOfType<GravityPlatform>();
		rotatingPlatforms = FindObjectsOfType<RotatingPlatform>();
		movingPlatforms = FindObjectsOfType<MovingPlatform>();
		goldTiles = FindObjectsOfType<GoldTile>();
	}
	
	public void Restart(TileOrientation startingOrientation)
	{
		Pawn.Instance.Respawn( startingOrientation );
		
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
	
	/// <summary>
	/// Make the game over. This will bring the fadeout if the player failed (like dying by falling
	/// or being crushed), or may bring the result page if the player has succeeded.
	/// </summary>
	/// <param name="hasPlayerWin">Should be <c>true</c> if the player as successfully completed the level, or false if he died and need to restart the level</param>
	public void GameOver(bool hasPlayerWin)
	{
		// set the game over flag
		isGameOver = true;

		// activate the end screen or the fade out depending if player won or lose
		if (hasPlayerWin)
			HUD.Instance.ShowResultPage();
		else
			HUD.Instance.StartFadeOut();
	}

	public void GameStart()
	{
		isGameOver = false;
		Restart(TileOrientation.Up);
	}
	
	public bool IsThereAnyCubeFalling()
	{
		foreach (FallingCube cube in fallingCubes)
			if (cube.isFalling)
				return true;
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
	}
}
