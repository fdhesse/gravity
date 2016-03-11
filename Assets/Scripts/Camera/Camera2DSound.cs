using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * This class manage the play of 2D sounds. In Unity all sounds are 3D, to make 2D sounds, the audio source must
 * be attached to the same game object as the audio listener, which is the camera most of the time
 */
[RequireComponent(typeof(AudioSource))]
public class Camera2DSound : MonoBehaviour
{
	public enum SoundId
	{
		CLICK_TILE = 0,
		GRAVITY_CHANGE,
	}

	public AudioClip clickTile = null;
	public AudioClip gravityChange = null;

	private AudioSource mAudioSourceTemplate = null;
	private List<AudioSource> mAudioSources = new List<AudioSource>(5);

	void Awake()
	{
		// get the first audio source as a template and add it to the list
		mAudioSourceTemplate = GetComponent<AudioSource>();
		mAudioSources.Add(mAudioSourceTemplate);
	}

	public void playSound(AudioClip clip)
	{
		// get a free audio source
		AudioSource source = getFreeAudioSource();
		// play the clip with it
		source.PlayOneShot(clip, source.volume);
	}

	public void playSound(SoundId clipId)
	{
		switch (clipId)
		{
		case SoundId.CLICK_TILE: playSound(clickTile); break;
		case SoundId.GRAVITY_CHANGE: playSound(gravityChange); break;
		}
	}

	private AudioSource getFreeAudioSource()
	{
		// first look in the list if an audio source not playing is available
		foreach (AudioSource source in mAudioSources)
			if (!source.isPlaying)
				return source;
		
		// other wise spawn a new source
		AudioSource newSource = gameObject.AddComponent<AudioSource>();
		newSource.outputAudioMixerGroup = mAudioSourceTemplate.outputAudioMixerGroup;
		newSource.priority= mAudioSourceTemplate.priority;
		newSource.volume = mAudioSourceTemplate.volume;
		newSource.pitch = mAudioSourceTemplate.pitch;
		newSource.panStereo = mAudioSourceTemplate.panStereo;
		newSource.spatialBlend = mAudioSourceTemplate.spatialBlend;
		newSource.reverbZoneMix = mAudioSourceTemplate.reverbZoneMix;

		// add it to the list
		mAudioSources.Add(newSource);

		// and return it
		return newSource;
	}
}
