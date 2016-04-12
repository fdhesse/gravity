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

		bool isEditingThePrefab = (PrefabUtility.GetPrefabType(m_Instance) == PrefabType.Prefab);

		if (isEditingThePrefab)
		{
			EditorGUILayout.HelpBox("Gameplay cube prefab cannot be edited directly. Please add the prefab in the scene, modify it, then apply the modifications.", MessageType.Info);
		}
		else
		{
			this.DrawDefaultInspector();
		
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField("Faces", EditorStyles.boldLabel);
			ExposeProperties.Expose( m_fields );
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
		
		EditorGUI.BeginChangeCheck();
		
		// delete the mesh collider if any because this may interfere with the collision detection of the tile
		// in case of the pawn collide with this mesh collider before colliding with the mesh collider of the tile
		Collider[] colliders =  m_Instance.gameObject.GetComponents<Collider>();
		foreach (Collider collider in colliders)
			Component.DestroyImmediate(collider);

		// delete also the mesh filter and mesh renderer
		MeshRenderer[] renderers =  m_Instance.gameObject.GetComponents<MeshRenderer>();
		foreach (MeshRenderer renderer in renderers)
			Component.DestroyImmediate(renderer);

		MeshFilter[] filters =  m_Instance.gameObject.GetComponents<MeshFilter>();
		foreach (MeshFilter filter in filters)
			Component.DestroyImmediate(filter);

		// update the tile mesh (if type or glue flags are changed) unless it is the prefab
		if (!isEditingThePrefab)
			m_Instance.updateTileMesh(false);

		EditorGUI.EndChangeCheck();

		//serializedObject.ApplyModifiedProperties ();
	}
}