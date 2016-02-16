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

	[Tooltip("The quantity of movement, so the distance in game unit for a TRANSLATION step, or angle in degree for a ROTATION step.")]
	public float moveValue = 10f;

	[Tooltip("The translation speed in game unit/second for a TRANSLATION step, or in degree/second for a ROTATION step.")]
	public float speed = 10f;

	[Tooltip("The acceleration used at the begining of the step which is also the decelleration used at the end of the step, in game unit/second² for a TRANSLATION step, or in degree/second² for a ROTATION step.")]
	public float acceleration = 10f;

	[Tooltip("The time in second, the platform will wait without moving at the end of the step.")]
	public float pauseTime = 2f;
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


	void Start ()
	{
	
	}
	
	void Update ()
	{
	
	}
}
