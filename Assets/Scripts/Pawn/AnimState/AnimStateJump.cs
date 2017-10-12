using UnityEngine;

public class AnimStateJump : StateMachineBehaviour
{
	private static readonly int END_JUMP_ANIM_ID = Animator.StringToHash("JumpAndLand");

	// the start and end tile
	private Tile m_StartTile = null;
	private Tile m_EndTile = null;

	public void SetStartAndEndTile(Tile start, Tile end)
	{
		m_StartTile = start;
		m_EndTile = end;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
		Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");
		rootMotionController.enabled = true;

		// compute the up of the animation based on the current gravity
		Vector3 up = -World.GetGravityNormalizedVector(World.Instance.CurrentGravityOrientation);

		// set a mach target to the edge of the current tile
		bool isMovingToTheEdge = (animatorStateInfo.shortNameHash != END_JUMP_ANIM_ID);
		Vector3 targetPosition = ComputeTargetPosition(isMovingToTheEdge, up);
		Quaternion targetOrientation = ComputeTargetOrientation(isMovingToTheEdge, up);
		rootMotionController.SetTargetPositionAndDirection(targetPosition, targetOrientation, true, animatorStateInfo.shortNameHash);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (animatorStateInfo.shortNameHash == END_JUMP_ANIM_ID)
		{
			// disable the root motion controller when we have finished
			RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
			Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");
			rootMotionController.enabled = false;

			// then warn the pawn that the animation is finished
			Pawn pawn = animator.GetComponentInParent<Pawn>();
			Debug.Assert(pawn != null, "The pawn is null in the state exit of the Anim State Jump");
			pawn.OnJumpFinished();
		}
	}
	
	private Vector3 ComputeTargetPosition(bool isMovingToTheEdge, Vector3 up)
	{
		Vector3 result = Vector3.zero;

		// the target computation is different depending if I go to the edge or the center of the tile
		if (isMovingToTheEdge)
		{
			result = m_StartTile.transform.position + m_EndTile.transform.position;
			result *= 0.5f;
			// move up to half a cube in the direction of the up, and add it to the result
			up *= 5f;
			result += up;
		}
		else
		{
			result = m_EndTile.transform.position;
		}

		return result;
	}

	private Quaternion ComputeTargetOrientation(bool isMovingToTheEdge, Vector3 up)
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
