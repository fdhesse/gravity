using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( ClimbDownAnimatedMotion ) )]
public class ClimbDownAnimatedMotionEditor : AnimatedMotionEditor
{
    ClimbDownAnimatedMotion animatedClimbDownMotion;

    SerializedProperty movementDurationProperty;

    const string MovementDurationPropertyName = "MovementDuration";

    public override void OnEnable()
    {
        base.OnEnable();

        animatedClimbDownMotion = ( ClimbDownAnimatedMotion )target;

        movementDurationProperty = serializedObject.FindProperty( MovementDurationPropertyName );

        animatedClimbDownMotion.Name = "Climb Down Animated Motion";
    }

    public override void PostOnInspectorGUI()
    {
        EditorGUILayout.BeginVertical( GUI.skin.box );

        EditorGUILayout.BeginHorizontal();

        //EditorGUILayout.LabelField( "Description:" );
        //animatedMotion.Description = EditorGUILayout.TextField( animatedMotion.Description );

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField( "Movement Duration:" );
        animatedClimbDownMotion.MovementDuration = EditorGUILayout.FloatField( animatedClimbDownMotion.MovementDuration );

        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup( animatedClimbDownMotion.TargetAnimation == null );

        if ( animatedClimbDownMotion.TargetAnimation == null ||
            ( animatedClimbDownMotion.TargetAnimation != null &&
             animatedClimbDownMotion.MovementDuration != animatedClimbDownMotion.TargetAnimation.length) )
        {
            if ( GUILayout.Button( "Calculate Movement Duration based on Animation" ) )
            {
                var animation = animatedClimbDownMotion.TargetAnimation;

                animatedClimbDownMotion.MovementDuration = animation.length;
            }
        }

        if ( animatedClimbDownMotion.TargetAnimation == null )
        {
            EditorGUILayout.LabelField( "Can't calculate duration because the movement has no animation" );
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndVertical();
    }
}
