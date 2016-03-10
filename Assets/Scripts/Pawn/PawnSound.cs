using UnityEngine;
using System.Collections;

public class PawnSound : MonoBehaviour
{
	
	public RandomSoundPitcher stepSound = null;
	public AudioClip LandSound = null;

	private AudioSource mAudioSource;

	void Awake()
	{
		mAudioSource = GetComponent<AudioSource>();
	}

	#region anim Event
	public void onStepEvent(int param)
	{
		stepSound.playSound(mAudioSource);
	}

	public void onLandEvent(int param)
	{
		mAudioSource.pitch = 1f;
		mAudioSource.PlayOneShot(LandSound, mAudioSource.volume);
	}
	#endregion
}
