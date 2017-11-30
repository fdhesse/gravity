using UnityEngine;

public class AnimStateFallAbseilAbove : AnimStateJumpFallBase
{
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
			// otherwise for a normal state, set the target position and rotation for the body
			RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
			Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");
			rootMotionController.enabled = true;

			// set a match target to the center of the destination tile
			Quaternion targetOrientation = ComputeTargetOrientation(animator);
			rootMotionController.SetTargetPositionAndOrientation(m_EndTile.transform.position, targetOrientation, true, animatorStateInfo.shortNameHash);
			rootMotionController.SetTargetBone(RootMotionController.TargetBone.BODY);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		// disable the root motion controller when we have finished
		RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
		Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");

		if (animatorStateInfo.shortNameHash == ANIM_ROTATE_ROOT_STATE)
		{
			// when we exit the rotate root state, we can disable the root motion controller, as it has finished to rotate it.
			rootMotionController.enabled = false;
			// then warn the pawn that the animation is finished
			Pawn pawn = animator.GetComponentInParent<Pawn>();
			Debug.Assert(pawn != null, "The pawn is null in the state exit of the Anim State Jump");
			pawn.OnRollOrFallAbseilFinished();
		}
		else
		{
			// reset the target bone to root, the default one and set the target position on the ground again
			rootMotionController.SetTargetPositionAndOrientation(m_EndTile.transform.position, rootMotionController.TargetOrientation);
			rootMotionController.SetTargetBone(RootMotionController.TargetBone.ROOT);
			// but anyway, disable the match target
			rootMotionController.SetMoveModes(RootMotionController.TranslationMode.USE_ANIM_TRANSLATION, RootMotionController.RotationMode.USE_ANIM_ROTATION);
		}
	}

	private Quaternion ComputeTargetOrientation(Animator animator)
	{
		// up is like the destination tile
		Vector3 up = -World.GetGravityNormalizedVector(m_EndTile.orientation);

		// get the forward of the pawn, and snap it to the world axis, and reverse it
		Vector3 forward = animator.transform.forward;
		forward = new Vector3(Mathf.Round(-forward.x), Mathf.Round(-forward.y), Mathf.Round(-forward.z));

		// now compute the action quaternion based on this two vectors
		return Quaternion.LookRotation(forward, up);
	}
}
