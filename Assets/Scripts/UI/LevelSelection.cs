using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelection : MonoBehaviour 
{
	public void ChangeScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}
