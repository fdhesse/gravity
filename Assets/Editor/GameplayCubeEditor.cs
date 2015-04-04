using UnityEditor;
using UnityEngine;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(GameplayCube))]
public class GameplayCubeEditor : Editor
{

	GameplayCube m_Instance;
	PropertyField[] m_fields;
	
	public void OnEnable()
	{
		m_Instance = target as GameplayCube;
		m_fields = ExposeProperties.GetProperties( m_Instance );
	}
	
	public override void OnInspectorGUI ()
	{
		//serializedObject.Update ();

		if ( m_Instance == null )
			return;

		this.DrawDefaultInspector();
		
		ExposeProperties.Expose( m_fields );

		if (GUI.changed)
		{
			EditorUtility.SetDirty (target);
		//	m_Instance.Refresh ();
		}

		//serializedObject.ApplyModifiedProperties ();
	}
}