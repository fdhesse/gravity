using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MovingStep
{
	public enum MoveType
	{
		TRANSLATION = 0,
		ROTATION,
	}

	public enum Axis
	{
		X = 0,
		Y,
		Z,
	}

	[Tooltip("The type of movement for this step.")]
	public MoveType stepType = MoveType.TRANSLATION;

	[Tooltip("The translation axis for a TRANSLATION step, or the rotation axis for a ROTATION step.")]
	public Axis axis = Axis.X;

	[Tooltip("The quantity of movement, so the distance in game unit for a TRANSLATION step, or angle in degree for a ROTATION step. You can use a negative value to move in the reverse direction as the world axis, or turn in the opposite direction.")]
	public float moveValue = 10f;

	[Tooltip("The translation speed in game unit/second for a TRANSLATION step, or in degree/second for a ROTATION step.")]
	[Range(0f, 100f)]
	public float speed = 10f;

	[Tooltip("The acceleration used at the begining of the step which is also the decelleration used at the end of the step, in game unit/second² for a TRANSLATION step, or in degree/second² for a ROTATION step.")]
	[Range(0f, 100f)]
	public float acceleration = 10f;

	[Tooltip("The time in second, the platform will wait without moving at the end of the step.")]
	[Range(0f, 600f)]
	public float pauseTime = 2f;

	[HideInInspector]
	public float moveDirection = 1f; // basically the sign of moveValue

	[HideInInspector] 
	public Vector3 stepPosition;

	[HideInInspector] 
	public Quaternion stepOrientation;

	public void init(Vector3 previousPosition, Quaternion previousOrientation)
	{
		// compute the move direction
		moveDirection = (moveValue > 0f) ? 1f : -1f;

		// init the 
		stepPosition = previousPosition;
		stepOrientation = previousOrientation;

		switch (stepType)
		{
		case MovingStep.MoveType.TRANSLATION:
			switch(axis)
			{
			case Axis.X:
				stepPosition.x += moveValue;
				break;
			case Axis.Y:
				stepPosition.y += moveValue;
				break;
			case Axis.Z:
				stepPosition.z += moveValue;
				break;
			}
			break;
			
		case MovingStep.MoveType.ROTATION:
			switch(axis)
			{
			case Axis.X:
				stepOrientation *= Quaternion.AngleAxis(moveValue, Vector3.right);
				break;
			case Axis.Y:
				stepOrientation *= Quaternion.AngleAxis(moveValue, Vector3.up);
				break;
			case Axis.Z:
				stepOrientation *= Quaternion.AngleAxis(moveValue, Vector3.forward);
				break;
			}
			break;
		}
	}
}

public class MovingPlatform : MonoBehaviour
{
	public enum PlayingMode
	{
		ONE_GO = 0,
		PING_PONG,
		LOOP,
	}

	[Tooltip("The way the animation will play.\n ONE_GO: the animation will play only one time, then the platform won't move anymore.\nPING_PONG: the animation will play in reverse at the end, to return back at the beginning.\nLOOP: when the animation finish, the animation plays again from the begining. This is a usefull mode if you want a circular motion.")]
	public PlayingMode playingMode = PlayingMode.PING_PONG;

	[Tooltip("All the steps of the animation. You can specify parameters for each steps. Steps are played in the order of the list.")]
	public List<MovingStep> steps;

	private enum StepPhase
	{
		ACCELERATE = 0,
		CONSTANT_MOVE,
		DECCELERATE,
		WAIT,
	}

	private bool isInit = false;
	private Vector3 mStartingPosition;
	private Quaternion mStartingOrientation;
	private int mCurrentStep = 0;
	private StepPhase mCurrentStepPhase = StepPhase.ACCELERATE;
	private float mCurrentSpeed = 0f;
	private float mCurrentTime = 0f;
	private float[] mPhaseTime = new float[(int)StepPhase.WAIT + 1];
	private int mPingPongDirection = 1;

