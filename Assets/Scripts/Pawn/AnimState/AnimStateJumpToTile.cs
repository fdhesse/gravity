using UnityEngine;

public class AnimStateJumpToTile : AnimStateJumpFallBase
{
	private static readonly int END_JUMP_ANIM_ID = Animator.StringToHash("JumpAndLand");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
		Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");
		rootMotionController.enabled = true;

		// compute the up of the animation based on the current gravity
		Vector3 up = -World.GetGravityNormalizedVector(m_EndTile.orientation);

		// set a mach target to the edge of the current tile
		bool isMovingToTheEdge = (animatorStateInfo.shortNameHash != END_JUMP_ANIM_ID);
		Vector3 targetPosition = ComputeTargetPosition(isMovingToTheEdge, up);
		Quaternion targetOrientation = ComputeTargetOrientation(isMovingToTheEdge, up);
		rootMotionController.SetTargetPositionAndOrientation(targetPosition, targetOrientation, true, animatorStateInfo.shortNameHash);
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
}
