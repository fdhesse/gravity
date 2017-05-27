using System.Collections.Generic;
using FullInspector;
using UnityEngine;

public class MotionFrameData
{
    public int Frame;
    public Vector3 Translation;
    public Vector3 Rotation;

    public MotionFrameData(Vector3 translation, Vector3 rotation)
    {
        Translation = translation;
        Rotation = rotation;
    }
}

public class FramedAnimatedMotion : AnimatedMotion
{
    public float MovementDuration;
    public int TotalFrames;
    public List<MotionFrameData> MotionFramesData;

    [InspectorButton] public void AssignTargetAnimationDuration()
    {
        MovementDuration = TargetAnimation.length;
    }
}