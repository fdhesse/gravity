#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Test tools editor window, providing bunch of function to tweak game's behaviours
/// </summary>
[InitializeOnLoad]
public class EditorSettings : EditorWindow
{
	#region main functions
	static EditorSettings()
	{
		// add the play mode change callback
		EditorApplication.playmodeStateChanged += OnPlayModeChanged;
		EditorSceneManager.sceneOpened += OnSceneOpened;
		EditorSceneManager.sceneSaving += OnSceneSaving;
		EditorSceneManager.sceneSaved += OnSceneSaved;
	}

	[MenuItem("Mu/Editor Settings")]
	public static void OpenDebugWindow()
	{
		EditorWindow.GetWindow<EditorSettings>().Show();
	}

	protected virtual void OnGUI()
	{
		DrawHideHUD();
	}
	#endregion

	#region PlayerPrefs
	private static bool m_IsHUDHidden = true;

	private void DrawHideHUD()
	{
		bool previousState = m_IsHUDHidden;
		m_IsHUDHidden = EditorGUILayout.Toggle("Hide HUD during edition", m_IsHUDHidden);
		if (previousState != m_IsHUDHidden)
		{
			// if we uncheck the checkbox, we need to reenable
			if (m_IsHUDHidden)
				EnableDisableHUDIfNeeded();
			else
				EnableDisableHUD(true);
		}
	}

	private static void EnableDisableHUD(bool isEnabled)
	{
		for (int i = 0; i < SceneManager.sceneCount; ++i)
		{
			// iterate through all open scenes
			var currentScene = SceneManager.GetSceneAt(i);
			if (currentScene.isLoaded)
			{
				GameObject[] rootObjects = currentScene.GetRootGameObjects();
				foreach (GameObject rootObj in rootObjects)
				{
					// try to get the hud in the scene
					if (rootObj.CompareTag("HUD"))
						rootObj.SetActive(isEnabled);

					// try to get the camera in the scene
					if (rootObj.CompareTag("MainCamera"))
					{
						var camCanvas = rootObj.GetComponentInChildren<Canvas>(true);
						if (camCanvas != null)
							camCanvas.gameObject.SetActive(isEnabled);
					}
				}
			}
		}
	}

	private static void EnableDisableHUDIfNeeded()
	{
		// if the checkbox is not check, do not change the value of the HUD
		if (m_IsHUDHidden)
			EnableDisableHUD(EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode);
	}

	private static void OnPlayModeChanged()
	{
		EnableDisableHUDIfNeeded();
	}

	private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
	{
		// always reenable the HUD before saving
		EnableDisableHUDIfNeeded();
	}

	private static void OnSceneSaving(Scene scene, string path)
	{
		// always reenable the HUD before saving (if the checkbox is set), otherwise leave it to what it is
		if (m_IsHUDHidden)
			EnableDisableHUD(true);
	}

	private static void OnSceneSaved(Scene scene)
	{
		EnableDisableHUDIfNeeded();
	}
	#endregion
}

#endif
