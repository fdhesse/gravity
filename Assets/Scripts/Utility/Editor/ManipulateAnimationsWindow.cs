using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ManipulateAnimationsWindow : EditorWindow
{
    AnimationClip animationClip;
    string resultAnimationSavePath = "Assets/Animations/";
    string resultAnimationName = "";

    const string AnimationClipLabel = "Target Animation";
    const string ResultAnimationSavePathLabel = "Path to save";
    const string ResultAnimationNameLabel = "Animation name";

    [MenuItem( "Mu/Manipulate Animations" )] public static void ShowWindow()
    {
        GetWindow( typeof(ManipulateAnimationsWindow) );
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space( 10 );
        var guiStyle = EditorStyles.centeredGreyMiniLabel;
        guiStyle.fontSize = 20;
        guiStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label( "Manipulate Animations", guiStyle );
        GUILayout.Space( 20 );
        animationClip = EditorGUILayout.ObjectField(
            AnimationClipLabel, animationClip, typeof(AnimationClip), false ) as AnimationClip;
        GUILayout.Space( 20 );

        resultAnimationSavePath = EditorGUILayout.TextField( ResultAnimationSavePathLabel, resultAnimationSavePath );
        resultAnimationName = EditorGUILayout.TextField( ResultAnimationNameLabel, resultAnimationName );

        if ( GUILayout.Button( "Rotate Animation 90 on x (left)" ) )
        {
            RotateAndSave( animationClip, new Vector3( 90f, 0f, 0f ) );
        }

        if ( GUILayout.Button( "Rotate Animation -90 on the x (right)" ) )
        {
            RotateAndSave( animationClip, new Vector3( -90, 0f, 0f ) );
        }

        if ( GUILayout.Button( "Reverse Animation" ) )
        {
            ReverseAndSave( animationClip );
        }

        EditorGUILayout.EndVertical();
    }

    void ReverseAndSave( AnimationClip sourceAnimation )
    {
        var resultAnimation = new AnimationClip();

        var clipLength = sourceAnimation.length;
        var curves = AnimationUtility.GetAllCurves( sourceAnimation, true );

        foreach ( var curve in curves )
        {
            var keys = curve.curve.keys;
            var keyCount = keys.Length;
            var postWrapmode = curve.curve.postWrapMode;
            curve.curve.postWrapMode = curve.curve.preWrapMode;
            curve.curve.preWrapMode = postWrapmode;
            for ( var i = 0; i < keyCount; i++ )
            {
                var keyframe = keys[i];
                keyframe.time = clipLength - keyframe.time;
                var tmp = -keyframe.inTangent;
                keyframe.inTangent = -keyframe.outTangent;
                keyframe.outTangent = tmp;
                keys[i] = keyframe;
            }
            curve.curve.keys = keys;
            resultAnimation.SetCurve( curve.path, curve.type, curve.propertyName, curve.curve );
        }

        var events = AnimationUtility.GetAnimationEvents( resultAnimation );
        if ( events.Length > 0 )
        {
            for ( var i = 0; i < events.Length; i++ )
            {
                events[i].time = resultAnimation.length - events[i].time;
            }
            AnimationUtility.SetAnimationEvents( resultAnimation, events );
        }

        AssetDatabase.CreateAsset( resultAnimation,
            Path.ChangeExtension( Path.Combine( resultAnimationSavePath, resultAnimationName ), "anim" ) );
    }

    // Applies some axial rotation to the animation's first joint
    void RotateAndSave( AnimationClip sourceAnimation, Vector3 axialRotation )
    {
        var resultAnimation = new AnimationClip();

        var bindings = AnimationUtility.GetCurveBindings( sourceAnimation );
        var firstBinding = bindings[0];

        var firstNode = firstBinding.path.Split( '/' )[0];
        foreach ( var binding in AnimationUtility.GetCurveBindings( sourceAnimation ) )
        {
            var curve = AnimationUtility.GetEditorCurve( sourceAnimation, binding );
            if ( binding.path.Equals( firstNode ) && binding.propertyName.Contains( "localEulerAnglesRaw." ) )
            {
                var keyClones = new Keyframe[curve.keys.Length];
                for ( var i = 0; i < curve.keys.Length; i++ )
                {
                    var keys = curve.keys;
                    var keyframe = keys[i];
                    switch ( binding.propertyName.Last() )
                    {
                        case 'x':
                            keyframe.value += axialRotation.x;
                            break;
                        case 'y':
                            keyframe.value += axialRotation.y;
                            break;
                        case 'z':
                            keyframe.value += axialRotation.z;
                            break;
                        default:
                            throw new UnityException( string.Format( "Unknown binding property: {0}", binding.propertyName ) );
                    }

                    keyClones[i] = keyframe;
                }
                curve.keys = keyClones;
            }
            AnimationUtility.SetEditorCurve( resultAnimation, binding, curve );

        }

        AssetDatabase.CreateAsset( resultAnimation,
            Path.ChangeExtension( Path.Combine( resultAnimationSavePath, resultAnimationName ), "anim" ) );
    }
}