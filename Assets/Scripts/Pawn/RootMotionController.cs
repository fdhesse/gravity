using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RootMotionController : MonoBehaviour
{
	[Tooltip("Actor rotation speed in degrees per second.")]
	[SerializeField]
	private float m_TurnSpeed = 90.0f;

	// init config
	private Vector3 m_InitialLocalPositionOfCharacterMesh = Vector3.zero;

	private Transform m_MyGrandParent = null;
	private Vector3 m_LocalTargetPosition = Vector3.zero;
	public Vector3 LocalTargetPosition
	{
		// use SetTargetPositionAndDirection instead 
		private set
		{
			m_LocalTargetPosition = value;
			m_IsMatchTargetSet = false;
		}
		get { return m_LocalTargetPosition; }
	}

	public Vector3 TargetPosition
	{
		// use SetTargetPositionAndDirection instead 
		private set
		{
			m_MyGrandParent = this.transform.parent.parent;
			if (m_MyGrandParent != null)
				m_LocalTargetPosition = m_MyGrandParent.InverseTransformPoint(value);
			else
				m_LocalTargetPosition = value;
			m_IsMatchTargetSet = false;
		}
		get
		{
			if (m_MyGrandParent != null)
			{
				return m_MyGrandParent.TransformPoint(m_LocalTargetPosition);
			}
			else
			{
				var grandParent = this.transform.parent.parent;
				if (grandParent != null)
					return grandParent.TransformPoint(m_LocalTargetPosition);
				else
					return m_LocalTargetPosition;
			}
		}
	}

	private Quaternion m_ActionOrientation = Quaternion.identity;
	public Quaternion ActionOrientation
	{
		// use SetTargetPositionAndDirection instead 
		private set	{ m_ActionOrientation = value; }
		get { return m_ActionOrientation; }
	}

	// a flag reset every time you change the target position
	private bool m_IsMatchTargetSet = false;
	private int m_LastMatchTargetCancelFrameNumber = 0;
	private int m_StateOrTagHashForMatchTarget = 0;
	private float m_MatchTargetNormalisedEndTime = 0f;

	// a variable to check if the grand parent position has moved
	private Vector3 m_LastGrandParentPosition = Vector3.zero;

	/// <summary>
	/// Sets the target position. You can also specified an expected horizontal and vertical distance, and this will
	/// scale the movement while reaching the target position.
	/// </summary>
	/// <param name="position">The position that the root should target.</param>
	/// <param name="orientation">The target orientation in world coordinate.</param>
	/// <param name="shouldInterruptMatchTarget">if <c>true</c> the current match target will be interrupted WITHOUT teleporting the character to the final position of the current match target.</param>
	/// <param name="stateOrTagHashForMatchTarget">The anim hash code for which you want to set the target position and direction. If zero, do it for the current anim.</param>
	/// <param name="isWorldPosition">if <c>true</c> the specified position is a world position, otherwise the position is local to parent of the parent of the this root movement controller.</param>
	public void SetTargetPositionAndDirection(Vector3 position, Quaternion orientation, bool shouldInterruptMatchTarget = true, int stateOrTagHashForMatchTarget = 0, bool isWorldPosition = true)
	{
		// set the targets (use the accessors to set the other variables)
		this.ActionOrientation = orientation;
		if (isWorldPosition)
			this.TargetPosition = position;
		else
			this.LocalTargetPosition = position;

		// interrupt the match target without teleporting the character
		if (shouldInterruptMatchTarget)
			InterruptMatchTarget();

		// memorise also the anim for which the target is set
		m_StateOrTagHashForMatchTarget = stateOrTagHashForMatchTarget;
	}

	public enum RotationMode
	{
		USE_ANIM_ROTATION,
		ROTATE_TOWARD_ACTION_DIRECTION,
	}

	private RotationMode m_RotationMode = RotationMode.USE_ANIM_ROTATION;
	public RotationMode RotationModeValue
	{
		set { m_RotationMode = value; }
	}

	public enum TranslationMode
	{
		USE_ANIM_TRANSLATION,
		MOVE_TO_TARGET_POSITION,
	}

	private TranslationMode m_TranslationMode = TranslationMode.USE_ANIM_TRANSLATION;
	public TranslationMode TranslationModeValue
	{
		set { m_TranslationMode = value; }
	}

	public void SetMoveModes(TranslationMode translationMode, RotationMode rotationMode)
	{
		m_TranslationMode = translationMode;
		m_RotationMode = rotationMode;
	}

	private Animator m_Animator = null;

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
		m_InitialLocalPositionOfCharacterMesh = m_Animator.transform.localPosition;
	}

	private void Update()
	{
		ApplyRootMovementAndResetLocalPosition();
		ApplyRootRotationAndResetLocalRotation();
	}

	public void ResetAllParameters()
	{
		m_MyGrandParent = null;
		m_LocalTargetPosition = Vector3.zero;
		m_ActionOrientation = Quaternion.identity;
		m_IsMatchTargetSet = false;
		m_StateOrTagHashForMatchTarget = 0;
		m_MatchTargetNormalisedEndTime = 0f;
		m_LastGrandParentPosition = Vector3.zero;
		m_RotationMode = RotationMode.USE_ANIM_ROTATION;
		m_TranslationMode = TranslationMode.USE_ANIM_TRANSLATION;
        
        // also interrupt the match target if one is running
        // check if the animator was set, because it may happen that this reset is called before my Awake,
        // in such case we don't care to interrupt the match target if I'm not awake yet.
        if (m_Animator != null)
			InterruptMatchTarget();
    }

	private void ApplyRootMovementAndResetLocalPosition()
	{
		// get the current anim state info
		var animStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

		// if the animator is in transition cancel and reset the match target
		if (m_Animator.IsInTransition(0))
		{
			InterruptMatchTarget();
		}
		else
		{
			// toggle the match target set flag
			if ((m_TranslationMode == TranslationMode.MOVE_TO_TARGET_POSITION) && !m_IsMatchTargetSet && (Time.frameCount > m_LastMatchTargetCancelFrameNumber) &&
				((m_StateOrTagHashForMatchTarget == 0) || (animStateInfo.tagHash == m_StateOrTagHashForMatchTarget) || (animStateInfo.shortNameHash == m_StateOrTagHashForMatchTarget)))
			{
				// set the time a little further because if you set a start time for the MatchTarget which is before the the current time it will directly teleport to the target.
				float normalizeTime = Mathf.Repeat(animStateInfo.normalizedTime, 1f) + float.Epsilon;
				m_MatchTargetNormalisedEndTime = GetMatchTargetNormalizedEndTime(normalizeTime);
				if (normalizeTime < m_MatchTargetNormalisedEndTime)
				{
					// reset the match target every frame because the world TargetPosition may change if the player is on a plateform
					m_Animator.MatchTarget(TargetPosition, m_ActionOrientation, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, 0f), normalizeTime, m_MatchTargetNormalisedEndTime);
					m_IsMatchTargetSet = true;
					// memorise the position of the grand parent to check if I'm moving
					m_LastGrandParentPosition = GetGrandParentWorldPosition();
				}
			}
		}

		// update the MatchTarget position if the match target is set, in case I'm attached to a platform, floating block, etc...
		if ((m_IsMatchTargetSet) && (m_LastGrandParentPosition != GetGrandParentWorldPosition()))
		{
			// get the new grand parent position
			m_LastGrandParentPosition = GetGrandParentWorldPosition();

			// cancel the match target because anyway we will reset it, and we cannot set a match target if there's one already running
			InterruptMatchTarget();

			// set the time a little further because if you set a start time for the MatchTarget which is before the the current time it will directly teleport to the target.
			float normalizeTime = Mathf.Repeat(animStateInfo.normalizedTime, 1f) + float.Epsilon;
			if (normalizeTime < m_MatchTargetNormalisedEndTime)
			{
				m_Animator.MatchTarget(TargetPosition, m_ActionOrientation, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, 0f), normalizeTime, m_MatchTargetNormalisedEndTime);
				m_IsMatchTargetSet = true;
			}
		}

		// apply the root motion due to the animation, to the transform of my parent, and reset my local transform
		transform.localPosition -= m_InitialLocalPositionOfCharacterMesh;
		transform.parent.position = transform.position;
		transform.localPosition = m_InitialLocalPositionOfCharacterMesh;
	}

	private void InterruptMatchTarget()
	{
		m_Animator.InterruptMatchTarget(false);
		m_LastMatchTargetCancelFrameNumber = Time.frameCount + 1;
		m_IsMatchTargetSet = false;
	}

	private float GetMatchTargetNormalizedEndTime(float currentNormalizedTime)
	{
		// try to find if there's an event to change the root rotation mode in the current clip
		// if yes, try to match the target at the time the event if fired.
		var clipInfo = m_Animator.GetCurrentAnimatorClipInfo(0);
		if (clipInfo.Length > 0)
		{
			var clip = clipInfo[0].clip;
			var eventList = clip.events;
			foreach (var animEvent in eventList)
			{
				// normalize the time of the event, because the time in the event class is in second
				float normalizedEventTime = animEvent.time / clip.length;
				if ((normalizedEventTime > currentNormalizedTime) && animEvent.functionName.Equals("OnRootTranslationMode"))
					return normalizedEventTime;
			}
		}

		// by default return the end of the anim (normalized)
		return 1f;
	}

	private void ApplyRootRotationAndResetLocalRotation()
	{
		switch (m_RotationMode)
		{
		case RotationMode.USE_ANIM_ROTATION:
			transform.parent.rotation = transform.rotation;
			transform.localRotation = Quaternion.identity;
			break;
		case RotationMode.ROTATE_TOWARD_ACTION_DIRECTION:
			transform.parent.rotation = Quaternion.RotateTowards(transform.parent.rotation, m_ActionOrientation, m_TurnSpeed * Time.deltaTime);
			transform.localRotation = Quaternion.identity;
			break;
		}
	}

	/// <summary>
	/// return the world position of my grand parent, if I have any set, or zero otherwise.
	/// This function is usefull to check if my grand parent has moved
	/// (for example if I'm on a platform that has moved).
	/// </summary>
	/// <returns>the world position of my grand parent, if I have any set, or zero otherwise.</returns>
	private Vector3 GetGrandParentWorldPosition()
	{
		var grandParent = this.transform.parent.parent;
		if (grandParent != null)
			return grandParent.position;
		return Vector3.zero;
	}

	/// <summary>
	/// This function should be called when the grand parent of thie RootMotionController (so generally the Pawn)
	/// as changed its parent. The new grand parent can be null, in that case this function should also be called.
	/// </summary>
	public void NotifyGrandParentAttachementChange()
	{
		// if my parent was attached to a new object, we need to recompute the local target
		// first get my new grand parent
		Transform newGrandParent = this.transform.parent.parent;
		// check if my grand parent has changed, if nothing changed, does nothing
		if (newGrandParent != m_MyGrandParent)
		{
			// re-compute the local target position, by going back to world with old grand parent (if any)
			if (m_MyGrandParent != null)
				m_LocalTargetPosition = m_MyGrandParent.TransformPoint(m_LocalTargetPosition);
			// and going back to local with the new grandparent (if we have a valid one)
			if (newGrandParent != null)
				m_LocalTargetPosition = newGrandParent.InverseTransformPoint(m_LocalTargetPosition);
			// memorize my new grand parent
			m_MyGrandParent = newGrandParent;
		}
	}

	#region draw debug
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
        if (transform.parent.CompareTag("Player"))
    		Gizmos.color = new Color(1.0f, 0.75f, 0.6f);
        else
    		Gizmos.color = new Color(0.6f, 0.75f, 1.0f);
        
        Gizmos.DrawSphere(TargetPosition, 1f);
    }
	#endif
	#endregion
}
