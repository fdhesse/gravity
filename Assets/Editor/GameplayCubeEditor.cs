using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(GameplayCube))]
[CanEditMultipleObjects]
public class GameplayCubeEditor : Editor
{
	GameplayCube m_Instance;
	PropertyField[] m_fields;
	
	
	public void OnEnable()
	{
		m_Instance = target as GameplayCube;
		m_fields = ExposeProperties.GetProperties( m_Instance );
	}
	
	public override void OnInspectorGUI () {
		
		if ( m_Instance == null )
			return;
		
//		this.DrawDefaultInspector();
		
		ExposeProperties.Expose( m_fields );
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (target);
		//	m_Instance.Refresh ();
		}
	}
}