using UnityEngine;
using System.Collections;

[System.Serializable]
public class RandomSoundPitcher
{
	[Tooltip("Minimum pitch that will be used to play one of the sound randomly.")]
	public float minPitch = 1.0f;
	[Tooltip("Maximum pitch that will be used to play one of the sound randomly.")]
	public float maxPitch = 1.0f;
	[Tooltip("The list of audio clip that will be used. Each time the sound needs to play one of the list will be picked randomly.")]
	public AudioClip[] randomSound;
	[Tooltip("Minimum delay in second between two consecutive play. If the delay is not respected, the sound will not be played and ignored.")]
	public float minDelayBetweenSound = 0.2f;

	private float mLastPlayedTime = 0f;

	public void playSound(AudioSource audio)
	{
		// check the last time a sound was played to know if we should skip it
		if (Time.time > mLastPlayedTime + minDelayBetweenSound)
		{
			// memorized the play time
			mLastPlayedTime = Time.time;

			// set the random pitch of the audio source
			audio.pitch = Random.Range(minPitch, maxPitch);

			// if the random sound array is not empty, also set a random clip, otherwise play the current one
			if (randomSound.Length > 0)
			{
				AudioClip clipToPlay = randomSound[Random.Range(0, randomSound.Length)];
				audio.PlayOneShot(clipToPlay, audio.volume);
			}
			else
			{
				audio.Play();
			}
		}
	}
}
