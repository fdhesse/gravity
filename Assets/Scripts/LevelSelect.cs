using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour {

	GameObject targetHit;

	void Update ()
	{
		if (targetHit != null)
			targetHit.transform.localScale = Vector3.one;

		Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );
		RaycastHit hit = new RaycastHit ();

		if ( Physics.Raycast( mouseRay, out hit, float.MaxValue ))
		{
			targetHit = hit.collider.gameObject;
			targetHit.transform.localScale = Vector3.one * 1.2f;

			if ( Input.GetMouseButtonUp(0) )
			{
				Application.LoadLevel(targetHit.name);
			}
		}

	}
}
