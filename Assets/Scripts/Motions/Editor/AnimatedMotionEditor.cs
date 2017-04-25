using UnityEditor;
using UnityEngine;

[CustomEditor( typeof(AnimatedMotion) )]
public class AnimatedMotionEditor : Editor
{
    public SerializedProperty pawnAnimatedMotions;

    AnimatedMotion animatedMotion;

    SerializedProperty nameProperty;
    SerializedProperty descriptionProperty;
    SerializedProperty animatedClipProperty;

    const string NamePropertyName = "Name";
    const string DescriptionPropertyName = "Description";
    const string AnimatedClipPropertyName = "TargetAnimation";

    public virtual void OnEnable()
    {
        // Cache the target.
        animatedMotion = (AnimatedMotion)target;

        // If this Editor has persisted through the destruction of it's target then destroy it.
        if ( target == null )
        {
            DestroyImmediate( this );
            return;
        }

        // Cache the SerializedProperties.
        nameProperty = serializedObject.FindProperty( NamePropertyName );
        descriptionProperty = serializedObject.FindProperty( DescriptionPropertyName );
        animatedClipProperty = serializedObject.FindProperty( AnimatedClipPropertyName );
    }

    public void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal( GUI.skin.box );
        EditorGUI.indentLevel++;

        EditorGUILayout.BeginVertical();

        PreOnInspectorGUI();

        EditorGUILayout.BeginHorizontal();

        var guiStyle = EditorStyles.foldout;
        guiStyle.fontSize = 13;
        guiStyle.fontStyle = FontStyle.Bold;
        nameProperty.isExpanded = EditorGUILayout.Foldout( nameProperty.isExpanded, nameProperty.stringValue, guiStyle );
        //EditorGUILayout.LabelField( nameProperty.stringValue, guiStyle );

        var removeMotionButton = new GUIStyle( GUI.skin.button );
        removeMotionButton.normal.textColor = Color.white;
        removeMotionButton.fixedWidth = 20f;
        var oldColor = GUI.backgroundColor;
        Color redButtonColor;
        ColorUtility.TryParseHtmlString( "#c64141", out redButtonColor );
        GUI.backgroundColor = redButtonColor;

        if ( GUILayout.Button( "x", removeMotionButton ) )
        {
            pawnAnimatedMotions.RemoveFromObjectArray( animatedMotion );
        }

        GUI.backgroundColor = oldColor;


            EditorGUILayout.EndHorizontal();
        if ( nameProperty.isExpanded )
        {
            EditorGUILayout.BeginVertical( GUI.skin.box );

            EditorGUILayout.LabelField( "Description:" );
            animatedMotion.Description = EditorGUILayout.TextArea( animatedMotion.Description, GUILayout.Height( 50 ) );

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField( "Target Animation:" );
            animatedMotion.TargetAnimation =
                (AnimationClip)
                    EditorGUILayout.ObjectField( animatedMotion.TargetAnimation, typeof(AnimationClip), true );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

        PostOnInspectorGUI();
        }

        EditorGUILayout.EndVertical();

        EditorGUI.indentLevel--;
            EditorGUILayout.EndHorizontal();
    }

    public virtual void PreOnInspectorGUI()
    {
    }

    public virtual void PostOnInspectorGUI()
    {
    }

    public static AnimatedMotion CreateAnimatedMotion( string description )
    {
        var newMotion = CreateInstance<AnimatedMotion>();

        newMotion.Description = description;
        return newMotion;
    }

    public static ClimbDownAnimatedMotion CreateClimbDownAnimatedMotion( string description )
    {
        var newMotion = CreateInstance<ClimbDownAnimatedMotion>();

        newMotion.Description = description;
        return newMotion;
    }

    public static RappelDownAnimatedMotion CreateRappelDownAnimatedMotion( string description )
    {
        var newMotion = CreateInstance<RappelDownAnimatedMotion>();

        newMotion.Description = description;
        return newMotion;
    }
}