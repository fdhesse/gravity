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
		m_TargetOrientation = ComputeTargetOrientation(animator);
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

	private Quaternion ComputeTargetOrientation(Animator animator)
	{
		// up is like the destination tile
		Vector3 up = -World.GetGravityNormalizedVector(World.Instance.CurrentGravityOrientation);

		// get the forward of the pawn, and snap it to the world axis
		Vector3 forward = animator.transform.forward;
		forward = new Vector3(Mathf.Round(forward.x), Mathf.Round(forward.y), Mathf.Round(forward.z));

		// if the forward is collinear with the gravity, take something else
		float dot = Vector3.Dot(up, forward);
		if ((dot > 0.9f) || (dot < -0.9f))
		{
			Vector3 animatorUp = animator.transform.up;
			if (dot > 0f)
				forward = new Vector3(Mathf.Round(-animatorUp.x), Mathf.Round(-animatorUp.y), Mathf.Round(-animatorUp.z));
			else
				forward = new Vector3(Mathf.Round(animatorUp.x), Mathf.Round(animatorUp.y), Mathf.Round(animatorUp.z));
		}

		// now compute the action quaternion based on this two vectors
		return Quaternion.LookRotation(forward, up);
	}
}
