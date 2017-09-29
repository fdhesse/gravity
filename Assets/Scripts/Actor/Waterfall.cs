using UnityEngine;

public class Waterfall : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The particle system that generate the water particles")]
	private ParticleSystem m_WaterEmitter = null;

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
	}

	public void Reset(TileOrientation startingOrientation)
	{
		// for now simply change the orientation
		ChangeGravity(startingOrientation);
	}

	public void ChangeGravity(TileOrientation orientation)
	{
		var sizeOverLifetime = m_WaterEmitter.sizeOverLifetime;

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
