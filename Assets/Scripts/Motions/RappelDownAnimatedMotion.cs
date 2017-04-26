using UnityEngine;

public enum RappelDownLengthType
{
    TwoBlocks = 20,
    ThreeBlocks = 30,
    FourBlocks = 40,
    FiveBlocks = 50,
    SixBlocks = 60,
    SevenBlocks = 70
}

public class RappelDownAnimatedMotion : AnimatedMotion
{
    public AnimationClip StartRappelAnimationClip;
    public RappelDownLengthType Type;
    public float MovementDuration = 0.3f;

    public void Move( Pawn pawn )
    {
        var tilePosition = pawn.focusedTile.Position;

        var direction = tilePosition - pawn.transform.position;
        var horizontalDirection = new Vector3(direction.x,0,direction.z);

        var rappelDistance = Mathf.Abs( pawn.pawnTile.transform.position.y - pawn.focusedTile.transform.position.y );
        var numberOfCubes = (int)( rappelDistance / 10 );

        //throw new NotImplementedException();
        pawn.isRappelingDown = true;
        pawn.isFalling = true;

        pawn.animState = numberOfCubes + 3;

        pawn.Animator.SetTrigger( "Transitioning" );

        // reset the pawn tile when starting to climb down, because if you climb down from
        // a moving platform, you don't want to climb down relative to the plateform
        pawn.onEnterTile( null );

        // the modification in height
        pawn.StartCoroutine( SectionedMovement.MoveDown( pawn, 0, MovementDuration, () =>
        {
            pawn.isRappelingDown = false;
            pawn.clickedTile = null; // target reached, forget it
            //pawn.LookAtPosition( pawn.transform.position + direction );
        } ) );

        // Look the other way
        // pawn.LookAtPosition( pawn.transform.position - ( tilePosition - pawn.transform.position ) );

        pawn.LookAtPosition( tilePosition );
    }
}