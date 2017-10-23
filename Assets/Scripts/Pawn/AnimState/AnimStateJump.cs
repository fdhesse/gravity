using UnityEngine;

/// <summary>
/// This class is a base class for the two jump state classes, aka AnimStateJumpToTile and AnimStateJumpAbseil
/// </summary>
public abstract class AnimStateJump : StateMachineBehaviour
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
			result = m_StartTile.transform.position + m_EndTile.transform.position;
			result *= 0.5f;
			// move up to half a cube in the direction of the up, and add it to the result
			up *= GameplayCube.HALF_CUBE_SIZE;
			result += up;
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
