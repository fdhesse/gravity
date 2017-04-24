public class ClimbDownAnimatedMotion : AnimatedMotion {

    public float MovementDuration = 0.3f;

    public void Move( Pawn pawn )
    {
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
        } ) );

        // the modification in orientation
        //if ( lookCoroutine != null )
        //    StopCoroutine( lookCoroutine );
        //lookCoroutine = LookAt( clickedTile.transform.position );
        //StartCoroutine( lookCoroutine );
    }

}
