using UnityEngine;
using System.Collections;

/// <summary>
/// Monobehaviour for the game object inside which the game should take place, can be a cube or anything else.
/// The Pawn is considered out of bounds when it moves out of the GameSpace object.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GameSpace : MonoBehaviour
{
	void Awake()
	{
		// get the bounds of this game space
		Bounds bound = this.GetComponent<Collider>().bounds;

		// make sure all the falling cube and the pawn is inside the game space,
		// otherwise log a warning, because it can creates bugs later
		GameObject pawn = GameObject.FindGameObjectWithTag("Player");
		if (pawn != null)
		{
			Debug.Assert(bound.Contains(pawn.transform.position), "WARNING: The player pawn is OUTSIDE the GameSpace limit. When the player falls out of the game, it will not be detected and the player won't respawn! Please enlarge the GameSpace bounding box.");
		}

		// also check that all falling cubes are inside the game space,
		// because pawn cannot move until all falling cube has finished to fall
		GameObject[] fallingCubes = GameObject.FindGameObjectsWithTag("FallingCube");
		foreach (GameObject cube in fallingCubes)
		{
			Debug.Assert(bound.Contains(cube.transform.position), "WARNING: A falling cube is OUTSIDE the GameSpace limit. When the cube falls out of the game, it will not be detected and the player won't be able to move anymore! Please enlarge the GameSpace bounding box.");
		}
	}

    void OnTriggerExit(Collider c)
	{
		if (c.gameObject.tag == "Player" )
	        c.gameObject.GetComponent<Pawn>().outOfBounds();
		else if (c.gameObject.tag == "FallingCube" )
			c.gameObject.GetComponent<FallingCubeBody>().OutOfBounds();
    }
}
