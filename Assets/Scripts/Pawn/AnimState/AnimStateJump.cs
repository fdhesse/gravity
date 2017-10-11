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

		// set a mach target to the edge of the current tile
		bool isMovingToTheEdge = (animatorStateInfo.shortNameHash != END_JUMP_ANIM_ID);
		Vector3 targetPosition = ComputeTargetPosition(isMovingToTheEdge);
		Quaternion targetOrientation = ComputeTargetOrientation(isMovingToTheEdge);
		rootMotionController.SetTargetPositionAndDirection(targetPosition, targetOrientation, true, animatorStateInfo.shortNameHash);
		rootMotionController.SetMoveModes(RootMotionController.TranslationMode.MOVE_TO_TARGET_POSITION, RootMotionController.RotationMode.USE_ANIM_ROTATION);
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
	
	private Vector3 ComputeTargetPosition(bool isMovingToTheEdge)
	{
		Vector3 gravityDirection = World.GetGravityNormalizedVector(World.Instance.CurrentGravityOrientation);
		gravityDirection *= 5f;

		Vector3 result = Vector3.zero;

		if (isMovingToTheEdge)
		{
			result = m_StartTile.transform.position + m_EndTile.transform.position;
			result *= 0.5f;
			result -= gravityDirection;
		}
		else
		{
			result = m_EndTile.transform.position;
		}

		return result;
	}

	private Quaternion ComputeTargetOrientation(bool isMovingToTheEdge)
	{
		// TODO implement correctly this method: this only works with gravity along Y
		Vector3 diff = m_StartTile.transform.position - m_EndTile.transform.position;
		if (isMovingToTheEdge)
			return Quaternion.LookRotation(diff);
		else
			return Quaternion.LookRotation(-diff);
	}
}
