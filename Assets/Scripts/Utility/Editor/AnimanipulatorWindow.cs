using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class AnimanipulatorWindow : EditorWindow
{
    AnimationClip animationClip;
    string resultAnimationSavePath = "Assets/Animation/Waterfall/Sorted Animations/Derived/";
    string resultAnimationName = "";

    const string AnimationClipLabel = "Target Animation";
    const string ResultAnimationSavePathLabel = "Path to save";
    const string ResultAnimationNameLabel = "Animation name";

    [MenuItem( "Mu/Animanipulator" )] public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow( typeof(AnimanipulatorWindow) );
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space( 10 );
        var guiStyle = EditorStyles.centeredGreyMiniLabel;
        guiStyle.fontSize = 20;
        guiStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label( "Animanipulator", guiStyle );
        GUILayout.Space( 20 );
        animationClip = EditorGUILayout.ObjectField(
            AnimationClipLabel, animationClip, typeof(AnimationClip), false ) as AnimationClip;
        GUILayout.Space( 20 );

        resultAnimationSavePath = EditorGUILayout.TextField( ResultAnimationSavePathLabel, resultAnimationSavePath );
        resultAnimationName = EditorGUILayout.TextField( ResultAnimationNameLabel, resultAnimationName );

        //if ( GUILayout.Button( "Rotate Animation 180 on x (left)" ) )
        //{
        //    RotateOnAxisAndSave( animationClip, new Vector3( 180f, 0f, 0f ) );
        //}

        if ( GUILayout.Button( "Rotate Animation 90 on x (left)" ) )
        {
            RotateOnAxisAndSave( animationClip, new Vector3( 90f, 0f, 0f ) );
        }

        if ( GUILayout.Button( "Rotate Animation -90 on the x (right)" ) )
        {
            RotateOnAxisAndSave( animationClip, new Vector3( -90, 0f, 0f ) );
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

        float clipLength = sourceAnimation.length;
        var curves = AnimationUtility.GetAllCurves( sourceAnimation, true );

        foreach ( AnimationClipCurveData curve in curves )
        {
            var keys = curve.curve.keys;
            int keyCount = keys.Length;
            var postWrapmode = curve.curve.postWrapMode;
            curve.curve.postWrapMode = curve.curve.preWrapMode;
            curve.curve.preWrapMode = postWrapmode;
            for ( int i = 0; i < keyCount; i++ )
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
            for ( int i = 0; i < events.Length; i++ )
            {
                events[i].time = resultAnimation.length - events[i].time;
            }
            AnimationUtility.SetAnimationEvents( resultAnimation, events );
        }

        AssetDatabase.CreateAsset( resultAnimation,
            Path.ChangeExtension( Path.Combine( resultAnimationSavePath, resultAnimationName ), "anim" ) );
    }

    void RotateOnAxisAndSave( AnimationClip sourceAnimation, Vector3 axialRotation )
    {
        var resultAnimation = new AnimationClip();
        foreach ( var binding in AnimationUtility.GetCurveBindings( sourceAnimation ) )
        {
            var curve = AnimationUtility.GetEditorCurve( sourceAnimation, binding );
            Debug.Log( binding.propertyName );
            if ( binding.path.Equals( "joint1" ) && binding.propertyName.Contains( "localEulerAnglesRaw.y" ) )
            {
                var keyClones = new Keyframe[curve.keys.Length];
                for ( var i = 0; i < curve.keys.Length; i++ )
                {
                    var keys = curve.keys;
                    var keyframe = keys[i];
                    Debug.Log( keyframe.value );
                    keyframe.value += axialRotation.y;

                    keyClones[i] = keyframe;
                }
                curve.keys = keyClones;
            }
            if ( binding.path.Equals( "joint1" ) && binding.propertyName.Contains( "localEulerAnglesRaw.x" ) )
            {
                var keyClones = new Keyframe[curve.keys.Length];
                for ( var i = 0; i < curve.keys.Length; i++ )
                {
                    var keys = curve.keys;
                    var keyframe = keys[i];
                    Debug.Log( keyframe.value );
                    keyframe.value += axialRotation.x;

                    keyClones[i] = keyframe;
                }
                curve.keys = keyClones;
            }
            if ( binding.path.Equals( "joint1" ) && binding.propertyName.Contains( "localEulerAnglesRaw.z" ) )
            {
                var keyClones = new Keyframe[curve.keys.Length];
                for ( var i = 0; i < curve.keys.Length; i++ )
                {
                    var keys = curve.keys;
                    var keyframe = keys[i];
                    Debug.Log( keyframe.value );
                    keyframe.value += axialRotation.z;

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