using UnityEngine;

public class Waterfall : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The particle system that generate the water particles")]
	private ParticleSystem m_WaterEmitter = null;

	// we need a copy of the Water emiter to make a smooth transition when we change gravity
	private ParticleSystem m_DuplicatedWaterEmitter = null;

	private ParticleSystem m_CurrentEmitterPlaying = null;

	// save the 3 size over time curve, that we intervert during gravity change
	private ParticleSystem.MinMaxCurve m_SizeCurveForWidth;
	private ParticleSystem.MinMaxCurve m_SizeCurveAlongGravity;
	private ParticleSystem.MinMaxCurve m_SizeCurveAlongEmissionDirection;
	
	private void Awake()
	{
		// copy all the original curve (the VFX is designed in the editor to have the gravity
		// along Y and to emit along Z)
		m_SizeCurveForWidth = m_WaterEmitter.sizeOverLifetime.x;
		m_SizeCurveAlongGravity = m_WaterEmitter.sizeOverLifetime.y;
		m_SizeCurveAlongEmissionDirection = m_WaterEmitter.sizeOverLifetime.z;

		// duplicate the emitter
		m_DuplicatedWaterEmitter = Instantiate(m_WaterEmitter, m_WaterEmitter.transform.parent);

		// and take a reference on the first emitter as the current one
		m_CurrentEmitterPlaying = m_WaterEmitter;		
	}

	public void Reset(TileOrientation startingOrientation)
	{
		// for now simply change the orientation
		ChangeGravity(startingOrientation);
		StartCurrentEmitter(true);
	}

	public void ChangeGravity(TileOrientation orientation)
	{
		SwitchEmitter();

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

	private void SwitchEmitter()
	{
		// stop the current emitter and let the particle die
		m_CurrentEmitterPlaying.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		// switch
		m_CurrentEmitterPlaying = (m_CurrentEmitterPlaying == m_WaterEmitter) ? m_DuplicatedWaterEmitter : m_WaterEmitter;
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
