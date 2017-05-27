using System.Collections.Generic;
using FullInspector;
using UnityEngine;

public class MotionFrameData
{
    public int Frame;
    public Vector3 Translation;
    public Vector3 Rotation;

    public MotionFrameData(int frame, Vector3 translation, Vector3 rotation)
    {
        Frame = frame;
        Translation = translation;
        Rotation = rotation;
    }
}

public class FramedAnimatedMotion : AnimatedMotion
{
    public float MovementDuration;
    public int TotalFrames;

    public List<MotionFrameData> MotionFramesData { get; private set; }

    [InspectorButton] public void AssignTargetAnimationDuration()
    {
        MovementDuration = TargetAnimation.length;
    }
}