﻿using UnityEngine;
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
		World.SetGravity( startingOrientation );

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
		default:
			break;
		}
	}
}
