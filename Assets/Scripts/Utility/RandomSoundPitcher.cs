using UnityEngine;

/// <summary>
/// This class is a small helper for grouping various similar sound (AudioClip), such as hit sound or step sound,
/// so that a random sound with a random pitch is played each time, we need to play it.
/// </summary>
[System.Serializable]
public class RandomSoundPitcher
{
	[Tooltip("The audio source to use for playing those various randomized sounds.")]
	[SerializeField]
	private AudioSource m_AudioSource = null;

	[Tooltip("Minimum pitch that will be used to play one of the sound randomly.")]
	[SerializeField]
	private float m_MinPitch = 1.0f;

	[Tooltip("Maximum pitch that will be used to play one of the sound randomly.")]
	[SerializeField]
	private float m_MaxPitch = 1.0f;

	[Tooltip("The list of audio clip that will be used. Each time the sound needs to play one of the list will be picked randomly.")]
	[SerializeField]
	private AudioClip[] m_RandomSound = null;

	[Tooltip("Minimum delay in second between two consecutive play. If the delay is not respected, the sound will not be played and ignored.")]
	[SerializeField]
	private float m_MinDelayBetweenSound = 0.2f;

	private float m_LastPlayedTime = 0f;

	public void Play()
	{
		// do nothing if there's no audio sources
		if (m_AudioSource == null)
			return;

		// check the last time a sound was played to know if we should skip it
		if (Time.time > m_LastPlayedTime + m_MinDelayBetweenSound)
		{
			// memorized the play time
			m_LastPlayedTime = Time.time;

			// set the random pitch of the audio source
			m_AudioSource.pitch = Random.Range(m_MinPitch, m_MaxPitch);

			// if the random sound array is not empty, also set a random clip, otherwise play the current one
			if (m_RandomSound.Length > 0)
			{
				AudioClip clipToPlay = m_RandomSound[Random.Range(0, m_RandomSound.Length)];
				m_AudioSource.PlayOneShot(clipToPlay);
			}
			else
			{
				m_AudioSource.Play();
			}
		}
	}
}
