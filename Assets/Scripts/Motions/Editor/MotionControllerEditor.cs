
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( MotionController ) )]
public class MotionControllerEditor : Editor
{
    private MotionController controller;

    public AnimatedMotionEditor[] AnimatedMotionEditors;

    SerializedProperty animatedMotionsProperty;
    const string AnimatedMotionsPropertyName = "AnimatedMotions";

    private const string creationPath = "Assets/Resources/MotionController.asset";

    public void OnEnable()
    {
        // Cache a reference to the target.
        controller = ( MotionController )target;

        // If there aren't any Conditions on the target, create an empty array of Conditions.
        if ( controller.AnimatedMotions == null ) {
            controller.AnimatedMotions = new AnimatedMotion[0];
        }
        // If there aren't any editors, create them.
        if ( AnimatedMotionEditors == null )
        {
            CreateEditors();
        }

        // Cache the SerializedProperties.
        animatedMotionsProperty = serializedObject.FindProperty( AnimatedMotionsPropertyName );
    }

    void CreateEditors()
    {
        AnimatedMotionEditors = new AnimatedMotionEditor[controller.AnimatedMotions.Length];

        // Go through all the empty array...
        for ( int i = 0; i < AnimatedMotionEditors.Length; i++ )
        {
            // ... and create an editor with an editor type to display correctly.
            AnimatedMotionEditors[i] = CreateEditor( controller.TryGetAnimatedMotionAt( i ) ) as AnimatedMotionEditor;
            Debug.Assert( AnimatedMotionEditors[i] != null);
            AnimatedMotionEditors[i].nameProperty.isExpanded = false;
            AnimatedMotionEditors[i].ParentEditor = this;
            AnimatedMotionEditors[i].Controller = controller;
        }
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        // Pull information from the target into the serializedObject.
        DrawDefaultInspector();

        foreach ( var editor in AnimatedMotionEditors )
        {
            editor.OnInspectorGUI();
            EditorGUILayout.Space();
        }

        // Create a right-aligned button which when clicked, creates a new Motion in the Motions array.
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        if ( !controller.HasMotionType( typeof( ClimbDownAnimatedMotion ) ) )
        {
            if ( GUILayout.Button( "Add Climb Down Motion" ) )
            {
                var motion = AnimatedMotionEditor.CreateClimbDownAnimatedMotion( "New Climb Down Motion" );
                AssetDatabase.AddObjectToAsset( motion,target );

                var list = controller.AnimatedMotions.ToList();
                list.Add( motion );
                controller.AnimatedMotions = list.ToArray();
                CreateEditors();
            }
        }

        var allRappelMotions = controller.GetAllMotionsOfType( typeof( RappelDownAnimatedMotion ) );

        var typesNotFound = new List<RappelDownLengthType>();
        foreach ( var rappelType in Enum.GetValues( typeof( RappelDownLengthType ) ) )
        {
            var found = false;
            foreach ( var rappelMotion in allRappelMotions )
            {
                if ( ( ( RappelDownAnimatedMotion )rappelMotion ).Type == ( RappelDownLengthType )rappelType )
                {
                    found = true;
                    break;
                }
            }
            if ( !found )
            {
                typesNotFound.Add( ( RappelDownLengthType )rappelType );
            }
        }
        foreach ( var type in typesNotFound )
        {
            if ( GUILayout.Button( "Add Rappel Down " + ( int )type ) )
            {
                var motion = AnimatedMotionEditor.CreateRappelDownAnimatedMotion( "New Rappel Down Motion" );
                motion.Type = type;
                AssetDatabase.AddObjectToAsset( motion, target );

                var list = controller.AnimatedMotions.ToList();
                list.Add( motion );
                controller.AnimatedMotions = list.ToArray();
                CreateEditors();
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }


    public void OnDisable()
    {
        // When this Editor ends, destroy all it's subEditors.
        CleanupEditors();
    }
    protected void CleanupEditors()
    {
        // If there are no subEditors do nothing.
        if ( AnimatedMotionEditors == null )
            return;

        // Otherwise destroy all the subEditors.
        for ( int i = 0; i < AnimatedMotionEditors.Length; i++ )
        {
            DestroyImmediate( AnimatedMotionEditors[i] );
        }

        // Null the array so it's GCed.
        AnimatedMotionEditors = null;
    }

    //protected override void SubEditorSetup( AnimatedMotionEditor editor )
    //{
    //    editor.ControllerMotionsArray = animatedMotionsCollectionProperty;
    //}

    //protected override void SubEditorSetup( ClimbDownAnimatedMotionEditor editor )
    //{
    //    editor.ControllerMotionsArray = animatedMotionsProperty;
    //}
}
