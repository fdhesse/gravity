using UnityEngine;
using System.Collections;

public class Particules : MonoBehaviour {

	private ParticleAnimator[] animators;

	public bool upsideDown = false;
	public float intensity = 1.0f;

	// Use this for initialization
	void Start () {
		animators = this.GetComponentsInChildren<ParticleAnimator> ();// this.GetComponents<Particules>();
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 gravity = Physics.gravity;
		if (upsideDown)
			gravity.y = -gravity.y;
		
		for (int i = 0; i != animators.Length; i++)
		{
			animators [i].force = gravity * intensity;
		}
	}
}
