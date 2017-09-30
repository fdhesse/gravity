using UnityEngine;

public class Waterfall : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The particle system that generate the water particles for normal cases")]
	private ParticleSystem m_StreamWaterEmitter = null;

	[SerializeField]
	[Tooltip("The particle system that generate the water particles when the gravity is against the water direction")]
	private ParticleSystem m_AgaintGravityWaterEmitter = null;

	// we need a copy of the Water emiter to make a smooth transition when we change gravity
	private ParticleSystem m_DuplicatedStreamWaterEmitter = null;

	// memorize the current emitter
	private ParticleSystem m_CurrentEmitterPlaying = null;

	// save the 3 size over time curve, that we intervert during gravity change
	private ParticleSystem.MinMaxCurve m_SizeCurveForWidth;
	private ParticleSystem.MinMaxCurve m_SizeCurveAlongGravity;
	private ParticleSystem.MinMaxCurve m_SizeCurveAlongEmissionDirection;

	// a flag to tell if the emitter should be prewarmed during the next gravity change because when the world init, both Reset and ChangeGravity are called
	private bool m_InitEmitterDuringNextGravityChange = false;

	private void Awake()
	{
		// copy all the original curve (the VFX is designed in the editor to have the gravity
		// along Y and to emit along Z)
		m_SizeCurveForWidth = m_StreamWaterEmitter.sizeOverLifetime.x;
		m_SizeCurveAlongGravity = m_StreamWaterEmitter.sizeOverLifetime.y;
		m_SizeCurveAlongEmissionDirection = m_StreamWaterEmitter.sizeOverLifetime.z;

		// duplicate the emitter
		m_DuplicatedStreamWaterEmitter = Instantiate(m_StreamWaterEmitter, m_StreamWaterEmitter.transform.parent);

		// and take a reference on the first emitter as the current one
		m_CurrentEmitterPlaying = m_StreamWaterEmitter;		
	}

	public void Reset(TileOrientation startingOrientation)
	{
		// stop and clear all the emitter
		m_StreamWaterEmitter.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		m_DuplicatedStreamWaterEmitter.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		m_AgaintGravityWaterEmitter.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

		// During world init, both Reset and SetGravity is called. We need to wait for the physical gravity
		// to be reset before initializing the particle emitter. So set a flag to tell that the next GravityChange
		// call will be the one called after the reset
		m_InitEmitterDuringNextGravityChange = true;
	}

	public void ChangeGravity(TileOrientation orientation)
	{
		// check if we need to ignore this gravity change
		if (m_InitEmitterDuringNextGravityChange)
		{
			// reset the flag
			m_InitEmitterDuringNextGravityChange = false;
			// set the correct curve along the starting gravity
			SetCurveAccordingToGravityForCurrentEmitter(orientation);
			// restart the current emitter with prewarm
			StartCurrentEmitter(true);
		}
		else
		{
			SwitchEmitter(orientation);
			SetCurveAccordingToGravityForCurrentEmitter(orientation);
		}
	}

	private void SetCurveAccordingToGravityForCurrentEmitter(TileOrientation orientation)
	{
		// if we don't play the against gravity, emitter, we need to set the correct curves
		if (m_CurrentEmitterPlaying != m_AgaintGravityWaterEmitter)
		{
			var sizeOverLifetime = m_CurrentEmitterPlaying.sizeOverLifetime;

			// intervert the size over life time curves depending on the orientation of the gravity
			switch (orientation)
			{
				case TileOrientation.Up:
				case TileOrientation.Down:
					sizeOverLifetime.x = m_SizeCurveForWidth;
					sizeOverLifetime.y = m_SizeCurveAlongGravity;
					sizeOverLifetime.z = m_SizeCurveAlongEmissionDirection;
					break;

				case TileOrientation.Left:
				case TileOrientation.Right:
					sizeOverLifetime.x = m_SizeCurveAlongGravity;
					sizeOverLifetime.y = m_SizeCurveForWidth;
					sizeOverLifetime.z = m_SizeCurveAlongEmissionDirection;
					break;

				case TileOrientation.Front:
				case TileOrientation.Back:
					sizeOverLifetime.x = m_SizeCurveForWidth;
					sizeOverLifetime.y = m_SizeCurveAlongEmissionDirection;
					sizeOverLifetime.z = m_SizeCurveAlongGravity;
					break;
			}
		}
	}

	private void SwitchEmitter(TileOrientation orientation)
	{
		// stop the current emitter and let the particle die
		m_CurrentEmitterPlaying.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		// switch
		if (orientation == TileOrientation.Back)
			m_CurrentEmitterPlaying = m_AgaintGravityWaterEmitter;
		else
			m_CurrentEmitterPlaying = (m_CurrentEmitterPlaying == m_StreamWaterEmitter) ? m_DuplicatedStreamWaterEmitter : m_StreamWaterEmitter;
		// start the new emitter
		StartCurrentEmitter(false);
	}

	private void StartCurrentEmitter(bool usePrewarm)
	{
		// set the use prewarn to the current emitter and his children
		var allEmitters = m_CurrentEmitterPlaying.GetComponentsInChildren<ParticleSystem>();
		foreach (var emitter in allEmitters)
		{
			var mainModule = emitter.main;
			mainModule.prewarm = usePrewarm;
		}
		// and start the current emitter
		m_CurrentEmitterPlaying.Play();
	}
}
