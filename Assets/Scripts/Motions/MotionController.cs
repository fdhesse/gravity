using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[CreateAssetMenu]
public class MotionController : BaseScriptableObject {
    public AnimatedMotion[] AnimatedMotions = new AnimatedMotion[0];

    readonly Dictionary<int, FramedAnimationMotionType> distanceToRappelType = new Dictionary<int, FramedAnimationMotionType>()
    {
        {20, FramedAnimationMotionType.RappelDown20 },
        {30, FramedAnimationMotionType.RappelDown30 },
        {40, FramedAnimationMotionType.RappelDown40 },
        {50, FramedAnimationMotionType.RappelDown50 },
        {60, FramedAnimationMotionType.RappelDown60 },
        {70, FramedAnimationMotionType.RappelDown70 }
    };

    public FramedAnimationMotionType GetRappelingMotion( int i )
    {
        return distanceToRappelType[i];
    }
}