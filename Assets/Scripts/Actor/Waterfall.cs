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

	// a flag to tell if the emitter should be prewarmed during the next gravity change because when the world init, both Reset and ChangeGravity are called
	private bool m_InitEmitterDuringNextGravityChange = false;

	private void Awake()
	{
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
		// switch emitter to choose the correct one at init time
		SwitchEmitter(orientation, m_InitEmitterDuringNextGravityChange);

		// reset the flag after calling the switch emitter
		m_InitEmitterDuringNextGravityChange = false;
	}

	private void SwitchEmitter(TileOrientation orientation, bool usePrewarm)
	{
		// stop the current emitter and let the particle die
		m_CurrentEmitterPlaying.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		
		// get the direction of the emission (the forward of the waterfall) compared to the direction of the gravity
		Vector3 gravityVector = World.GetGravityNormalizedVector(orientation);
		float dot = Vector3.Dot(gravityVector, transform.forward);
		
		// switch either to the against gravity emitter or to the stream emmitter depending on the gravity direction and waterfall direction
		if (dot < -0.5f)
		{
			// the gravity is in opposite direction of the waterfall direction
			m_CurrentEmitterPlaying = m_AgaintGravityWaterEmitter;
		}
		else
		{
			// switch between the stream emitter and the cloned one.
			m_CurrentEmitterPlaying = (m_CurrentEmitterPlaying == m_StreamWaterEmitter) ? m_DuplicatedStreamWaterEmitter : m_StreamWaterEmitter;

			// check is the emitter is aligned with gravity
			bool isEmitterAlongGravity = (dot > 0.5f);

			// adjust some configuration of the stream emitter
			AdjustStreamEmitterConfiguration(isEmitterAlongGravity);

			// also rotate the emitter (if not along with gravity)
			if (!isEmitterAlongGravity)
				RotateEmitter(gravityVector);
		}

		// start the new emitter
		StartCurrentEmitter(usePrewarm);
	}

	private void RotateEmitter(Vector3 gravityVector)
	{
		// we don't need to rotate the against gravity emitter
		Debug.Assert(m_CurrentEmitterPlaying != m_AgaintGravityWaterEmitter);

		// allign the up of the emitter with the gravity
		Transform emitterTransform = m_CurrentEmitterPlaying.transform;
		emitterTransform.rotation = Quaternion.FromToRotation(emitterTransform.up, -gravityVector) * emitterTransform.rotation;
		// if the emitter is inverted, rotate it half turn
		if (Vector3.Dot(emitterTransform.forward, transform.forward) < 0.5f)
			emitterTransform.rotation *= Quaternion.AngleAxis(180f, Vector3.up);
	}

	private void AdjustStreamEmitterConfiguration(bool isEmitterAlongGravity)
	{
		// if the gravity is right in the same direction of the waterfall, we need to disable the rotation over life time
		var rotationOverLifetime = m_CurrentEmitterPlaying.rotationOverLifetime;
		rotationOverLifetime.enabled = !isEmitterAlongGravity;

		// if the emitter is along gravity we need to rotate the particles
		float startRotationX = isEmitterAlongGravity ? 90f : 0f;
		var mainModule = m_CurrentEmitterPlaying.main;
		mainModule.startRotationX = new ParticleSystem.MinMaxCurve(startRotationX * Mathf.Deg2Rad);
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