	void Awake()
	{
		if (!isInit)
		{
			isInit = true;

			mStartingPosition = transform.position;
			mStartingOrientation = transform.rotation;

			// init the internal values of the steps
			Vector3 previousPosition = transform.position;
			Quaternion previousRotation = transform.rotation;
			foreach (MovingStep step in steps)
			{
				step.init(previousPosition, previousRotation);
				previousPosition = step.stepPosition;
				previousRotation = step.stepOrientation;
			}
		}
	}

	public void Reset(TileOrientation startingOrientation)
	{
		if (!isInit)
			Awake();

		// reset the transform
		transform.position = mStartingPosition;
		transform.rotation = mStartingOrientation;
		// reset the step and speed
		mCurrentStep = 0;
		mCurrentStepPhase = StepPhase.ACCELERATE;
		mCurrentSpeed = 0f;
		mCurrentTime = 0f;
		mPingPongDirection = 1;
		// recompute the phase time of the first step
		computePhaseTimeForStep(steps[0]);
	}

	void Update()
	{
		// check that the current step is valid. It can be invalid in ONE_GO mode,
		// when the animation reach the end
		if (mCurrentStep < steps.Count)
		{
			// get the current step
			MovingStep currentStep = steps[mCurrentStep];

			// adjust the speed inside the step
			adjustSpeed(currentStep);

			// and move the platform
			move(currentStep);
		}
	}

	private void adjustSpeed(MovingStep currentStep)
	{
		// increase the current time
		mCurrentTime += Time.deltaTime;
		// get the current end phase time
		float endPhaseTime = mPhaseTime[(int)mCurrentStepPhase];

		// adjust speed according to step
		switch (mCurrentStepPhase)
		{
		case StepPhase.ACCELERATE:
			// accelerate
			mCurrentSpeed += currentStep.acceleration * Time.deltaTime;
			// check if we reach the maximum speed
			if ((mCurrentTime > endPhaseTime) || (mCurrentSpeed > currentStep.speed))
			{
				mCurrentSpeed = currentStep.speed;
				mCurrentStepPhase = StepPhase.CONSTANT_MOVE;
			}
			break;

		case StepPhase.CONSTANT_MOVE:
			// check if we reach the time to decelerate
			if (mCurrentTime > endPhaseTime)
				mCurrentStepPhase = StepPhase.DECCELERATE;
			break;

		case StepPhase.DECCELERATE:
			// deccelerate
			mCurrentSpeed -= currentStep.acceleration * Time.deltaTime;
			// check if we reach the maximum distance
			if ((mCurrentTime > endPhaseTime) || (mCurrentSpeed < 0f))
			{
				mCurrentSpeed = 0f;
				mCurrentStepPhase = StepPhase.WAIT;
			}
			break;

		case StepPhase.WAIT:
			// check if we wait enough time
			if (mCurrentTime > endPhaseTime)
			{
				// reset the phase for the next step
				mCurrentStepPhase = StepPhase.ACCELERATE;
				mCurrentTime = 0f;
				// increase the step
				goToNextStep();
				// compute the new phase time for the new current step
				if (mCurrentStep < steps.Count)
					computePhaseTimeForStep(steps[mCurrentStep]);
			}
			break;
		}
	}

	private void goToNextStep()
	{
		switch (playingMode)
		{
		case PlayingMode.ONE_GO:
			// increase the step
			// in the one go mode, once we reach the end we don't move anymore
			mCurrentStep++;
			break;
		case PlayingMode.PING_PONG:
			// if there's only one step, the current step never change and stay 0
			if (steps.Count > 1)
			{
				// check if we reach the end of one direction, and then reverse the ping pong direction
				// without changing the step, cause we need to play the same step again but in reverse direction
				if ((mPingPongDirection == 1) && (mCurrentStep == steps.Count - 1))
					mPingPongDirection = -1;
				else if ((mPingPongDirection == -1) && (mCurrentStep == 0))
					mPingPongDirection = 1;
				else // go to the next step in the current ping pong direction
					mCurrentStep += mPingPongDirection;
				break;
			}
			else
			{
				mPingPongDirection = -mPingPongDirection;
			}
			break;
		case PlayingMode.LOOP:
			mCurrentStep = (mCurrentStep + 1) % steps.Count;
			break;
		}
	}

