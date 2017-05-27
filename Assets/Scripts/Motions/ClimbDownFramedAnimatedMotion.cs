using System;
using System.Collections;
using System.Collections.Generic;
using Eppy;
using UnityEngine;

[CreateAssetMenu]
public class ClimbDownFramedAnimatedMotion : FramedAnimatedMotion
{
    List<Tuple<float, MotionFrameData>> frameToMotionFramesData;

    Tile LastTile;

    public void Move( Pawn pawn, Vector3 direction )
    {
        var orientation = pawn.GetWorldVerticality();
        var gravity = World.getGravityVector( orientation );

        var planarDirection = new Vector3( gravity.x == 0f ? direction.x : 0, gravity.y == 0f ? direction.y : 0,
            gravity.z == 0f ? direction.z : 0 );
        var normalizedPlanarDirection = planarDirection.normalized;
        //Debug.LogError( string.Format( "Gravity is {0}, direction is {1}, nplanarDir{2}", gravity, direction, normalizedPlanarDirection ) );

        if ( AlmostEqual( normalizedPlanarDirection, Vector3.forward, 0.01f ) )
        {
            ClimbDown( pawn );
        }
        else
        {
            throw new NotImplementedException();
            // Debug.LogError( string.Format( "{0} is not {1}", normalizedPlanarDirection, Vector3.forward ) );
        }
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

    void ClimbDown( Pawn pawn )
    {
        frameToMotionFramesData = new List<Tuple<float, MotionFrameData>>();

        for ( var i = 0; i < MotionFramesData.Count; i++ )
        {
            var motionFrame = MotionFramesData[i];
            var motionFrameSection = ( motionFrame.Frame - 1 ) / ( (float)TotalFrames - 1 ); // %
            Debug.LogError( motionFrameSection );
            var elem = new Tuple<float, MotionFrameData>( motionFrameSection, motionFrame );
            frameToMotionFramesData.Add( elem );
        }
        pawn.IsMotionOverridingMovement = true;
        pawn.GetComponent<Rigidbody>().useGravity = false;
        pawn.GetComponent<Rigidbody>().isKinematic = true;

        pawn.isClimbingDown = true;
        pawn.isFalling = true;

        pawn.animState = 4;
        pawn.Animator.SetTrigger( "Transitioning" );

        LastTile = pawn.pawnTile;
        Debug.LogError( "LAstTile", LastTile );
        // reset the pawn tile when starting to climb down, because if you climb down from
        // a moving platform, you don't want to climb down relative to the plateform
        pawn.onEnterTile( null );
        pawn.StartCoroutine( MoveEveryFrame( pawn ) );

        return;
        pawn.StartCoroutine( SectionedMovement.MoveDown( pawn, 0, MovementDuration, () =>
        {
            frameToMotionFramesData = new List<Tuple<float, MotionFrameData>>();
            pawn.isClimbingDown = false;
            pawn.clickedTile = null; // target reached, forget it
            // the modification in orientation
            //pawn.LookAtPosition( pawn.transform.position + direction );
        } ) );

        // look the other way
        // pawn.LookAtPosition( pawn.transform.position - direction );

        // pawn.LookAtPosition( tilePosition );
    }

    IEnumerator MoveEveryFrame( Pawn pawn )
    {
        var progress = 0f;
        var elapsedTime = 0f;
        var frameIndex = 0;
        var firstFrameData = new MotionFrameData( Vector3.zero, Vector3.zero );

        while ( progress < 1f )
        {
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / MovementDuration;

            if ( progress > frameToMotionFramesData[frameIndex].Item1 )
            {
                frameIndex++;
            }
            else
            {
                var sourceFrameData = frameIndex == 0 ?
                    firstFrameData : frameToMotionFramesData[frameIndex - 1].Item2;
                var destinationFrameData = frameToMotionFramesData[frameIndex].Item2;

                var lowerBoundFrameDataProgress = frameIndex == 0
                    ? 0f : frameToMotionFramesData[frameIndex - 1].Item1;
                var upperBoundFrameDataProgress = frameToMotionFramesData[frameIndex].Item1;

                MoveLinearlyAccordingToFrameData(
                    pawn,
                    progress,
                    lowerBoundFrameDataProgress,
                    upperBoundFrameDataProgress,
                    sourceFrameData,
                    destinationFrameData );

                yield return new WaitForEndOfFrame();
            }
        }
    }

    void MoveLinearlyAccordingToFrameData( Pawn pawn, float progress, float lowerProgressRange, float upperProgressRange,
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
        pawn.transform.position = LastTile.transform.position +
                                  Vector3.Lerp( sourceFrameData.Translation, destinationFrameData.Translation, t ) + Vector3.up* Pawn.TileHeight;
        pawn.transform.rotation = Quaternion.Lerp( Quaternion.Euler( sourceFrameData.Rotation ),
            Quaternion.Euler( destinationFrameData.Rotation ), t );

        Debug.DrawLine( LastTile.transform.position + sourceFrameData.Translation,
            LastTile.transform.position + destinationFrameData.Translation, Color.green, 1f );
    }
}