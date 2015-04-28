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
	private GoldTile[] goldTiles;
	
	private static Pawn playerPawn; // Player Pawn
	public static Pawn Pawn
	{
		get { return playerPawn; }
	}

	void Awake()
	{
		fallingCubes = FindObjectsOfType<FallingCube>();
		gravityPlatforms = FindObjectsOfType<GravityPlatform>();
		rotatingPlatforms = FindObjectsOfType<RotatingPlatform>();
		goldTiles = FindObjectsOfType<GoldTile> ();
	}
	
	public static void Init( Pawn player )
	{
		playerPawn = player;
	}
	
	public void Restart()
	{
		playerPawn.respawn();
		
		for (int i = 0; i < fallingCubes.Length; i++)
			((FallingCube) fallingCubes[i]).Reset();
		
		for (int i = 0; i < gravityPlatforms.Length; i++)
			((GravityPlatform) gravityPlatforms[i]).Reset();

		for (int i = 0; i < rotatingPlatforms.Length; i++)
			((RotatingPlatform) rotatingPlatforms[i]).Reset();

		for (int i = 0; i < goldTiles.Length; i++)
			((GoldTile)goldTiles [i]).Reset ();

		ChangeGravity ( playerPawn.orientation );
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
		Restart();
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
		default:
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
		}
	}
	
	public void ChangeGravity( TileOrientation orientation )
	{
		for (int i = 0; i < gravityPlatforms.Length; i++)
			((GravityPlatform) gravityPlatforms[i]).Unfreeze( orientation );
		
		for (int i = 0; i < rotatingPlatforms.Length; i++)
			((RotatingPlatform) rotatingPlatforms[i]).SendMessage( "ChangeGravity", orientation );
		
		for (int i = 0; i < goldTiles.Length; i++)
			((GoldTile) goldTiles[i]).SendMessage( "ChangeGravity", orientation );
	}
	
	/// <summary>
	/// Sets the gravitational orientation vector.
	/// </summary>
	public static void SetGravity(TileOrientation orientation)
	{
		switch (orientation)
		{
		default:
			break;
		case TileOrientation.Front:
			Physics.gravity = new Vector3(0, 0, G);
			break;
		case TileOrientation.Back:
			Physics.gravity = new Vector3(0, 0, -G);
			break;
		case TileOrientation.Right:
			Physics.gravity = new Vector3(G, 0, 0);
			break;
		case TileOrientation.Left:
			Physics.gravity = new Vector3(-G, 0, 0);
			break;
		case TileOrientation.Up:
			Physics.gravity = new Vector3(0, -G, 0);
			break;
		case TileOrientation.Down:
			Physics.gravity = new Vector3(0, G, 0);
			break;
		}
	}
}
