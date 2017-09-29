#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

/// <summary>
/// Test tools editor window, providing bunch of function to tweak game's behaviours
/// </summary>
public class DebugWindow : EditorWindow
{
	#region main functions
	[MenuItem("Mu/Debug Window")]
	public static void OpenDebugWindow()
	{
		EditorWindow.GetWindow<DebugWindow>().Show();
	}

	protected virtual void OnGUI()
	{
		DrawTimeScale();
		DrawSelectMCButton();
		DrawResetPlayerPrefs();
    }
	#endregion

	#region PlayerPrefs
	private void DrawResetPlayerPrefs()
    {
		if (GUILayout.Button("Delete All PlayerPrefs"))
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
		}
	}
    #endregion

	#region time scale
	private void DrawTimeScale()
	{
		Time.timeScale = EditorGUILayout.Slider("Time Scale", Time.timeScale, 0, 1f);
	}
	#endregion

	#region Pawn
	private void DrawSelectMCButton()
	{
		if (GUILayout.Button("Select Pawn"))
		{
			GameObject selectedObject = null;
			Pawn pawn = FindObjectOfType<Pawn>();
			if (pawn != null)
				selectedObject = pawn.GetComponentInChildren<Animator>().gameObject;

			if (selectedObject != null)
				UnityEditor.Selection.objects = new Object[] { selectedObject };
		}
	}
    #endregion
}

#endif