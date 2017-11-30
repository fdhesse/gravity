using UnityEngine;

public class AnimStateIdle : StateMachineBehaviour
{
	private static readonly int IDLE_ANIM_ID = Animator.StringToHash("Idle Id");
	private static readonly int IDLE_TAG = Animator.StringToHash("Idle");

	// list all the Ids used in the animator with the IDLE_ANIM_ID variable
	private enum IdleId
	{
		IDLE = 0,
		IDLE02 = 1,
		WAIT = 10,
	}

	[SerializeField]
	[Tooltip("The probability to play idle anim 0 versus idle anim 2.")]
	[Range(0f, 1f)]
	private float m_IdleAnimProbability = 0.65f;

	[SerializeField]
	[Tooltip("The minimum and maximum time in second, that the idle anim should be played before the Wait anim is triggered.")]
	private Vector2 m_WaitAnimTimeRange = new Vector2(8f, 16f);

	// the time when we should choose for a new idle anim
	private float m_NextIdleAnimChooseTime = 0;

	// the time when the wait anim will be played
	private float m_WaitAnimStartTime = 0;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		// set the wait anim time, some time randomly in the future if not set.
		// It would have been easier to place this into the OnStateMachineEnter() function, but there's a
		// unity bug preventing this function to be called when the "entry" green box is linked to the sub state machine
		if (m_WaitAnimStartTime == 0)
			ChooseNextWaitAnimStartTime();

		// compute the next anim choose time if we are playing a standard idle anim
		ChooseNextIdleAnim(animator, animatorStateInfo);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		// check if it's time to play the wait anim
		if (Time.time > m_WaitAnimStartTime)
		{
			// change the wait anim start time to avoid double trigger of the wait anim
			ChooseNextWaitAnimStartTime();
			// and trigger the anim
			animator.SetInteger(IDLE_ANIM_ID, (int)IdleId.WAIT);
			// move the next idle anim time further, to leave some time to the transition
			m_NextIdleAnimChooseTime = Time.time + 10f;
		}
		else if (Time.time > m_NextIdleAnimChooseTime)
		{
			// and set the next idle anim for later
			ChooseNextIdleAnim(animator, animatorStateInfo);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		// reset the idle anim id, if I leave the wait state
		if (animator.GetInteger(IDLE_ANIM_ID) == (int)IdleId.WAIT)
			animator.SetInteger(IDLE_ANIM_ID, (int)IdleId.IDLE);

		// reset the wait timer, if we exit toward a non idle tagged anim,
		// this will reset the timer, next time we re-enter the idle state
		if (animator.GetCurrentAnimatorStateInfo(layerIndex).tagHash != IDLE_TAG)
			m_WaitAnimStartTime = 0;
	}

	private void ChooseNextWaitAnimStartTime()
	{
		// set the wait anim time, some time randomly in the future
		m_WaitAnimStartTime = Time.time + Random.Range(m_WaitAnimTimeRange.x, m_WaitAnimTimeRange.y);
	}

	private void ChooseNextIdleAnim(Animator animator, AnimatorStateInfo animatorStateInfo)
	{
		// draw a new random idle anim
		float draw = Random.value;
		animator.SetInteger(IDLE_ANIM_ID, (draw < m_IdleAnimProbability) ? (int)IdleId.IDLE : (int)IdleId.IDLE02);
		// set the next anim choose time
		m_NextIdleAnimChooseTime = Time.time + animatorStateInfo.length;
	}
}
