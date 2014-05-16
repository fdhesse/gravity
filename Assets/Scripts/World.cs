using UnityEngine;
using System.Collections;

/// <summary>
/// Class in charge of the "World" 
/// </summary>
public class World : MonoBehaviour {
	
	public float G = 40.0f;	// 9.81f		// constante gravité
	private bool isGameOver = false;		//Game state
	private FallingCube[] cubes;
	private GravityPlatform[] gravityPlatforms;
	
	private Pawn PlayerPawn; // Player Pawn
	
	// Use this for initialization
	void Start () {
//		gameObject.AddComponent( "Editor" );
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void Init()
	{
		cubes = FindObjectsOfType<FallingCube>();
		gravityPlatforms = FindObjectsOfType<GravityPlatform>();
		PlayerPawn = (Pawn) GameObject.Find ("Pawn").GetComponent<Pawn>();
	}
	
	public void Restart()
	{
		PlayerPawn.respawn();
		
		for (int i = 0; i != cubes.Length; i++)
		{
			FallingCube cube = (FallingCube) cubes[i];
			cube.Reset();
		}

		for (int i = 0; i != gravityPlatforms.Length; i++)
		{
			GravityPlatform gPlatform = (GravityPlatform) gravityPlatforms[i];
			gPlatform.Reset();
		}
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
		for (int i = 0; i != cubes.Length; i++)
		{
			FallingCube cube = (FallingCube) cubes[i];

			if ( cube.isFalling )
				return true;
				
		}
		
		return false;
	}

	public void ChangeGravity()
	{
		for (int i = 0; i != gravityPlatforms.Length; i++)
		{
			GravityPlatform gPlatform = (GravityPlatform) gravityPlatforms[i];
			gPlatform.Unfreeze();
		}
	}
}
