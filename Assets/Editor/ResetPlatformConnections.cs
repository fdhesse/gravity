/*
 * Reset the platform Connections in RAJA game
 *
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

public class ResetPlatformConnections : MonoBehaviour {

	[MenuItem ("GameObject/RAJA: Reset platforms")]
	static void ResetPlatforms () {
		
		Platform[] platforms = FindObjectsOfType<Platform>();

		int count = 0;
		int diff = 0;
		
		for (int i = 0; i != platforms.Length; i++)
		{
			Platform platform = (Platform) platforms[i].gameObject.GetComponent<Platform>();

			diff = platform.Connections.Count;

			platform._connections = null;

			platform.connections.Clear();
			platform.Connections.Clear();

			if ( diff != platform.Connections.Count )
				count++;
		}

		Debug.Log ("ResetPlatformConnections: " + count + " platforms reseted.");
	}
}