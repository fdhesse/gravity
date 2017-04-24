using UnityEngine;
using UnityEditor;

[CustomEditor( typeof(RappelDownAnimatedMotion) )]
public class AnimatedRappelMotionEditor : AnimatedMotionEditor
{
    RappelDownAnimatedMotion animatedRappelDownMotion;

    SerializedProperty movementDurationProperty;
    SerializedProperty TypeProperty;

    const string MovementDurationPropertyName = "MovementDuration";
    const string TypePropertyName = "Type";

    public override void OnEnable()
    {
        base.OnEnable();
        animatedRappelDownMotion = (RappelDownAnimatedMotion)target;

        movementDurationProperty = serializedObject.FindProperty( MovementDurationPropertyName );
        TypeProperty = serializedObject.FindProperty( TypePropertyName );

        animatedMotionName = string.Format( "Animated Rappel Down {0} Motion", ( int )animatedRappelDownMotion.Type );
    }

    public override void PreOnInspectorGUI()
    {
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
        animatedRappelDownMotion.MovementDuration =
            EditorGUILayout.FloatField( animatedRappelDownMotion.MovementDuration );

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        animatedRappelDownMotion.Type =
            (RappelDownLengthType) EditorGUILayout.EnumPopup( "Type of Rappel Down motion:", animatedRappelDownMotion.Type );
        animatedMotionName = string.Format( "Animated Rappel Down {0} Motion", ( int )animatedRappelDownMotion.Type );

        //animatedClimbDownMotion.MovementDuration = EditorGUILayout.FloatField( animatedClimbDownMotion.MovementDuration );

        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup( animatedRappelDownMotion.TargetAnimation == null );

        if ( animatedRappelDownMotion.TargetAnimation == null ||
             ( animatedRappelDownMotion.TargetAnimation != null &&
               animatedRappelDownMotion.MovementDuration != animatedRappelDownMotion.TargetAnimation.length ) )
        {
            if ( GUILayout.Button( "Calculate Movement Duration based on Animation" ) )
            {
                var animation = animatedRappelDownMotion.TargetAnimation;

                animatedRappelDownMotion.MovementDuration = animation.length;
            }
        }

        if ( animatedRappelDownMotion.TargetAnimation == null )
        {
            EditorGUILayout.LabelField( "Can't calculate duration because the movement has no animation" );
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndVertical();
    }
}