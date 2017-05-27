using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class FrameProgression
{
    public float Progress;
    public MotionFrameData MotionFrameData;

    public FrameProgression( float progress, MotionFrameData motionFrameData )
    {
        Progress = progress;
        MotionFrameData = motionFrameData;
    }
}

[CreateAssetMenu]
public class ClimbDownFramedAnimatedMotion : FramedAnimatedMotion
{
    List<FrameProgression> frameToMotionFramesData;

    public void Move( Pawn pawn, Vector3 direction )
    {
        frameToMotionFramesData = new List<FrameProgression>();
        var gravityOrientation = pawn.GetWorldVerticality();
        var gravityDirection = World.getGravityVector( gravityOrientation );

        const float tolerance = 0.001f;
        var planarDirection = new Vector3(
            Math.Abs( gravityDirection.x ) < tolerance ? direction.x : 0,
            Math.Abs( gravityDirection.y ) < tolerance ? direction.y : 0,
            Math.Abs( gravityDirection.z ) < tolerance ? direction.z : 0 );

        var normalizedPlanarDirection = planarDirection.normalized; // Tipically something like Vector(0,1f,0)

        ClimbDown( pawn, normalizedPlanarDirection );
    }

    public static bool AlmostEqual( Vector3 v1, Vector3 v2, float precision )
    {
        var equal = !( Mathf.Abs( v1.x - v2.x ) > precision );
        if ( Mathf.Abs( v1.y - v2.y ) > precision )
            equal = false;
        if ( Mathf.Abs( v1.z - v2.z ) > precision )
            equal = false;

        return equal;
    }

    void ClimbDown( Pawn pawn, Vector3 targetTileDirection )
    {
        var movementRotationAngle = GetPawnRotationOnTheVerticalAxisForTargetTileDirection( targetTileDirection );
        Assert.IsNotNull( pawn.pawnTile );
        var positionBeforeMovement = pawn.pawnTile.transform.position;
        frameToMotionFramesData = new List<FrameProgression>();

        for ( var i = 0; i < MotionFramesData.Count; i++ )
        {
            var rotation = MotionFramesData[i].Rotation + movementRotationAngle;
            var translation = RotatePointAroundPivot( 
                positionBeforeMovement + MotionFramesData[i].Translation,
                pawn.transform.position,
                movementRotationAngle );
            var motionFrameSection = ( MotionFramesData[i].Frame - 1 ) / ( (float)TotalFrames - 1 ); // %
            var motionFrame = new MotionFrameData( MotionFramesData[i].Frame, translation, rotation );
            var elem = new FrameProgression( motionFrameSection, motionFrame );
            frameToMotionFramesData.Add( elem );
        }
        pawn.IsMotionOverridingMovement = true;
        pawn.GetComponent<Rigidbody>().useGravity = false;
        pawn.GetComponent<Rigidbody>().isKinematic = true;

        pawn.isClimbingDown = true;
        pawn.isFalling = true;

        pawn.animState = 4;
        pawn.Animator.SetTrigger( "Transitioning" );

        // reset the pawn tile when starting to climb down, because if you climb down from
        // a moving platform, you don't want to climb down relative to the plateform
        pawn.onEnterTile( null );
        pawn.StartCoroutine( MoveEveryFrame( pawn.gameObject, () =>
        {
            frameToMotionFramesData = new List<FrameProgression>();
            pawn.IsMotionOverridingMovement = false;
            pawn.GetComponent<Rigidbody>().useGravity = true;
            pawn.GetComponent<Rigidbody>().isKinematic = false;
            pawn.isClimbingDown = false;
            pawn.clickedTile = null; // target reached, forget it
        } ) );
    }

    IEnumerator MoveEveryFrame( GameObject moveable, Action atTheEndDo )
    {
        var progress = 0f;
        var elapsedTime = 0f;
        var frameIndex = 1;

        while ( progress < 1f )
        {
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / MovementDuration;

            if ( progress > frameToMotionFramesData[frameIndex].Progress )
            {
                frameIndex++;
            }
            else
            {
                var sourceFrameData =frameToMotionFramesData[frameIndex - 1].MotionFrameData;
                var destinationFrameData = frameToMotionFramesData[frameIndex].MotionFrameData;

                var lowerBoundFrameDataProgress = frameToMotionFramesData[frameIndex - 1].Progress;
                var upperBoundFrameDataProgress = frameToMotionFramesData[frameIndex].Progress;

                MoveLinearlyAccordingToFrameData(
                    moveable,
                    progress,
                    lowerBoundFrameDataProgress,
                    upperBoundFrameDataProgress,
                    sourceFrameData,
                    destinationFrameData );

                yield return new WaitForEndOfFrame();
            }
        }
        atTheEndDo();
    }

    static void MoveLinearlyAccordingToFrameData( GameObject moveable, float progress,
        float lowerProgressRange, float upperProgressRange,
        MotionFrameData sourceFrameData, MotionFrameData destinationFrameData )
    {
        var progressInThisFrameRange = progress - lowerProgressRange;
        var differenceBetweenProgressRanges = upperProgressRange - lowerProgressRange;

        Debug.Assert( differenceBetweenProgressRanges > 0 );
        if ( differenceBetweenProgressRanges <= 0 )
        {
            Debug.LogError( string.Format( "lower:{0} upper:{1}", lowerProgressRange, upperProgressRange ) );
        }
        var t = progressInThisFrameRange / differenceBetweenProgressRanges;
        moveable.transform.position = Vector3.Lerp( sourceFrameData.Translation, destinationFrameData.Translation, t ) +
                                      Vector3.up * Pawn.TileHeight;
        moveable.transform.rotation = Quaternion.Lerp( Quaternion.Euler( sourceFrameData.Rotation ),
            Quaternion.Euler( destinationFrameData.Rotation ), t );

        Debug.DrawLine( sourceFrameData.Translation, destinationFrameData.Translation, Color.green, 1f );
    }

    static Vector3 GetPawnRotationOnTheVerticalAxisForTargetTileDirection( Vector3 targetTileDirection )
    {
        const float tolerance = 0.001f;
        if ( AlmostEqual( targetTileDirection, Vector3.forward, tolerance ) )
        {
            return new Vector3( 0, 0, 0 );
        }
        if ( AlmostEqual( targetTileDirection, Vector3.right, tolerance ) )
        {
            return new Vector3( 0, 90, 0 );
        }
        if ( AlmostEqual( targetTileDirection, -Vector3.forward, tolerance ) )
        {
            return new Vector3( 0, 180, 0 );
        }
        if ( AlmostEqual( targetTileDirection, -Vector3.right, tolerance ) )
        {
            return new Vector3( 0, 270, 0 );
        }
        Debug.LogError( "No rotation found for target tile direction: " + targetTileDirection );
        return Vector3.zero;
    }

    public Vector3 RotatePointAroundPivot( Vector3 point, Vector3 pivot, Vector3 angles )
    {
        return Quaternion.Euler( angles ) * ( point - pivot ) + pivot;
    }
}