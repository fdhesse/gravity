using UnityEngine;
using System.Collections;

public class NarrativeTrigger : MonoBehaviour {
	
	[TextArea(1,5)]
	public string[] TextPages;

	void OnTriggerEnter( Collider other )
	{
		if (other.tag != "Player")
			return;

		//GameObject.
		((HUD)GameObject.FindWithTag("HUD").GetComponent<HUD>()).DisplayNarrativeText(TextPages);

		Destroy( gameObject );
	}
}
