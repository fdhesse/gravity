using UnityEngine;

public class AnimStateFreeFall : StateMachineBehaviour
{
	[SerializeField]
	[Tooltip("The duration of the rotation, for the Pawn to rotate his body toward the gravity (in second).")]
	private float m_RotationDuration = 1f;

	// the starting time of the rotation
	private float m_StartRotationTime = 0f;

	// the target orientation of the free fall
	private Quaternion m_TargetOrientation = Quaternion.identity;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		// memorise the time of the begining of the free fall
		m_StartRotationTime = Time.time;

		// compute the target orientation

	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		// compute the rotation percentage
		float t = (m_RotationDuration > 0f) ? (Time.time - m_StartRotationTime) / m_RotationDuration : 1f;
		if (t < 1f)
			animator.transform.parent.rotation = Quaternion.Lerp(animator.transform.parent.rotation, m_TargetOrientation, t);
		else
			animator.transform.parent.rotation = m_TargetOrientation;
	}

	//public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	//{
	//}
}
