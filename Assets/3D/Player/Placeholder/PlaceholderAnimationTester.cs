using UnityEngine;

public class PlaceholderAnimationTester : MonoBehaviour
{
    Pawn pawn;
    Animator animator;

    public void Start()
    {
        pawn = FindObjectOfType( typeof(Pawn) ) as Pawn;
        if ( pawn != null )
        {
            animator = pawn.Animator;
        }
        else
        {
            Debug.LogError( "Couldnt find Pawn" );
        }
    }

    public void OnGUI()
    {
        var style = new GUIStyle { fontSize = 30 };

        GUI.Label( new Rect( Screen.width / 2 - 200, 50, 200, 200 ), GetStateName(), style );
    }

    public string GetStateName()
    {
        var animStateNames = new[]
        {
            "Idle", "Fall", "Land", "Walk", "ClimbDown", "StartRappel", "Rappel20", "Rappel30", "Rappel40", "Rappel50",
            "Rappel60", "Rappel70"
        };

        var animationName = "Unknown";
        foreach ( var possibleName in animStateNames )
        {
            if ( animator.GetCurrentAnimatorStateInfo( 0 ).IsName( possibleName ) )
            {
                animationName = possibleName;
            }
        }
        return animationName;
    }
}