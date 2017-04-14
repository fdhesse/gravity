using UnityEditor;
using UnityEngine;

public enum WaterOrientation
{
    RunningDown = 0,
    RunningUp = 1,
    RunninLeft = 2,
    RunningRight = 3,
    RunningFront = 4,
    RunningBack = 5
}

public class GravityWaterfallTester : MonoBehaviour
{
    public WaterOrientation WaterFlow;
    public Animator WatefallAnimator;
    public void OnValidate()
    {
        WatefallAnimator.SetInteger( "WaterOrientation", ( int )WaterFlow );
        Debug.Log( "cenas" );
    }

    public AnimationClip clip;
}