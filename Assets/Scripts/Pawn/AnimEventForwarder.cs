using UnityEngine;

public class AnimEventForwarder : MonoBehaviour
{
	private PawnSound m_PawnSound = null;

	void Awake()
	{
		m_PawnSound = transform.GetComponentInParent<PawnSound>();
	}

    #region sound event
    void OnPlaySound(string soundId)
	{
		m_PawnSound.OnPlaySound(soundId); 
	}

	void OnStepSound()
	{
		m_PawnSound.OnStepSound();
	}
	#endregion    
}
