using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour {

	GameObject targetHit;
	Color32 defaultColor;
	Color32 highlightColor;

	void Start ()
	{
		defaultColor = new Color32 (255, 161, 88, 255);
		//highlightColor = new Color32 (255, 185, 0, 255);
		highlightColor = new Color32 (255, 0, 0, 255);
	}

	void Update ()
	{
		if (targetHit != null)
			UnhighlightLevel (targetHit);
			//targetHit.transform.localScale = Vector3.one;

		Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );
		RaycastHit hit = new RaycastHit ();

		if ( Physics.Raycast( mouseRay, out hit, float.MaxValue ))
		{
			targetHit = hit.collider.gameObject;
			HighlightLevel( targetHit );
			//targetHit.transform.localScale = Vector3.one * 1.2f;

			if ( InputManager.isClickUp() )
				Application.LoadLevel(targetHit.name);
		}

	}
	
	private void HighlightLevel( GameObject target )
	{
		target.GetComponent<Renderer>().material.color = highlightColor;
		
		Renderer[] childrenRenderers = target.transform.FindChild("text").GetComponentsInChildren<Renderer> ();
		
		foreach ( Renderer renderer in childrenRenderers )
		{
			renderer.enabled = true;
		}
	}
	
	private void UnhighlightLevel( GameObject target )
	{
		targetHit.GetComponent<Renderer>().material.color = defaultColor;

		Renderer[] childrenRenderers = target.transform.FindChild("text").GetComponentsInChildren<Renderer> ();
		
		foreach ( Renderer renderer in childrenRenderers )
		{
			renderer.enabled = false;
		}
	}
	
	public void ExitGame()
	{
		//Debug.Log ("Exit now");
		Application.Quit();
	}
}
