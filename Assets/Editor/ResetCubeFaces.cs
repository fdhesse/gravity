/*
 * Reset the platform Faces in RAJA game
 *
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

public class ResetCubeFaces : MonoBehaviour {

	[MenuItem ("GameObject/RAJA: Reset cubes faces")]
	static void ResetCubes () {
		
		GameplayCube[] cubes = FindObjectsOfType<GameplayCube>();

		int count = 0;
		
		for (int i = 0; i != cubes.Length; i++)
		{
			GameplayCube cube = (GameplayCube) cubes[i];
			
			cube.transform.localScale = new Vector3( 10.0f, 10.0f, 10.0f );
			
			foreach ( Tile platform in cube.GetComponentsInChildren<Tile>() )
			{
				count++;
				
				platform.gameObject.layer = 14;
				platform.transform.localScale = new Vector3( 0.1f, 0.1f, 0.1f );
				
				// Correct Unity behaviour which slighty offset transform's position
				if ( platform.transform.localPosition.x < 0.1f && platform.transform.localPosition.x > -0.1f )
					platform.transform.localPosition = new Vector3 ( 0, platform.transform.localPosition.y, platform.transform.localPosition.z );
				if ( platform.transform.localPosition.y < 0.1f && platform.transform.localPosition.y > -0.1f )
					platform.transform.localPosition = new Vector3 ( platform.transform.localPosition.x, 0, platform.transform.localPosition.z );
				if ( platform.transform.localPosition.z < 0.1f && platform.transform.localPosition.z > -0.1f )
					platform.transform.localPosition = new Vector3 ( platform.transform.localPosition.x, platform.transform.localPosition.y, 0 );
				
				if ( platform.transform.localPosition.x > 0 && platform.transform.localPosition.x < 0.55f )
				{
					platform.transform.localPosition = new Vector3 ( 0.5f, 0, 0 );
					continue;
				}
				else if ( platform.transform.localPosition.x < 0 && platform.transform.localPosition.x > -0.55f )
				{
					platform.transform.localPosition = new Vector3 ( -0.5f, 0, 0 );
					continue;
				}
				else if ( platform.transform.localPosition.y > 0 && platform.transform.localPosition.y < 0.55f )
				{
					platform.transform.localPosition = new Vector3 ( 0,  0.5f, 0 );
					continue;
				}
				else if ( (platform.transform.localPosition.y < 0 ) && (platform.transform.localPosition.y > -0.55f) )
				{
					platform.transform.localPosition = new Vector3 ( 0, -0.5f, 0 );
					continue;
				}
				else if ( platform.transform.localPosition.z > 0 && platform.transform.localPosition.z < 0.55f )
				{
					platform.transform.localPosition = new Vector3 ( 0, 0, 0.5f );
					continue;
				}
				else if ( platform.transform.localPosition.z < 0 && platform.transform.localPosition.z > -0.55f )
				{
					platform.transform.localPosition = new Vector3 ( 0, 0, -0.5f );
					continue;
				}

				/*				
				for ( int axis in platform.transform.position )
				{
					if ( axis != 0 )
						axis = ( axis > 0 ) ? axis - 0.2 : axis + 0.2;
					else
						continue;
				}
				*/
			}
		}

		Debug.Log ("ResetCubeFaces: " + count + " faces reseted.");
	}
}