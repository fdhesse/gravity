using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RootMotionController : MonoBehaviour
{
	#region target position and rotation
	// my grand parent info
	private Transform m_MyGrandParent = null;	
	private Vector3 m_LastGrandParentPosition = Vector3.zero; // a variable to check if the grand parent position has moved

	// we store the target position in local space
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

	private Quaternion m_TargetOrientation = Quaternion.identity;
	public Quaternion TargetOrientation
	{
		// use SetTargetPositionAndDirection instead 
		private set	{ m_TargetOrientation = value; }
		get { return m_TargetOrientation; }
	}

	/// <summary>
	/// Sets the target position. You can also specified an expected horizontal and vertical distance, and this will
	/// scale the movement while reaching the target position.
	/// </summary>
	/// <param name="position">The position that the root should target.</param>
	/// <param name="orientation">The target orientation in world coordinate.</param>
	/// <param name="shouldInterruptMatchTarget">if <c>true</c> the current match target will be interrupted WITHOUT teleporting the character to the final position of the current match target.</param>
	/// <param name="stateOrTagHashForMatchTarget">The anim hash code for which you want to set the target position and direction. If zero, do it for the current anim.</param>
	/// <param name="isWorldPosition">if <c>true</c> the specified position is a world position, otherwise the position is local to parent of the parent of the this root movement controller.</param>
	public void SetTargetPositionAndOrientation(Vector3 position, Quaternion orientation, bool shouldInterruptMatchTarget = true, int stateOrTagHashForMatchTarget = 0, bool isWorldPosition = true)
	{
		// set the targets (use the accessors to set the other variables)
		this.TargetOrientation = orientation;
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
	#endregion

	#region target bone
	public enum TargetBone
	{
		ROOT = AvatarTarget.Root,
		BODY = AvatarTarget.Body
	}

	// The root reference is used during the update to know which motion should be apply to my parent transform
	private TargetBone m_TargetBone = TargetBone.ROOT;

	/// <summary>
	/// This method set the target bone that will be used during the match target with the specified value.
	/// If the wasTargetPositionSetForRoot flag is set, then the TargetPosition will be considered to be 
	/// set for the root, so if the specified targetBone equals BODY, then the target position will be
	/// recomputed during the OnAnimationIK using the body position that we get at that time and the TargetOrientation.
	/// Otherwise the target position will not be changed, and we consider that it was correctly set for the body position.
	/// </summary>
	/// <param name="targetBone">The bone that you want the match target to use to align with the TargetPosition and TargetOrientation.</param>
	/// <param name="wasTargetPositionSetForRoot">If <c>true</c> the RootMotionController will consider that the TargetPosition was set for the root, therefore will adjust it if you chosse to target the BODY bone.</param>
	public void SetTargetBone(TargetBone targetBone, bool wasTargetPositionSetForRoot = true)
	{
		// memorise the flag
		m_WasTargetPositionSetForRoot = wasTargetPositionSetForRoot;

		// check if we switch from BODY to ROOT or ROOT to BODY, and save one of the two flags accordingly
		// do not set the flags if nothing has changed.
		if (targetBone == TargetBone.BODY)
			m_DoesNeedToSaveTheLocalBodyPosition = (m_TargetBone == TargetBone.ROOT);
		else
			m_DoesNeedTodRelocateRootTransformFromBodyTransform = (m_TargetBone == TargetBone.BODY);

		// then save the new target bone
		m_TargetBone = targetBone;
	}

	// a flag that tells if we need to recompute the target position for body, in case it was set for root
	private bool m_WasTargetPositionSetForRoot = true;

	// this flag is used to save the m_LocalBodyPositionWhenTargetBoneWasSwitchToBody in the OnAnimatorIK function
	private bool m_DoesNeedToSaveTheLocalBodyPosition = false;

	// this flag is used when we had a match target from the Body bone, and we return to match target the root bone
	private bool m_DoesNeedTodRelocateRootTransformFromBodyTransform = false;

	/// <summary>
	/// The position of the body in local space of the root, when the TargetBoneValue was switch to
	/// the BODY mode. This value will then be reused to relocate the root position from the body position
	/// when the TargetBoneValue will be switched back to ROOT.
	/// </summary>
	private Vector3 m_LocalBodyPositionWhenTargetBoneWasSwitchToBody = Vector3.zero;
	#endregion

	#region move mode (rotation and translation)
	public enum RotationMode
	{
		USE_ANIM_ROTATION,
		ROTATE_TO_TARGET_ORIENTATION,
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
	#endregion

	#region internal data members
	// initial config set in awake, and should not be modified later
	private Vector3 m_InitialLocalPositionOfCharacterMesh = Vector3.zero;
	private Animator m_Animator = null;

	// internal data for the match target logic
	private bool m_IsMatchTargetSet = false; // a flag reset every time you change the target position
	private int m_LastMatchTargetCancelFrameNumber = 0;
	private int m_StateOrTagHashForMatchTarget = 0;
	private float m_MatchTargetNormalisedEndTime = 0f;
	MatchTargetWeightMask m_MatchTargetWeightMask;
	#endregion

	#region unity methods
	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
		m_InitialLocalPositionOfCharacterMesh = m_Animator.transform.localPosition;
	}

	private void Update()
	{
		UpdateMatchTarget();
		ApplyRootMovementAndResetLocalTransform();
	}

	private void OnAnimatorIK(int layerIndex)
	{
		// first check if we need to save the local body position during this frame
		if (m_DoesNeedToSaveTheLocalBodyPosition)
		{
			// clear the flag
			m_DoesNeedToSaveTheLocalBodyPosition = false;

			// the m_Animator.bodyPosition is given in world coordinate, translate it in local
			m_LocalBodyPositionWhenTargetBoneWasSwitchToBody = m_Animator.bodyPosition - transform.position;

			// check if we need to adjust the target position, after having computed the local body
			if (m_WasTargetPositionSetForRoot)
			{
				// recompute the target position
				TargetPosition += TargetOrientation * m_LocalBodyPositionWhenTargetBoneWasSwitchToBody;

				// change the flag since now the target position target the body
				m_WasTargetPositionSetForRoot = false;
			}
		}

		// then check if we need to relocate the root during this frame
		if (m_DoesNeedTodRelocateRootTransformFromBodyTransform)
		{
			m_DoesNeedTodRelocateRootTransformFromBodyTransform = false;

			// set my parent position from the body position
			transform.parent.position = m_Animator.bodyPosition - (m_Animator.bodyRotation * (m_LocalBodyPositionWhenTargetBoneWasSwitchToBody + m_InitialLocalPositionOfCharacterMesh));
			transform.localPosition = m_InitialLocalPositionOfCharacterMesh;

			// assign the body rotation to my parent, and clear my local rotation
			transform.parent.rotation = m_Animator.bodyRotation;
			transform.localRotation = Quaternion.identity;
		}
	}
	#endregion

	#region init/reset
	public void ResetAllParameters(bool startEnabled)
	{
		// reset all my data members
		m_MyGrandParent = null;
		m_LastGrandParentPosition = Vector3.zero;
		m_LocalTargetPosition = Vector3.zero;
		m_TargetOrientation = Quaternion.identity;
		m_RotationMode = RotationMode.USE_ANIM_ROTATION;
		m_TranslationMode = TranslationMode.USE_ANIM_TRANSLATION;
		m_IsMatchTargetSet = false;
		m_LastMatchTargetCancelFrameNumber = 0;
		m_StateOrTagHashForMatchTarget = 0;
		m_MatchTargetNormalisedEndTime = 0f;
		m_MatchTargetWeightMask = new MatchTargetWeightMask(Vector3.zero, 0f);
		m_TargetBone = TargetBone.ROOT;
		m_WasTargetPositionSetForRoot = true;
		m_DoesNeedToSaveTheLocalBodyPosition = false;
		m_DoesNeedTodRelocateRootTransformFromBodyTransform = false;
		m_LocalBodyPositionWhenTargetBoneWasSwitchToBody = Vector3.zero;

		// enable myself or not
		this.enabled = startEnabled;

        // also interrupt the match target if one is running
        // check if the animator was set, because it may happen that this reset is called before my Awake,
        // in such case we don't care to interrupt the match target if I'm not awake yet.
        if (m_Animator != null)
			InterruptMatchTarget();
    }

	private void InterruptMatchTarget()
	{
		m_Animator.InterruptMatchTarget(false);
		m_LastMatchTargetCancelFrameNumber = Time.frameCount + 1;
		m_IsMatchTargetSet = false;
	}
	#endregion

	#region update match target
	private void TryToSetMatchTarget(AnimatorStateInfo animStateInfo)
	{
		// check if we need to set a match target (in any of the to mode requires it)
		bool shouldMatchTranslation = (m_TranslationMode == TranslationMode.MOVE_TO_TARGET_POSITION);
		bool shouldMatchRotation = (m_RotationMode == RotationMode.ROTATE_TO_TARGET_ORIENTATION);
		bool shouldSetMatchTarget = shouldMatchTranslation || shouldMatchRotation;

		// toggle the match target set flag if we need to
		if (shouldSetMatchTarget && !m_IsMatchTargetSet && (Time.frameCount > m_LastMatchTargetCancelFrameNumber) &&
			((m_StateOrTagHashForMatchTarget == 0) || (animStateInfo.tagHash == m_StateOrTagHashForMatchTarget) || (animStateInfo.shortNameHash == m_StateOrTagHashForMatchTarget)))
		{
			// set the time a little further because if you set a start time for the MatchTarget which is before the the current time it will directly teleport to the target.
			float normalizeTime = Mathf.Repeat(animStateInfo.normalizedTime, 1f) + float.Epsilon;
			// compute the match target end flags for both modes
			float translationEndTime = GetMatchTargetNormalizedEndTime(normalizeTime, true);
			float rotationEndTime = GetMatchTargetNormalizedEndTime(normalizeTime, false);
			// now the end match target time is the bigger of the two and the mask of the smaller one is a linear progression between the two
			float translationWeightMask = 1f;
			float rotationWeightMask = 1f;
			if (shouldMatchTranslation)
			{
				if (shouldMatchRotation)
				{
					// both mode should match target, we should take the smallest time
					if (translationEndTime < rotationEndTime)
					{
						m_MatchTargetNormalisedEndTime = translationEndTime;
						rotationWeightMask = translationEndTime / rotationEndTime;
					}
					else
					{
						m_MatchTargetNormalisedEndTime = rotationEndTime;
						translationWeightMask = rotationEndTime / translationEndTime;
					}
				}
				else
				{
					// only match target the translation
					m_MatchTargetNormalisedEndTime = translationEndTime;
					rotationWeightMask = 0f;
				}
			}
			else
			{
				// only match target the rotation
				m_MatchTargetNormalisedEndTime = rotationEndTime;
				translationWeightMask = 0f;
			}
				
			if (normalizeTime < m_MatchTargetNormalisedEndTime)
			{
				m_MatchTargetWeightMask = new MatchTargetWeightMask(new Vector3(translationWeightMask, translationWeightMask, translationWeightMask), rotationWeightMask);
				// set the match target
				SetMatchTarget(normalizeTime);
				// memorise the position of the grand parent to check if I'm moving
				m_LastGrandParentPosition = GetGrandParentWorldPosition();
			}
		}
	}

	private void SetMatchTarget(float normalizedTime)
	{
		m_Animator.MatchTarget(TargetPosition, m_TargetOrientation, (AvatarTarget)m_TargetBone, m_MatchTargetWeightMask, normalizedTime, m_MatchTargetNormalisedEndTime);
		m_IsMatchTargetSet = true;
	}

	private void UpdateMatchTarget()
	{
		// get the current anim state info
		var animStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

		// check if we reach the end of the Match target, we should clear the m_IsMatchTargetSet flag, 
		// to potentially set a new match target (and the clear of that flag is done in the interrupt match target function)
		if ((m_IsMatchTargetSet) && (animStateInfo.normalizedTime > m_MatchTargetNormalisedEndTime))
			InterruptMatchTarget();

		// if the animator is in transition cancel and reset the match target
		if (m_Animator.IsInTransition(0))
			InterruptMatchTarget();
		else
			TryToSetMatchTarget(animStateInfo);

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
				SetMatchTarget(normalizeTime);
		}
	}

	private float GetMatchTargetNormalizedEndTime(float currentNormalizedTime, bool isForTranslation)
	{
		// try to find if there's an event to change the root rotation mode in the current clip
		// if yes, try to match the target at the time the event if fired.
		var clipInfo = m_Animator.GetCurrentAnimatorClipInfo(0);
		if (clipInfo.Length > 0)
		{
			string eventName = isForTranslation ? "OnRootTranslationMode" : "OnRootRotationMode";
			var clip = clipInfo[0].clip;
			var eventList = clip.events;
			foreach (var animEvent in eventList)
			{
				// normalize the time of the event, because the time in the event class is in second
				float normalizedEventTime = animEvent.time / clip.length;
				if ((normalizedEventTime > currentNormalizedTime) && animEvent.functionName.Equals(eventName))
					return normalizedEventTime;
			}
		}

		// by default return the end of the anim (normalized)
		return 1f;
	}
	#endregion

	#region apply animation movement to parent
	private void ApplyRootMovementAndResetLocalTransform()
	{
		// apply the root motion due to the animation, to the transform of my parent, and reset my local transform
		transform.localPosition -= m_InitialLocalPositionOfCharacterMesh;
		transform.parent.position = transform.position;
		transform.localPosition = m_InitialLocalPositionOfCharacterMesh;

		// apply the rotation
		transform.parent.rotation = transform.rotation;
		transform.localRotation = Quaternion.identity;
	}
	#endregion

	#region grand parent
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
	#endregion

	#region anim event
	private void OnRootTranslationMode(int mode)
	{
		// if we just set a match target rotation value that was not set before,
		// we need to interrupt the match target for the update to recreate one
		if ((m_TranslationMode == TranslationMode.USE_ANIM_TRANSLATION) &&
			(mode == (int)TranslationMode.MOVE_TO_TARGET_POSITION))
			InterruptMatchTarget();

		// set the new mode
		TranslationModeValue = (TranslationMode)mode;
	}

	private void OnRootRotationMode(int mode)
	{
		// if we just set a match target rotation value that was not set before,
		// we need to interrupt the match target for the update to recreate one
		if ((m_RotationMode == RotationMode.USE_ANIM_ROTATION) &&
			(mode == (int)RotationMode.ROTATE_TO_TARGET_ORIENTATION))
			InterruptMatchTarget();

		// set the new mode
		RotationModeValue = (RotationMode)mode;
	}
	#endregion

	#region draw debug
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Color activeColor = new Color(1.0f, 0.5f, 0.5f);
		Color inactiveColor = new Color(0.8f, 0.8f, 0.6f);
		Vector3 targetPos = TargetPosition;

		// draw the target position
		Gizmos.color = (m_TranslationMode == TranslationMode.MOVE_TO_TARGET_POSITION) ? activeColor : inactiveColor;
        Gizmos.DrawSphere(targetPos, 1f);

		// draw the target direction
		UnityEditor.Handles.color = (m_RotationMode == RotationMode.ROTATE_TO_TARGET_ORIENTATION) ? activeColor : inactiveColor;
		UnityEditor.Handles.ArrowHandleCap(0, targetPos, TargetOrientation, GameplayCube.HALF_CUBE_SIZE, EventType.Repaint);
		// draw the target up
		Gizmos.color = (m_RotationMode == RotationMode.ROTATE_TO_TARGET_ORIENTATION) ? activeColor : inactiveColor;
		Vector3 up = TargetOrientation * Vector3.up;
		up *= GameplayCube.HALF_CUBE_SIZE;
		Gizmos.DrawRay(targetPos, up);

		// draw the remaining time
		if (m_Animator != null)
		{
			var animStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
			int completionRatio = 100;
			if (animStateInfo.normalizedTime < m_MatchTargetNormalisedEndTime)
				completionRatio = (int)((animStateInfo.normalizedTime * 100f) / m_MatchTargetNormalisedEndTime);
			UnityEditor.Handles.Label(targetPos, completionRatio.ToString());

			//UnityEditor.Handles.color = Color.cyan;
			//UnityEditor.Handles.ArrowHandleCap(1, m_Animator.bodyPosition, m_Animator.bodyRotation, GameplayCube.HALF_CUBE_SIZE, EventType.Repaint);

			UnityEditor.Handles.color = Color.black;
			UnityEditor.Handles.ArrowHandleCap(1, m_Animator.targetPosition, m_Animator.targetRotation, GameplayCube.HALF_CUBE_SIZE, EventType.Repaint);
		}
	}
	#endif
	#endregion
}
