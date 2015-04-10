/*
 * Reset the platform Connections in RAJA game
 *
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

public class ResetTileConnections : MonoBehaviour {

	[MenuItem ("GameObject/RAJA: Reset platforms")]
	static void ResetTiles () {
		
		Tile[] platforms = FindObjectsOfType<Tile>();

		int count = 0;
		int diff = 0;
		
		for (int i = 0; i != platforms.Length; i++)
		{
			Tile platform = (Tile) platforms[i].gameObject.GetComponent<Tile>();

			diff = platform.Connections.Count;

			platform._connections = null;

			platform.connections.Clear();
			platform.Connections.Clear();

			if ( diff != platform.Connections.Count )
				count++;
		}

		Debug.Log ("ResetTileConnections: " + count + " platforms reseted.");
	}
}