using UnityEngine;
using System.Collections;

/// <summary>
/// Monobehaviour for the spikes
/// </summary>
public class Spikes : MonoBehaviour
{
    void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.tag == "Player" )
	        c.gameObject.GetComponent<Pawn>().DieOnSpikes();
    }
}
