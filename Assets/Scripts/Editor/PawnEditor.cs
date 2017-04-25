using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( Pawn ) )]
public class PawnEditor : EditorWithSubEditors<AnimatedMotionEditor, AnimatedMotion>
{
    private Pawn pawn;

    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Pull information from the target into the serializedObject.
        serializedObject.Update();

        if(pawn.MotionController != null && pawn.MotionController.AnimatedMotions.Length > 0 ) { 
            CheckAndCreateSubEditors( pawn.MotionController.AnimatedMotions );
        }

        // Display all of the ConditionCollections.
        for ( int i = 0; i < subEditors.Length; i++ )
        {
            subEditors[i].OnInspectorGUI();
            EditorGUILayout.Space();
        }

        // Create a right-aligned button which when clicked, creates a new ConditionCollection in the ConditionCollections array.
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();


        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }
    
    public void OnEnable()
    {
        // Cache a reference to the target.
        pawn = ( Pawn )target;

        // If this Editor exists but isn't targeting anything destroy it.
        if ( target == null )
        {
            DestroyImmediate( this );
            return;
        }

        // Check if the Editors for the Conditions need creating and optionally create them.
        if ( pawn.MotionController != null && pawn.MotionController.AnimatedMotions.Length > 0 )
        {
            CheckAndCreateSubEditors( pawn.MotionController.AnimatedMotions );
        }
    }

    void OnDisable()
    {
        // When this Editor ends, destroy all it's subEditors.
        CleanupEditors();
    }

    protected override void SubEditorSetup( AnimatedMotionEditor editor )
    {
    }
}
