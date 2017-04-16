using System;
using UnityEngine;
using UnityEngine.Assertions;

public enum WaterOrientation
{
    RunningDown = 0,
    RunningUp = 1,
    RunninLeft = 2,
    RunningRight = 3,
    RunningFront = 4,
    RunningBack = 5
}

public class Waterfall : MonoBehaviour
{
    [HideInInspector] public TileOrientation WaterfallTileOrientation;
    [HideInInspector] public TileOrientation GravityAffectingWaterfall;
    public Animator WaterfallAnimator;

    readonly int[,] tileToWaterOrientationMap =
    {
        //DOWN, UP, LEFT, RIGHT, FRONT, BACK
        { 5, 4, 0, 1, 3, 3 }, // Down
        { 4, 5, 1, 0, 3, 3 }, // Up
        { 0, 1, 4, 5, 3, 3 }, // Left
        { 0, 1, 5, 4, 2, 2 }, // Right
        { 0, 1, 3, 2, 5, 5 }, // Front
        { 0, 1, 2, 3, 4, 4 } // Back
    }; // Emmiter oriented towards left by default

    readonly string[] gravityOrientationToStateName =
    {
        "Flowing Down",
        "Flowing Up",
        "Flowing Left",
        "Flowing Right",
        "Flowing Forward",
        "Flowing Backward"
    };

    public void Awake()
    {
        Assert.IsNotNull( WaterfallAnimator );
    }

    public void Start()
    {
        GravityAffectingWaterfall = Pawn.Instance.world.CurrentGravityOrientation;
        var orientation =
            tileToWaterOrientationMap[(int)( WaterfallTileOrientation - 1 ), (int)( GravityAffectingWaterfall - 1 )];
        WaterfallAnimator.Play( gravityOrientationToStateName[orientation] );
    }

    public void ChangeGravity( TileOrientation inverseGravityOrientation )
    {
        GravityAffectingWaterfall = inverseGravityOrientation;
        WaterfallAnimator.SetInteger( "WaterOrientation",
            ( tileToWaterOrientationMap[(int)( WaterfallTileOrientation - 1 ), (int)( inverseGravityOrientation - 1 )] ) );
    }
}