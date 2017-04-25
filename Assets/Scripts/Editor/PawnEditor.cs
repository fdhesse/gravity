using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( Pawn ) )]
public class PawnEditor : EditorWithSubEditors<AnimatedMotionEditor, AnimatedMotion>
{
    private Pawn pawn;

    SerializedProperty animatedMotionsCollectionProperty;

    const string AnimatedCollectionsPropertyName = "AnimatedMotions";
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Pull information from the target into the serializedObject.
        serializedObject.Update();

        CheckAndCreateSubEditors( pawn.AnimatedMotions );

        // Display all of the ConditionCollections.
        for ( int i = 0; i < subEditors.Length; i++ )
        {
            subEditors[i].OnInspectorGUI();
            EditorGUILayout.Space();
        }

        // Create a right-aligned button which when clicked, creates a new ConditionCollection in the ConditionCollections array.
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if ( !pawn.HasMotionType( typeof( ClimbDownAnimatedMotion ) ) )
        {
            if ( GUILayout.Button( "Add Climb Down Motion" ) )
            {
                var motion = AnimatedMotionEditor.CreateClimbDownAnimatedMotion( "New Climb Down Motion" );
                animatedMotionsCollectionProperty.AddToObjectArray( motion );
            }
        }

        var allRappelMotions = pawn.GetAllMotionsOfType( typeof(RappelDownAnimatedMotion) );

        var typesNotFound = new List<RappelDownLengthType>();
        foreach ( var rappelType in Enum.GetValues( typeof(RappelDownLengthType) ) )
        {
            var found = false;
            foreach ( var rappelMotion in allRappelMotions )
            {
                if ( ( (RappelDownAnimatedMotion)rappelMotion ).Type == (RappelDownLengthType)rappelType )
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
            if ( GUILayout.Button( "Add Rappel Down " + (int)type ) )
            {
                var motion = AnimatedMotionEditor.CreateRappelDownAnimatedMotion( "New Rappel Down Motion" );
                motion.Type = (RappelDownLengthType)type;
                animatedMotionsCollectionProperty.AddToObjectArray( motion );
            }
        }
        
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

        // Cache the SerializedProperties.
        animatedMotionsCollectionProperty = serializedObject.FindProperty( AnimatedCollectionsPropertyName );

        // Check if the Editors for the Conditions need creating and optionally create them.
        CheckAndCreateSubEditors( pawn.AnimatedMotions );
    }

    void OnDisable()
    {
        // When this Editor ends, destroy all it's subEditors.
        CleanupEditors();
    }

    protected override void SubEditorSetup( AnimatedMotionEditor editor )
    {
        editor.pawnAnimatedMotions = animatedMotionsCollectionProperty;
    }
}
