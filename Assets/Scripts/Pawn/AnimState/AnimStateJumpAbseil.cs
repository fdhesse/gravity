using UnityEngine;

public class AnimStateJumpAbseil : AnimStateJump
{
	private static readonly int ABSEIL_DOWN_TAG = Animator.StringToHash("AbseilDown");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
		Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");
		rootMotionController.enabled = true;

		// compute the up of the animation based on the current gravity
		Vector3 up = -World.GetGravityNormalizedVector(m_EndTile.orientation);

		// set a mach target to the edge of the current tile
		bool isMovingToTheEdge = (animatorStateInfo.tagHash != ABSEIL_DOWN_TAG);
		Vector3 targetPosition = ComputeTargetPosition(isMovingToTheEdge, up);
		Quaternion targetOrientation = ComputeTargetOrientation(isMovingToTheEdge, up);
		rootMotionController.SetTargetPositionAndOrientation(targetPosition, targetOrientation, true, animatorStateInfo.shortNameHash);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (animatorStateInfo.tagHash == ABSEIL_DOWN_TAG)
		{
			// disable the root motion controller when we have finished
			RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
			Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");
			rootMotionController.enabled = false;

			// then warn the pawn that the animation is finished
			Pawn pawn = animator.GetComponentInParent<Pawn>();
			Debug.Assert(pawn != null, "The pawn is null in the state exit of the Anim State Abseil Down");
			pawn.OnJumpFinished();
		}
	}
}
