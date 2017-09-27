using UnityEngine;

public class AnimStateJump : StateMachineBehaviour
{
	private static readonly int END_JUMP_ANIM_ID = Animator.StringToHash("JumpAndLand");

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (animatorStateInfo.shortNameHash == END_JUMP_ANIM_ID)
		{
			Pawn pawn = animator.GetComponentInParent<Pawn>();
			Debug.Assert(pawn != null, "The pawn is null in the state exit of the Anim State Jump");
			pawn.OnJumpFinished();
		}
	}
}
