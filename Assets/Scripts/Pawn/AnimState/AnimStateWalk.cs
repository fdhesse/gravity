using UnityEngine;

public class AnimStateWalk : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		Pawn pawn = animator.GetComponentInParent<Pawn>();
		Debug.Assert(pawn != null, "The pawn is null in the state exit of the Anim State Walk");
		pawn.OnWalkToTileFinished();
	}
}
