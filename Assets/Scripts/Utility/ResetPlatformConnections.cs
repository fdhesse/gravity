/*
 * Reset the platform Connections in RAJA game
 *
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

public class ResetPlatformConnections : MonoBehaviour {

	[MenuItem ("GameObject/RAJA: Reset platforms")] //Place the Set Pivot menu item in the GameObject menu
	static void ResetPlatforms () {
		
		Platform[] platforms = FindObjectsOfType<Platform>();

		int count = 0;
		int diff = 0;
		
		for (int i = 0; i != platforms.Length; i++)
		{
			Platform p = (Platform) platforms[i];
			
			Platform c = p.gameObject.GetComponent<Platform>();

			diff = c.Connections.Count;

			c._connections = null;

			c.connections.Clear();
			c.Connections.Clear();

			if ( diff != c.Connections.Count )
				count++;
		}

		Debug.Log ("ResetPlatformConnections: " + count + " platforms reseted.");
	}
}