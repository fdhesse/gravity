using UnityEngine;

[System.Serializable]
public class StringAudioSourceDictionary : MonoDictionary<string, AudioSource> { }

[System.Serializable]
public class StringRandomSoundPitcherDictionary : MonoDictionary<string, RandomSoundPitcher> { }

/// <summary>
/// This class can receive event from animation to play sounds defined in its parameters.
/// The sounds are defined in a dictionnary list, so that designer can directly add an new
/// anim event, without code modification.
/// </summary>
public class PawnSound : MonoBehaviour
{
	[Header("Sounds for 'OnPlaySound' Event")]
	[Tooltip("The list of Audio source, associated with a sound id sent in the anim event.")]
	[SerializeField]
	private StringAudioSourceDictionary m_SimpleSound = null;

	[Tooltip("The list of randomized Audio Source, associated with a sound id sent in the anim event.")]
	[SerializeField]
	private StringRandomSoundPitcherDictionary m_RandomizedSound = null;

	[Header("Sounds for 'OnStepSound' Event")]
	[Tooltip("The sounds played when the player make a step on a default material.")]
	[SerializeField]
	private RandomSoundPitcher m_StepSound = null;

	#region anim Event
	public void OnStepSound()
	{
		m_StepSound.Play();
	}

	public void OnPlaySound(string soundId)
	{
		// try to find the sound in simple liste first, and play it
		AudioSource sound = null;
		if (m_SimpleSound.TryGetValue(soundId, out sound))
		{
			sound.Play();
		}
		else
		{
			// if not found in simple list, try to find it in random list, and play it
			RandomSoundPitcher randomSound = null;
			if (m_RandomizedSound.TryGetValue(soundId, out randomSound))
				randomSound.Play();
		}
	}
	#endregion
}
