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
		// switch emitter to choose the correct one at init time
		SwitchEmitter(orientation, m_InitEmitterDuringNextGravityChange);

		// reset the flag after calling the switch emitter
		m_InitEmitterDuringNextGravityChange = false;
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

			// if the gravity is right in the same direction of the waterfall, we need to disable the rotation over life time
			var rotationOverLifetime = m_CurrentEmitterPlaying.rotationOverLifetime;
			rotationOverLifetime.enabled = !isEmitterAlongGravity;

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
	}

	private void SetStartRotation(ParticleSystem.MainModule mainModule, float x, float y, float z)
	{
		mainModule.startRotationX = new ParticleSystem.MinMaxCurve(x * Mathf.Deg2Rad);
		mainModule.startRotationY = new ParticleSystem.MinMaxCurve(y * Mathf.Deg2Rad);
		mainModule.startRotationZ = new ParticleSystem.MinMaxCurve(z * Mathf.Deg2Rad);
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
