using UnityEngine;
using System.Collections;

public class LevelSelection : MonoBehaviour 
{
	public void ChangeScene(string sceneName)
	{
		Application.LoadLevel(sceneName);
	}
}
