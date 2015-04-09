/*
 * Reset the platform Connections in RAJA game
 *
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

public class ResetTileConnections : MonoBehaviour {

	[MenuItem ("Mu/Clear platforms connections")]
	static void ResetTiles ()
	{
		Tile[] platforms = FindObjectsOfType<Tile>();

		int count = 0;
		int diff = 0;
		
		for (int i = 0; i != platforms.Length; i++)
		{
			Tile platform = (Tile) platforms[i].gameObject.GetComponent<Tile>();

			diff = platform.Connections.Count;

			platform.Connections.Clear();

			if ( diff != platform.Connections.Count )
				count++;
		}

		Debug.Log (count + " platforms cleared.");
	}
}