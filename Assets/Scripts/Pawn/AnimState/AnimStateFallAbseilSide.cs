using UnityEngine;

public class AnimStateFallAbseilSide : AnimStateJump
{
	private static readonly int ABSEIL_DOWN_TAG = Animator.StringToHash("AbseilDown");
	private static readonly int ANIM_ROTATE_ROOT_STATE = Animator.StringToHash("RotateRootState");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (animatorStateInfo.shortNameHash == ANIM_ROTATE_ROOT_STATE)
		{
			// if we enter in the rotate root state, just exit immediateley, as we just need one frame
			// for the root motion controller to rotate the root.
			animator.SetTrigger(Pawn.ANIM_EXIT_STATE_TRIGGER);
		}
		else
		{
			RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
			Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");
			rootMotionController.enabled = true;

			// compute the up of the animation based on the current gravity
			Vector3 up = -World.GetGravityNormalizedVector(m_EndTile.orientation);

			// set a mach target to the center of the start or end tile, depending in which type of anim we are
			Vector3 targetPosition = m_StartTile.transform.position;
			if (animatorStateInfo.tagHash == ABSEIL_DOWN_TAG)
				targetPosition = m_EndTile.transform.position;
			Quaternion targetOrientation = ComputeTargetOrientation(true, up);
			rootMotionController.SetTargetPositionAndOrientation(targetPosition, targetOrientation, true, animatorStateInfo.shortNameHash);
			rootMotionController.SetTargetBone(RootMotionController.TargetBone.BODY);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
		Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");

		if (animatorStateInfo.shortNameHash == ANIM_ROTATE_ROOT_STATE)
		{
			// disable the root motion controller when we have finished
			rootMotionController.enabled = false;

			// then warn the pawn that the animation is finished
			Pawn pawn = animator.GetComponentInParent<Pawn>();
			Debug.Assert(pawn != null, "The pawn is null in the state exit of the Anim State Fall Abseil Side");
			// warn the pawn that the abseil is finished
			pawn.OnRollOrFallAbseilFinished();
		}
		else if (animatorStateInfo.tagHash == ABSEIL_DOWN_TAG)
		{
			// reset the target bone to root, the default one and set the target position on the ground again
			rootMotionController.SetTargetPositionAndOrientation(m_EndTile.transform.position, rootMotionController.TargetOrientation);
			rootMotionController.SetTargetBone(RootMotionController.TargetBone.ROOT);
			// but anyway, disable the match target
			rootMotionController.SetMoveModes(RootMotionController.TranslationMode.USE_ANIM_TRANSLATION, RootMotionController.RotationMode.USE_ANIM_ROTATION);
		}
	}
}
