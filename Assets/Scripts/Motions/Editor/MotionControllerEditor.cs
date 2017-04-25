
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( MotionController ) )]
public class MotionControllerEditor : EditorWithSubEditors<ClimbDownAnimatedMotionEditor, ClimbDownAnimatedMotion>
{
    private MotionController controller;

    SerializedProperty animatedMotionsProperty;

    const string AnimatedMotionsPropertyName = "AnimatedMotions";


    public void OnEnable()
    {
        // Cache a reference to the target.
        controller = ( MotionController )target;

        // If this Editor exists but isn't targeting anything destroy it.
        //if ( target == null )
        //{
        //    DestroyImmediate( this );
        //    return;
        //}

        // Cache the SerializedProperties.
        animatedMotionsProperty = serializedObject.FindProperty( AnimatedMotionsPropertyName );

        // Check if the Editors for the Animations need creating and optionally create them.
        CheckAndCreateSubEditors( controller.AnimatedMotions );
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        // Pull information from the target into the serializedObject.
        CheckAndCreateSubEditors( controller.AnimatedMotions );

        DrawDefaultInspector();


        // Display all of the Animated Motions
        for ( int i = 0; i < subEditors.Length; i++ )
        {
            subEditors[i].OnInspectorGUI();
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
                animatedMotionsProperty.AddToObjectArray( motion );
            }
        }

        //var allRappelMotions = controller.GetAllMotionsOfType( typeof( RappelDownAnimatedMotion ) );

        //var typesNotFound = new List<RappelDownLengthType>();
        //foreach ( var rappelType in Enum.GetValues( typeof( RappelDownLengthType ) ) )
        //{
        //    var found = false;
        //    foreach ( var rappelMotion in allRappelMotions )
        //    {
        //        if ( ( ( RappelDownAnimatedMotion )rappelMotion ).Type == ( RappelDownLengthType )rappelType )
        //        {
        //            found = true;
        //            break;
        //        }
        //    }
        //    if ( !found )
        //    {
        //        typesNotFound.Add( ( RappelDownLengthType )rappelType );
        //    }
        //}
        //foreach ( var type in typesNotFound )
        //{
        //    if ( GUILayout.Button( "Add Rappel Down " + ( int )type ) )
        //    {
        //        var motion = AnimatedMotionEditor.CreateRappelDownAnimatedMotion( "New Rappel Down Motion" );
        //        motion.Type = type;
        //        animatedMotionsCollectionProperty.AddToObjectArray( motion );
        //    }
        //}

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }


    public void OnDisable()
    {
        // When this Editor ends, destroy all it's subEditors.
        CleanupEditors();
    }

    //protected override void SubEditorSetup( AnimatedMotionEditor editor )
    //{
    //    editor.ControllerMotionsArray = animatedMotionsCollectionProperty;
    //}

    protected override void SubEditorSetup( ClimbDownAnimatedMotionEditor editor )
    {
        editor.ControllerMotionsArray = animatedMotionsProperty;
    }
}
