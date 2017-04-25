using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( Pawn ) )]
public class PawnEditor : Editor
{
    private Pawn pawn;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Pull information from the target into the serializedObject.
        serializedObject.Update();

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

    }

    void OnDisable()
    {
    }
}
