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
		// for now simply change the orientation
		ChangeGravity(startingOrientation);
		StartCurrentEmitter(true);
	}

	public void ChangeGravity(TileOrientation orientation)
	{
		SwitchEmitter(orientation);

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
		var mainModule = m_CurrentEmitterPlaying.main;
		mainModule.prewarm = usePrewarm;
		m_CurrentEmitterPlaying.Play();
	}
}
