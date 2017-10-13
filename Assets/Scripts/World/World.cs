﻿using UnityEngine;

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
			if (cube.isFalling)
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

		// inform the waterfalls of the gravity change
		foreach (var waterfall in waterfalls)
			waterfall.ChangeGravity(orientation);
	}

	/// <summary>
	///  Compute the relative height from the specified tile1 to the specified tile2
	///  along the specified height direction, and convert it into grid step 
	///  (currently one tile is 10m in world's coordinates). 
	///  The value is positive if tile1 is above tile2, negative otherwise.
	/// </summary>
	/// <param name="tile1">The first tile you want to test</param>
	/// <param name="tile2">The second tile you want to test</param>
	/// <param name="heightDirection">The direction along which you want to know the height. If this parameter equals <c>TileOrientation.None</c>, the current world's gravity will be used instead.</param>
	/// <returns>a number of grid step representing the difference of height (along current world's gravity) betweent the two tiles.</returns>
	public int GetTileRelativeGridHeight(Tile tile1, Tile tile2, TileOrientation heightDirection = TileOrientation.None)
	{
		// if the height direction is not specified, used the current gravity
		if (heightDirection == TileOrientation.None)
			heightDirection = currentGravityOrientation;

		// compute the tile position difference
		Vector3 distance = tile1.Position - tile2.Position;

		// get the correct height according to the gravity
		float height = 0f;
		switch (heightDirection)
		{
			case TileOrientation.Up:	height = distance.y; break;
			case TileOrientation.Down:	height = -distance.y; break;
			case TileOrientation.Left:	height = distance.x; break;
			case TileOrientation.Right: height = -distance.x; break;
			case TileOrientation.Back:	height = distance.z; break;
			case TileOrientation.Front: height = -distance.z; break;
		}

		// convert the distance into grid step
		return (int)Mathf.Round(height / GameplayCube.CUBE_SIZE);
	}
}
