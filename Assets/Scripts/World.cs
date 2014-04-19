using UnityEngine;
using System.Collections;

/// <summary>
/// Class in charge of the "World" 
/// </summary>
public class World : MonoBehaviour {
	
	public float G = 40.0f;	// 9.81f		// constante gravité
	private bool isGameOver = false;		//Game state
	private Cube[] cubes;
	
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
		cubes = FindObjectsOfType<Cube>();
		PlayerPawn = (Pawn) GameObject.Find ("Pawn").GetComponent<Pawn>();
	}
	
	public void Restart()
	{
		PlayerPawn.respawn();
		
		for (int i = 0; i != cubes.Length; i++)
		{
			Cube cube = (Cube) cubes[i];
			cube.Reset();
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
			Cube cube = (Cube) cubes[i];

			if ( cube.isFalling )
				return true;
				
		}
		
		return false;
	}
}
