using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor( typeof(Waterfall) )]
public class WaterfallEditor : Editor
{
    SerializedProperty waterfallTileOrientationProperty;
    const string WaterfallTileOrientationPropertyName = "WaterfallTileOrientation";

    public void OnEnable()
    {
        waterfallTileOrientationProperty = serializedObject.FindProperty( WaterfallTileOrientationPropertyName );
    }

    public void OnDisable()
    {
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        var waterfall = (Waterfall)target;

        var tileColoredButton = new GUIStyle( GUI.skin.button );
        tileColoredButton.normal.textColor = Color.white;
        tileColoredButton.fixedWidth = 200f;
        var tileButton = new GUIStyle( GUI.skin.button );
        tileButton.fixedWidth = 200f;
        var oldColor = GUI.backgroundColor;
        Color activeTileColor;
        ColorUtility.TryParseHtmlString( "#027bb1", out activeTileColor );

        var gravityColoredButton = new GUIStyle( GUI.skin.button );
        gravityColoredButton.normal.textColor = Color.white;
        gravityColoredButton.fixedWidth = 200f;
        var gravityButton = new GUIStyle( GUI.skin.button );
        gravityButton.fixedWidth = 200f;
        Color activeGravityColor;
        ColorUtility.TryParseHtmlString( "#02b102", out activeGravityColor );

        GUILayout.Space( 20 );

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical();

        ////////////////////////
        //  Test Orientation  //
        ////////////////////////

        EditorGUILayout.BeginVertical();
        GUILayout.Label( "Emmiter is on a tile oriented:" );
        GUILayout.Space( 5 );
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        if ( waterfall.WaterfallTileOrientation == TileOrientation.Up )
        {
            GUI.backgroundColor = activeTileColor;
        }
        if ( GUILayout.Button( "Up",
            waterfall.WaterfallTileOrientation == TileOrientation.Up ? tileColoredButton : tileButton ) )
        {
            waterfall.transform.rotation = Quaternion.Euler( 0f, 0f, -90f );
            waterfall.WaterfallTileOrientation = TileOrientation.Up;
            waterfallTileOrientationProperty.intValue = (int)( TileOrientation.Up );
            if ( Pawn.Instance )
            {
                waterfall.ChangeGravity( waterfall.GravityAffectingWaterfall );
            }
        }
        GUI.backgroundColor = oldColor;

        if ( waterfall.WaterfallTileOrientation == TileOrientation.Front )
        {
            GUI.backgroundColor = activeTileColor;
        }
        if ( GUILayout.Button( "Front",
            waterfall.WaterfallTileOrientation == TileOrientation.Front ? tileColoredButton : tileButton ) )
        {
            waterfall.transform.rotation = Quaternion.Euler( 0f, -90f, 0f );
            waterfall.WaterfallTileOrientation = TileOrientation.Front;
            waterfallTileOrientationProperty.intValue = (int)( TileOrientation.Front );

            if ( Pawn.Instance )
            {
                waterfall.ChangeGravity( waterfall.GravityAffectingWaterfall );
            }
        }
        GUI.backgroundColor = oldColor;

        if ( waterfall.WaterfallTileOrientation == TileOrientation.Left )
        {
            GUI.backgroundColor = activeTileColor;
        }
        if ( GUILayout.Button( "Left",
            waterfall.WaterfallTileOrientation == TileOrientation.Left ? tileColoredButton : tileButton ) )
        {
            waterfall.transform.rotation = Quaternion.Euler( 0f, 0f, 0f );
            waterfall.WaterfallTileOrientation = TileOrientation.Left;
            waterfallTileOrientationProperty.intValue = (int)( TileOrientation.Left );

            if ( Pawn.Instance )
            {
                waterfall.ChangeGravity( waterfall.GravityAffectingWaterfall );
            }
        }
        GUI.backgroundColor = oldColor;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        if ( waterfall.WaterfallTileOrientation == TileOrientation.Down )
        {
            GUI.backgroundColor = activeTileColor;
        }
        if ( GUILayout.Button( "Down",
            waterfall.WaterfallTileOrientation == TileOrientation.Down ? tileColoredButton : tileButton ) )
        {
            waterfall.transform.rotation = Quaternion.Euler( 0f, 0f, 90f );
            waterfall.WaterfallTileOrientation = TileOrientation.Down;
            waterfallTileOrientationProperty.intValue = (int)( TileOrientation.Down );

            if ( Pawn.Instance )
            {
                waterfall.ChangeGravity( waterfall.GravityAffectingWaterfall );
            }
        }
        GUI.backgroundColor = oldColor;

        if ( waterfall.WaterfallTileOrientation == TileOrientation.Back )
        {
            GUI.backgroundColor = activeTileColor;
        }
        if ( GUILayout.Button( "Back",
            waterfall.WaterfallTileOrientation == TileOrientation.Back ? tileColoredButton : tileButton ) )
        {
            waterfall.transform.rotation = Quaternion.Euler( 0f, 90f, 0f );
            waterfall.WaterfallTileOrientation = TileOrientation.Back;
            waterfallTileOrientationProperty.intValue = (int)( TileOrientation.Back );

            if ( Pawn.Instance )
            {
                waterfall.ChangeGravity( waterfall.GravityAffectingWaterfall );
            }
        }
        GUI.backgroundColor = oldColor;

        if ( waterfall.WaterfallTileOrientation == TileOrientation.Right )
        {
            GUI.backgroundColor = activeTileColor;
        }
        if ( GUILayout.Button( "Right",
            waterfall.WaterfallTileOrientation == TileOrientation.Right ? tileColoredButton : tileButton ) )
        {
            waterfall.transform.rotation = Quaternion.Euler( 0f, 180f, 0f );
            waterfall.WaterfallTileOrientation = TileOrientation.Right;
            waterfallTileOrientationProperty.intValue = (int)( TileOrientation.Right );

            if ( Pawn.Instance )
            {
                waterfall.ChangeGravity( waterfall.GravityAffectingWaterfall );
            }
        }
        GUI.backgroundColor = oldColor;

        EditorGUILayout.EndHorizontal();
        GUILayout.Label( "This will rotate the waterfall accordingly", EditorStyles.miniLabel );
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        GUILayout.Space( 20 );

        ////////////////////
        //  Test Gravity  //
        ////////////////////

        EditorGUI.BeginDisabledGroup( !Application.isPlaying );
        EditorGUILayout.BeginVertical();

        GUILayout.Label( "Use this to test gravity effect on waterfall:" );
        if ( !Application.isPlaying )
        {
            GUILayout.Label( "Overriding gravity values for the waterfall only works when game is playing",
                EditorStyles.whiteBoldLabel );
        }
        GUILayout.Space( 5 );
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        if ( waterfall.GravityAffectingWaterfall == TileOrientation.Up )
        {
            GUI.backgroundColor = activeGravityColor;
        }
        if ( GUILayout.Button( "Gravity Down",
            waterfall.GravityAffectingWaterfall == TileOrientation.Up ? tileColoredButton : tileButton ) )
        {
            waterfall.ChangeGravity( TileOrientation.Up );
        }
        GUI.backgroundColor = oldColor;

        if ( waterfall.GravityAffectingWaterfall == TileOrientation.Front )
        {
            GUI.backgroundColor = activeGravityColor;
        }
        if ( GUILayout.Button( "Gravity Front",
            waterfall.GravityAffectingWaterfall == TileOrientation.Front ? tileColoredButton : tileButton ) )
        {
            waterfall.ChangeGravity( TileOrientation.Front );
        }
        GUI.backgroundColor = oldColor;

        if ( waterfall.GravityAffectingWaterfall == TileOrientation.Left )
        {
            GUI.backgroundColor = activeGravityColor;
        }
        if ( GUILayout.Button( "Gravity Left",
            waterfall.GravityAffectingWaterfall == TileOrientation.Left ? tileColoredButton : tileButton ) )
        {
            waterfall.ChangeGravity( TileOrientation.Left );
        }
        GUI.backgroundColor = oldColor;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        if ( waterfall.GravityAffectingWaterfall == TileOrientation.Down )
        {
            GUI.backgroundColor = activeGravityColor;
        }
        if ( GUILayout.Button( "Gravity Up",
            waterfall.GravityAffectingWaterfall == TileOrientation.Down ? gravityColoredButton : gravityButton ) )
        {
            waterfall.ChangeGravity( TileOrientation.Down );
        }
        GUI.backgroundColor = oldColor;

        if ( waterfall.GravityAffectingWaterfall == TileOrientation.Back )
        {
            GUI.backgroundColor = activeGravityColor;
        }
        if ( GUILayout.Button( "Gravity Back",
            waterfall.GravityAffectingWaterfall == TileOrientation.Back ? gravityColoredButton : gravityButton ) )
        {
            waterfall.ChangeGravity( TileOrientation.Back );
        }
        GUI.backgroundColor = oldColor;

        if ( waterfall.GravityAffectingWaterfall == TileOrientation.Right )
        {
            GUI.backgroundColor = activeGravityColor;
        }
        if ( GUILayout.Button( "Gravity Right",
            waterfall.GravityAffectingWaterfall == TileOrientation.Right ? gravityColoredButton : gravityButton ) )
        {
            waterfall.ChangeGravity( TileOrientation.Right );
        }
        GUI.backgroundColor = oldColor;

        EditorGUILayout.EndHorizontal();
        GUILayout.Label( "Will temporarilly override gravity ONLY FOR THIS WATERFALL", EditorStyles.miniLabel );

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space( 20 );

        serializedObject.ApplyModifiedProperties();
    }

    Texture2D GetColoredTexture( Color color )
    {
        var texture = new Texture2D( 2, 2, TextureFormat.ARGB32, false );

        // set the pixel values
        texture.SetPixel( 0, 0, color );
        texture.SetPixel( 1, 0, color );
        texture.SetPixel( 0, 1, color );
        texture.SetPixel( 1, 1, color );

        // Apply all SetPixel calls
        texture.Apply();

        return texture;
    }
}