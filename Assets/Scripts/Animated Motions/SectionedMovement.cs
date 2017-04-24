using System;
using System.Collections;
using UnityEngine;

public static class SectionedMovement {
    public static IEnumerator MoveDown( Pawn pawn, float delayBeforeMovement, float movementDuration, Action post )
    {
        var targetTilePosition = pawn.focusedTile.transform.position;

        pawn.GetComponent<Rigidbody>().useGravity = false;
        pawn.GetComponent<Rigidbody>().isKinematic = true;

        var origin = pawn.gameObject.transform.position;
        var destination = targetTilePosition + Vector3.up * ( pawn.height / 2 - Pawn.TileHeight );
        var elapsedTime = 0.0f;

        // These four points compose
        // The path that takes the pawn down
        // To the tile bellow

        var p0 = origin;  // Center of origin tile
        DebugUtils.DrawPoint( p0, Color.red );

        var p3 = destination; // center of destination tile
        DebugUtils.DrawPoint( p3, Color.yellow );

        var p0Flat = new Vector3( p0.x, 0, p0.z );
        var p3Flat = new Vector3( p3.x, 0, p3.z );
        var p1 = p0 + ( p3Flat - p0Flat ) / 2 + ( p3Flat - p0Flat ).normalized * pawn.width;  // Edge of origin tile
        DebugUtils.DrawPoint( p1, Color.green );

        var p2 = p3 - ( p3Flat - p0Flat ) / 2 + ( p3Flat - p0Flat ).normalized * pawn.width;  // Edge of destination tile
        DebugUtils.DrawPoint( p2, Color.blue );

        // p0->p1 : first subpath
        // p1->p2 : second subpath
        // p2->p3 : third subpath
        var firstPathDistance = ( p1 - p0 ).magnitude;
        var secondPathDistance = ( p2 - p1 ).magnitude;
        var thirdPathDistance = ( p3 - p2 ).magnitude;

        var pathDistance = firstPathDistance + secondPathDistance + thirdPathDistance;

        while ( elapsedTime < delayBeforeMovement )
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0;

        while ( elapsedTime < movementDuration )
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / movementDuration;

            // BezierSingleMotionClimbdownUpdate( origin, destination, t );

            SectionedMotionDownUpdate( pawn, p0, p1, p2, p3, pathDistance, firstPathDistance, secondPathDistance,
                thirdPathDistance, t );

            yield return null;
        }

        pawn.gameObject.transform.position = destination;

        if ( post != null )
        {
            post();
        }

        pawn.gameObject.GetComponent<Rigidbody>().useGravity = true;
        pawn.gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    // TODO: If we want a smooth motion, we need to apply easing to float t
    public static void SectionedMotionDownUpdate( Pawn pawn, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float pathDistance,
        float firstPathDistance, float secondPathDistance, float thirdPathDistance, float progress )
    {
        if ( progress < firstPathDistance / pathDistance )
        {
            // Move on first path
            var firstPathSectionProgress = progress / ( firstPathDistance / pathDistance );
            pawn.gameObject.transform.position = Vector3.Lerp( p0, p1, firstPathSectionProgress );
        }
        else
        {
            if ( progress < ( firstPathDistance + secondPathDistance ) / pathDistance )
            {
                if ( pawn.focusedTile == null )
                {
                    pawn.onEnterTile( pawn.focusedTile );
                }
                // Move on second path
                var secondPathSectionProgress = ( progress - firstPathDistance / pathDistance ) /
                                                ( secondPathDistance / pathDistance );
                pawn.gameObject.transform.position = Vector3.Lerp( p1, p2, secondPathSectionProgress );
            }
            else
            {
                // Move on third path
                var thirdPathSectionProgress = ( progress - ( firstPathDistance + secondPathDistance ) / pathDistance ) /
                                                ( thirdPathDistance / pathDistance );
                pawn.gameObject.transform.position = Vector3.Lerp( p2, p3, thirdPathSectionProgress );
            }
        }
    }
}
