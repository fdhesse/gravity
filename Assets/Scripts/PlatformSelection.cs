using UnityEngine;
using System.Collections;

/// <summary>
/// This is more of a auxilliary class for handling platform selection
/// </summary>
public static class PlatformSelection
{
    private static Platform platform;//currently selected platform
    private static GameObject cam;//camera for this scene

    static PlatformSelection()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera"); //initialize the camera
    }

    /// <summary>
    /// Highlights target platform
    /// </summary>
    public static void highlightTargetPlatform()
    {
        if (platform != null)
        {
            platform.unHighlight();
        }

        platform = getPlatform();
    }

    /// <summary>
    /// Gets the platform that is being targeted.
    /// </summary>
    public static Platform getPlatform()
    {
        Platform p = null;
        if (cam == null)//if there isn't a camera associated with this script, get the main camera
        {
            cam = GameObject.FindGameObjectWithTag("MainCamera");
        }
        Ray mouseRay = cam.camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
//		if (Physics.Raycast(mouseRay, out hit, float.MaxValue, ~(1 << 11))) // cast a raycast ignoring the layer for the DeathZone
		if (Physics.Raycast(mouseRay, out hit, float.MaxValue, (1 << 14))) // cast a raycast ignoring all but the layer for the platforms
        {
            p = hit.collider.gameObject.GetComponent<Platform>();
            
            if (p != null) //if it is a platform
            {
                p.highlight();
            }
        }
        return p;
    }

}
