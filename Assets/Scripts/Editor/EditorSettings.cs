#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Test tools editor window, providing bunch of function to tweak game's behaviours
/// </summary>
[InitializeOnLoad]
public class EditorSettings : EditorWindow, UnityEditor.Build.IPreprocessBuild, UnityEditor.Build.IPostprocessBuild
{
	#region function implementation
	private static bool s_IsEditorBuilding = false;

	public int callbackOrder
	{
		get { return 0; }
	}

	public void OnPreprocessBuild(BuildTarget target, string path)
	{
		s_IsEditorBuilding = true;
	}

	public void OnPostprocessBuild(BuildTarget target, string path)
	{
		s_IsEditorBuilding = false;
	}
	#endregion

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
		DrawBlockGrid();
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
			EnableDisableHUD(EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || s_IsEditorBuilding);
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

	#region block grid
	private bool m_IsBlockGridGeneratorOpen = true;
	private Vector3 m_GridSize = new Vector3(5f, 1f, 5f);
	private GameObject m_GridCubePrefab = null;

	private void DrawBlockGrid()
	{
		GUILayout.Space(5);
		m_IsBlockGridGeneratorOpen = EditorGUILayout.Foldout(m_IsBlockGridGeneratorOpen, "Grid Generator");
		if (m_IsBlockGridGeneratorOpen)
		{
			// get the prefab for the cube
			m_GridCubePrefab = EditorGUILayout.ObjectField("Cube Prefab", m_GridCubePrefab, typeof(GameObject), false) as GameObject;
			if (m_GridCubePrefab == null)
				EditorGUILayout.HelpBox("Please provide a valid prefab for the generation of the grid.", MessageType.Warning);
			
			// the grid size
			m_GridSize = EditorGUILayout.Vector3Field("Grid Size", m_GridSize);
			if ((m_GridSize.x == 0) || (m_GridSize.y == 0) || (m_GridSize.z == 0))
				EditorGUILayout.HelpBox("One of the grid size is null, nothing will be generated.", MessageType.Warning);

			// check if the parent of the grid is selected
			bool isGridParentValid = Selection.gameObjects.Length > 0;
			if (!isGridParentValid)
				EditorGUILayout.HelpBox("Select one parent for the grid (only one), or the generate button won't work.", MessageType.Warning);

			// and the generate button
			if (GUILayout.Button("Generate block grid") && isGridParentValid)
				GenerateBlocksGrid();
		}
	}

	/// <summary>
	/// Instantiates the blocks that make up the grid
	/// </summary>
	private void GenerateBlocksGrid()
	{
		const int BLOCK_SIZE = 10;

		// if no object is selected to become the parent, early exit
		if (Selection.gameObjects.Length == 0)
			return;
		Transform gridParent = Selection.gameObjects[0].transform;

		for (int i = 0; i != m_GridSize.x; i++)
		{
			for (int j = 0; j != m_GridSize.z; j++)
			{
				for (int k = 0; k != m_GridSize.y; k++)
				{
					Vector3 blockPosition = new Vector3(i * BLOCK_SIZE, k * BLOCK_SIZE, j * BLOCK_SIZE);
					GameObject tempBlock = Instantiate(m_GridCubePrefab, blockPosition, Quaternion.identity, gridParent) as GameObject;
					tempBlock.name = tempBlock.name + "(" + i + ", " + k + ", " + j + ")";
				}
			}
		}
	}
	#endregion
}

#endif
