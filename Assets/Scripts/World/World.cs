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
	private Waterfall[] waterfalls;
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
		waterfalls = FindObjectsOfType<Waterfall>();
		goldTiles = FindObjectsOfType<GoldTile>();
	}
	
	public void Restart(TileOrientation startingOrientation)
	{
		Pawn.Instance.Respawn( startingOrientation );
		
		foreach (var cube in fallingCubes)
			cube.Reset(startingOrientation);

		foreach (var platform in gravityPlatforms)
			platform.Reset(startingOrientation);

		foreach (var platform in rotatingPlatforms)
			platform.Reset(startingOrientation);

		foreach (var platform in movingPlatforms)
			platform.Reset(startingOrientation);

		foreach (var tile in goldTiles)
			tile.Reset(startingOrientation);

		foreach (var waterfall in waterfalls)
			waterfall.Reset(startingOrientation);

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
			if (cube.IsFalling)
				return true;
		return false;
	}

	/// <summary>
	/// Gets the gravitational normalized vector, according to the specified orientation.
	/// <param name="orientation"/>The orientation for which you want the gravity vector</param>
	/// </summary>
	public static Vector3 GetGravityNormalizedVector(TileOrientation orientation)
	{
		switch (orientation)
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
	/// Gets the gravitational vector (including the gravity acceleration), according to the specified orientation.
	/// <param name="orientation"/>The orientation for which you want the gravity vector</param>
	/// </summary>
	public static Vector3 GetGravityVector(TileOrientation orientation)
	{
		return GetGravityNormalizedVector(orientation) * G;
	}

	/// <summary>
	/// Sets the gravitational orientation.
	/// </summary>
	public void SetGravity(TileOrientation orientation)
	{
		// save the new gravity orientaiton
		currentGravityOrientation = orientation;

		// change the gravity vector
		Physics.gravity = GetGravityVector(orientation);

		// inform all the platforms of the gravity change
		foreach (var platform in gravityPlatforms)
			platform.Unfreeze(orientation);

		foreach (var platform in rotatingPlatforms)
			platform.ChangeGravity(orientation);

		foreach (var tile in goldTiles)
			tile.ChangeGravity(orientation);

		// inform all the falling cube of the change of gravity
		foreach (var cube in fallingCubes)
			cube.ChangeGravity(orientation);

		// inform the waterfalls of the gravity change
		foreach (var waterfall in waterfalls)
			waterfall.ChangeGravity(orientation);
	}

	/// <summary>
	///  Compute the relative distance from the specified tile1 to the specified tile2
	///  along the orientation of the tile2, and convert it into grid step
	///  (currently one gameplay cube'edge equals to 2m (aka GameplayCube.CUBE_SIZE in world's coordinates). 
	///  - For two tiles with the same orientation, the value is positive if tile1 is above tile2 
	///    (according to orientation of tile2), negative otherwise.
	///  - For two tiles whose orientations are perpendicular, if tile2 is a touching wall of tile1 
	///    then the distance will be null.
	/// If the tile2 is a Falling Cube, this function assume that it will fall along its own orientation,
	/// therefore will compute the distance for when it will reach a ground. And if there's no ground
	/// under the falling cube, then int.MaxValue is returned.
	/// </summary>
	/// <param name="tile1">The first tile you want to test</param>
	/// <param name="tile2">The second tile you want to test</param>
	/// <returns>a number of grid step representing the difference of distance (along tile2's orientation) between the two tiles.</returns>
	public int GetTileRelativeGridDistance(Tile tile1, Tile tile2)
	{
		// compute the tile position difference
		Vector3 tileDiff = tile1.Position - tile2.Position;

		// get the correct height according to the gravity
		float distance = 0f;
		switch (tile2.orientation)
		{
			case TileOrientation.Up:	distance = tileDiff.y; break;
			case TileOrientation.Down:	distance = -tileDiff.y; break;
			case TileOrientation.Left:	distance = tileDiff.x; break;
			case TileOrientation.Right: distance = -tileDiff.x; break;
			case TileOrientation.Back:	distance = tileDiff.z; break;
			case TileOrientation.Front: distance = -tileDiff.z; break;
		}

		// if the two tiles are aligned on the same axis, nothing need to be adjusted,
		// but if the two tiles have perpendicular orientation, then we need to adjust the
		// distance by half a cube size.
		if (!AreTileOrientedOnTheSameAxis(tile1, tile2))
		{
			if (distance > 0f)
				distance -= GameplayCube.HALF_CUBE_SIZE;
			else
				distance += GameplayCube.HALF_CUBE_SIZE;
		}

		// convert the distance into grid step
		int result = (int)Mathf.Round(distance / GameplayCube.CUBE_SIZE);

		// now check if the tile2 is a falling cube. If it's the case, we need to check where it will fall
		if (tile2.CompareTag(GameplayCube.FALLING_CUBE_TAG))
		{
			Vector3 direction = GetGravityNormalizedVector(tile2.orientation);
			Vector3 origin = tile2.transform.parent.position + (direction * GameplayCube.HALF_CUBE_SIZE * 0.95f);
			RaycastHit hitInfo;
			if (Physics.Raycast(origin, direction, out hitInfo))
			{
				// if we hit something, add the grid height of the hit distance, to what we have computed so far
				result += (int)Mathf.Round(hitInfo.distance / GameplayCube.CUBE_SIZE);
			}
			else
			{
				// if we didn't find any collider under the falling cube, just return the infinite value
				result = int.MaxValue;
			}
		}

		// return the result
		return result;
	}

	/// <summary>
	/// This function tells you if the two specified tiles are oriented on the same axis, for example up and down are on the same axis.
	/// If the two specified tiles have equals orientation, for example both are left, of course it also return true.
	/// The couples that return true are (up/down), (left/right) and (front/back).
	/// The function will also return true if both orientation equals "none", but will return false if only one equals "none".
	/// </summary>
	/// <param name="tile1">the first tile to test</param>
	/// <param name="tile2">the second tile to test</param>
	/// <returns><c>true</c> if both specified tile's orientation are on the same axis.</returns>
	public bool AreTileOrientedOnTheSameAxis(Tile tile1, Tile tile2)
	{
		// Because Up/Down, Left/Right and Back/Front are grouped together in the enum,
		// we can substract 1 to even values to have a common value for each pair, and then test these common value
		int tile1AxisAlignment = ((int)tile1.orientation % 2) == 0 ? (int)tile1.orientation - 1 : (int)tile1.orientation;
		int tile2AxisAlignment = ((int)tile2.orientation % 2) == 0 ? (int)tile2.orientation - 1 : (int)tile2.orientation;

		// return true if the two values are equal
		return (tile1AxisAlignment == tile2AxisAlignment);
	}
}