	private void computePhaseTimeForStep(MovingStep currentStep)
	{
		// get the total distance moved during the step, by getting the absolute value of the move value
		float totalMoveDistance = Mathf.Abs(currentStep.moveValue);

		// float the time for acceleration/decceleration, knowing the speed and acceleration is simply:
		// a = v/t  so t = v/a
		float accelTime = currentStep.speed / currentStep.acceleration;

		// now we also need to compute the distance moved during that acceleration time
		// To compute decelleration knowing initial speed and distance is: a = v²/2d
		// so d = v²/2a
		float distanceDuringAccel = (currentStep.speed * currentStep.speed) / (2f * currentStep.acceleration);

		// so now we can compute the distance at constant speed, which is the total distance minus the
		// acceleration and decelleration distance (which are the same):
		float distanceDuringConstantSpeed = totalMoveDistance - (2f * distanceDuringAccel);

		// if the distance is negative, that means the acceleration was not strong enough to reach the
		// desired speed on the total distance chosen. Or it can also means the speed was too high.
		// We could also increase the distance, but on the design point of view, it's certainly not the
		// choice that the level designer would do. So we can choose to increase acceleration or decrease
		// speed, without touching the total distance. The level designer would certainly choose to increase
		// the acceleration, as it is a cosmetic things to have the platform accelerating and deccelerating
		// at the beginning/end of the step. In fact the speed if what the designer really wants
		// to tune, so lets not touch the speed, and lets increase the acceleration.
		if (distanceDuringConstantSpeed < 0f)
		{
			// lets redo the computation in reverse
			distanceDuringConstantSpeed = 0f;
			distanceDuringAccel = totalMoveDistance * 0.5f;
			currentStep.acceleration = (currentStep.speed * currentStep.speed) / (2f * distanceDuringAccel);
			accelTime = currentStep.speed / currentStep.acceleration;
		}

		// so now we can compute the time during the constant speed travel
		// v = d/t  so  t = d/v
		float constantTime = distanceDuringConstantSpeed / currentStep.speed;

		// So now we can set the four phase times
		mPhaseTime[(int)StepPhase.ACCELERATE] = accelTime;
		mPhaseTime[(int)StepPhase.CONSTANT_MOVE] = accelTime + constantTime;
		mPhaseTime[(int)StepPhase.DECCELERATE] = mPhaseTime[(int)StepPhase.CONSTANT_MOVE] + accelTime;
		mPhaseTime[(int)StepPhase.WAIT] = mPhaseTime[(int)StepPhase.DECCELERATE] + currentStep.pauseTime;
	}

	private void move(MovingStep currentStep)
	{
		if (mCurrentStepPhase == StepPhase.WAIT)
		{
//			transform.position = currentStep.stepPosition;
//			transform.rotation = currentStep.stepOrientation;
		}
		else
		{
			float deltaMove = mCurrentSpeed * Time.deltaTime * currentStep.moveDirection * mPingPongDirection;

			switch (currentStep.stepType)
			{
			case MovingStep.MoveType.TRANSLATION:
				switch(currentStep.axis)
				{
				case MovingStep.Axis.X:
					transform.position = new Vector3(transform.position.x + deltaMove, transform.position.y, transform.position.z);
					break;
				case MovingStep.Axis.Y:
					transform.position = new Vector3(transform.position.x, transform.position.y + deltaMove, transform.position.z);
					break;
				case MovingStep.Axis.Z:
					transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + deltaMove);
					break;
				}
				break;
				
			case MovingStep.MoveType.ROTATION:
//				switch(currentStep.axis)
//				{
//				case MovingStep.Axis.X:
//					transform.rotation *= Quaternion.AngleAxis(currentStep.moveValue, Vector3.right);
//					break;
//				case MovingStep.Axis.Y:
//					transform.rotation *= Quaternion.AngleAxis(currentStep.moveValue, Vector3.up);
//					break;
//				case MovingStep.Axis.Z:
//					transform.rotation *= Quaternion.AngleAxis(currentStep.moveValue, Vector3.forward);
//					break;
//				}
				break;
			}
		}
	}
}
