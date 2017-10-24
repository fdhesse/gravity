using UnityEngine;

public class AnimStateJumpAbseil : AnimStateJump
{
	private static readonly int ABSEIL_DOWN_TAG = Animator.StringToHash("AbseilDown");
	private static readonly int ANIM_ABSEIL_FROM_BORDER_TRIGGER = Animator.StringToHash("Abseil from Border");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		RootMotionController rootMotionController = animator.GetComponent<RootMotionController>();
		Debug.Assert(rootMotionController != null, "No root motion controller on the Pawn.");
		rootMotionController.enabled = true;

		// also start to play the axe animation
		if (animatorStateInfo.tagHash == ABSEIL_DOWN_TAG)
		{
			Pawn pawn = animator.GetComponentInParent<Pawn>();
			Debug.Assert(pawn != null, "The pawn is null in the state start of the Anim State Abseil Down");
			Transform pickaxe = pawn.PickaxeTransform;
			Debug.Assert(pickaxe != null, "The pawn doesn't have pickaxe in start Anim State Abseil Down");
			// dettach the pickaxe from the pawn and set its position to the previous world target which was the edge
			pickaxe.parent = pawn.transform.parent;
			pickaxe.position = rootMotionController.TargetPosition;
			pickaxe.rotation = rootMotionController.TargetOrientation;
			// set the correct trigger in the pickaxe animator to trigger the correct anim
			Animator axeAnimator = pickaxe.GetComponent<Animator>();
			axeAnimator.SetInteger(Pawn.ANIM_ABSEIL_HEIGHT_INT, animator.GetInteger(Pawn.ANIM_ABSEIL_HEIGHT_INT));
			axeAnimator.SetTrigger(ANIM_ABSEIL_FROM_BORDER_TRIGGER);
		}

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
			// reattach the pickaxe to the pawn
			Transform pickaxe = pawn.PickaxeTransform;
			Debug.Assert(pickaxe != null, "The pawn doesn't have pickaxe in exit Anim State Abseil Down");
			pickaxe.parent = pawn.transform;
			pickaxe.localPosition = Vector3.zero;
			pickaxe.localRotation = Quaternion.identity;
			// warn the pawn that the jump is finished
			pawn.OnJumpFinished();
		}
	}
}
