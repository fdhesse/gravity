using UnityEngine;

public class AnimStateRollToTile : StateMachineBehaviour
{
	private static readonly int ANIM_ROTATE_ROOT_STATE = Animator.StringToHash("RotateRootState");

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

			// set a mach target to the edge of the current tile
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
			pawn.OnRollOrAbseilFinished();
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

		// forwards depends on the animation chosen
		Vector3 startTileUp = -World.GetGravityNormalizedVector(m_StartTile.orientation);
		Vector3 forward = Vector3.forward;
		Pawn.BorderDirection borderDir = (Pawn.BorderDirection)animator.GetInteger(Pawn.ANIM_BORDER_DIRECTION_INT);
		switch (borderDir)
		{
			case Pawn.BorderDirection.FRONT:
				forward = startTileUp;
				break;
			case Pawn.BorderDirection.BACK:
				forward = -startTileUp;
				break;
			case Pawn.BorderDirection.RIGHT:
			case Pawn.BorderDirection.LEFT:
				{
					// make the cross product betwen the up of the first tile and the distance between the two tiles
					Vector3 diff = m_EndTile.transform.position - m_StartTile.transform.position;
					forward = Vector3.Cross(startTileUp, diff);
					if (borderDir == Pawn.BorderDirection.RIGHT)
						forward = -forward;
					break;
				}
		}

		// now compute the action quaternion based on this two vectors
		return Quaternion.LookRotation(forward, up);
	}
}
