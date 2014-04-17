using UnityEngine;
using System.Collections;

/// <summary>
/// Monobehaviour for the game object inside which the game should take place, can be a cube or anything else.
/// The Pawn is considered out of bounds when it moves out of the GameSpace object.
/// </summary>
public class Spikes : MonoBehaviour {

    void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.tag == "Player" )
	        c.gameObject.GetComponent<Pawn>().outOfBounds();
		else
			Debug.Log( "Cube reached DeadZone" );
    }
}
