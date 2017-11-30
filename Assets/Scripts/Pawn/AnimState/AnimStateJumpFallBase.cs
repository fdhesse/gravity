using UnityEngine;

/// <summary>
/// This class is a base class for the two jump state classes, aka AnimStateJumpToTile and AnimStateJumpAbseil
/// </summary>
public abstract class AnimStateJumpFallBase : StateMachineBehaviour
{
	// the start and end tile
	protected Tile m_StartTile = null;
	protected Tile m_EndTile = null;

	public void SetStartAndEndTile(Tile start, Tile end)
	{
		m_StartTile = start;
		m_EndTile = end;
	}

	protected Vector3 ComputeTargetPosition(bool isMovingToTheEdge, Vector3 up)
	{
		Vector3 result = Vector3.zero;

		// the target computation is different depending if I go to the edge or the center of the tile
		if (isMovingToTheEdge)
		{
			Vector3 diff = m_EndTile.transform.position - m_StartTile.transform.position;
			Vector3 right = Vector3.Cross(up, diff);
			Vector3 forward = Vector3.Cross(right, up);
			forward.Normalize();
			forward *= GameplayCube.HALF_CUBE_SIZE;
			result = m_StartTile.transform.position + forward;
		}
		else
		{
			result = m_EndTile.transform.position;
		}

		return result;
	}

	protected Quaternion ComputeTargetOrientation(bool isMovingToTheEdge, Vector3 up)
	{
		Vector3 diff = m_StartTile.transform.position - m_EndTile.transform.position;
		// cancel the diff along the up axis
		if (up.x != 0f)
			diff.x = 0f;
		else if (up.y != 0f)
			diff.y = 0f;
		else
			diff.z = 0f;
		// now compute the action quaternion based on this two vectors
		if (isMovingToTheEdge)
			return Quaternion.LookRotation(diff, up);
		else
			return Quaternion.LookRotation(-diff, up);
	}
}
