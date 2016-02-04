using UnityEngine;
using System.Collections;

public class InputManager
{
	/// <summary>
	/// Cross platform encapsulation of the click/tap down on screen.
	/// </summary>
	/// <returns><c>true</c>, if player has just click/tap down this frame, <c>false</c> otherwise.</returns>
	public static bool isClickDown()
	{
		#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)
		return Input.GetMouseButtonDown(0);
		#else
		return ((Input.touchCount == 1) && (Input.touches[0].phase == TouchPhase.Began));
		#endif
	}

	/// <summary>
	/// Cross platform encapsulation of the click/tap down on screen
	/// </summary>
	/// <returns><c>true</c>, if player currently hold the click/tap down, <c>false</c> otherwise.</returns>
	public static bool isClickHeldDown()
	{
		#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)
		return Input.GetMouseButton(0);
		#else
		return ((Input.touchCount == 1) && (Input.touches[0].phase == TouchPhase.Began || 
		                                    Input.touches[0].phase == TouchPhase.Moved || 
		                                    Input.touches[0].phase == TouchPhase.Stationary));
		#endif
	}
	
	/// <summary>
	/// Cross platform encapsulation of the click/tap up on screen
	/// </summary>
	/// <returns><c>true</c>, if player has click/tap up, <c>false</c> otherwise.</returns>
	public static bool isClickUp()
	{
		#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)
		return Input.GetMouseButtonUp(0);
		#else
		return ((Input.touchCount == 1) && (Input.touches[0].phase == TouchPhase.Ended || 
		                                    Input.touches[0].phase == TouchPhase.Canceled));
		#endif
	}

	/// <summary>
	/// Gets a zoom distance in world unit, based on the zoom input method correctly scaled
	/// to fit a world unit distance. Negative value if the player zoom in, positive if he zoom out.
	/// </summary>
	/// <returns>A positive or negative zoom distance or 0 if the player didn't zoom.</returns>
	public static float getZoomDistance()
	{
		return Input.GetAxis("Mouse ScrollWheel") * (-50.0f);
	}

	public static bool hasAnyInput()
	{
		#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)
		// on device with a mouse, we want to cast ray all the time
		return true;
		#else
		return (Input.touchCount == 1);
		#endif
	}
}
