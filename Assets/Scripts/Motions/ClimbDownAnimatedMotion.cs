public class ClimbDownAnimatedMotion : AnimatedMotion {

    public float MovementDuration = 0.3f;

    public void OnValidate()
    {
        Name = "Climb Down Animated Motion";
    }

    public void Move( Pawn pawn, float progress )
    {
        var tilePosition = pawn.focusedTile.Position;
        var direction = tilePosition - pawn.transform.position;

        pawn.isClimbingDown = true;
        pawn.isFalling = true;

        pawn.animState = 4;
        pawn.Animator.SetTrigger( "Transitioning" );

        // reset the pawn tile when starting to climb down, because if you climb down from
        // a moving platform, you don't want to climb down relative to the plateform
        pawn.onEnterTile( null );

        // the modification in height
        pawn.StartCoroutine( SectionedMovement.MoveDown( pawn, 0, MovementDuration, () =>
        {
            pawn.isClimbingDown = false;
            pawn.clickedTile = null; // target reached, forget it
                                     // the modification in orientation
            //pawn.LookAtPosition( pawn.transform.position + direction );
        } ) );

        // look the other way
        // pawn.LookAtPosition( pawn.transform.position - direction );

        pawn.LookAtPosition( tilePosition );
    }

}
