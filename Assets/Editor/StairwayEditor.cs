using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(Stairway))]
[CanEditMultipleObjects]
public class StairwayEditor : Editor
{
	Stairway m_Instance;
	PropertyField[] m_fields;
	
	public void OnEnable()
	{
		m_Instance = target as Stairway;
		m_fields = ExposeProperties.GetProperties( m_Instance );
	}
	
	public override void OnInspectorGUI ()
	{
		if ( m_Instance == null )
			return;
		
		ExposeProperties.Expose( m_fields );
		
		if (GUI.changed)
			EditorUtility.SetDirty (target);
	}
}