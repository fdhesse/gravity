using System.Collections.Generic;
using UnityEngine;

public class AnimatedMotionFrame
{
    public int Frame;
    public Vector3 Translation;
    public Vector3 Rotation;
}


public class FramedAnimatedMotion : AnimatedMotion
{
    public List<AnimatedMotionFrame> AnimatedMotionFrames;
}