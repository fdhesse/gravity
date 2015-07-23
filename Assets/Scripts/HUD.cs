using UnityEngine;
using System.Collections;

/// <summary>
/// Class responsible for the HUD
/// </summary>
public class HUD : MonoBehaviour
{
	[System.Serializable]
	public struct Stars
	{
		public int three;
		public int two;
	}

    private GUISkin skin;// skin we are using, should be assigned via editor

    public float ClickDelay = 0.1f;
	public int minGravityChange = 0;// minimum gravity change for this level
	public Stars stars;
    public int gravityChangeCount = 0;// times that the gravity has been changed

	public bool isTextScreen = false; // is a text screen up ?
    public bool isEndScreen = false; // is the end screen up ?
    public bool isPaused = false; // is the game pause screen up ?

    public Color dotColor = Color.cyan;
    public float dotSize = 2.0f;
    public bool dotIsInside = true;

    //values for positioning
    static private int resultWindowWidth = 800;
    static private int resultWindowHeight = 400;
    static private int pauseWindowWidth = 400;
	static private int pauseWindowHeight = 200;
	static private int textWindowWidth = 400;
	static private int textWindowHeight = 200;
	private Rect pauseWindowRect = new Rect(Screen.width *.5f - pauseWindowWidth *.5f, Screen.height *.5f - pauseWindowHeight *.25f, pauseWindowWidth, pauseWindowHeight);
	private Rect textWindowRect = new Rect(Screen.width *.5f - textWindowWidth *.5f, Screen.height *.5f - textWindowWidth *.25f, textWindowWidth, textWindowHeight);
	private Rect resultWindowRect = new Rect(Screen.width *.5f - resultWindowWidth *.5f, Screen.height *.5f - resultWindowHeight *.25f, resultWindowWidth, resultWindowHeight);
    private Rect pauseRect = new Rect(10, 10, 50, 50);

	private string[] textPages;
	private int pageId;

	// Camera reference
	private CameraControl cameraControl;
	
	private Texture greyStar;
	private Texture goldStar;

    // Use this for initialization
    void Start()
	{
        skin = Resources.Load("HUD/skin") as GUISkin;
		cameraControl = Camera.main.GetComponent<CameraControl> ();
		
		goldStar = Resources.Load ("HUD/goldstar") as Texture;
		greyStar = Resources.Load ("HUD/greyStar") as Texture;
    }

    // Update is called once per frame
    void OnGUI()
    {
		if (isEndScreen || isPaused || isTextScreen)
        {
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),GUIContent.none,skin.GetStyle("overlay"));
			Time.timeScale = 0;
			//cameraControl.enabled = false;
		}
		else
		{
			Time.timeScale = 1;
			//cameraControl.enabled = true;
		}

        if (isEndScreen)
        {
            resultWindowRect = GUI.Window(0, resultWindowRect, resultWindow, "Result", skin.GetStyle("window"));
        }
        else
        {
            GUIStyle buttonStyle = isPaused ? skin.GetStyle("pauseOn") : skin.GetStyle("pauseOff");
            if (GUI.Button(pauseRect, "", buttonStyle))
			{
                isPaused = !isPaused;
            }
            GUI.Label(new Rect(Screen.width - 80, 10, 70, 50), gravityChangeCount + "", skin.GetStyle("gravityCounter"));
        }

		if (isTextScreen)
		{
			textWindowRect = GUI.Window(0, textWindowRect, textWindow, "Text", skin.GetStyle("window"));
		}

        if (isPaused)
        {
            pauseWindowRect = GUI.Window(0, pauseWindowRect, pauseWindow, "Pause", skin.GetStyle("window"));
        }
    }


    void resultWindow(int windowID)
    {

        if (isEndScreen)
		{
			if ( gravityChangeCount <= stars.three )
			{
					// Three gold stars
					GUI.DrawTexture(new Rect(275, 50, 50, 50), goldStar);
					GUI.DrawTexture(new Rect(375, 50, 50, 50), goldStar);
					GUI.DrawTexture(new Rect(475, 50, 50, 50), goldStar);
			}
			else
			{
				if ( gravityChangeCount <= stars.two )
				{
					// Two gold stars
					GUI.DrawTexture(new Rect(275, 50, 50, 50), goldStar);
					GUI.DrawTexture(new Rect(375, 50, 50, 50), goldStar);
					GUI.DrawTexture(new Rect(475, 50, 50, 50), greyStar);
				}
				else
				{
					// One gold star
					GUI.DrawTexture(new Rect(275, 50, 50, 50), goldStar);
					GUI.DrawTexture(new Rect(375, 50, 50, 50), greyStar);
					GUI.DrawTexture(new Rect(475, 50, 50, 50), greyStar);
				}
			}

            GUI.Label(new Rect(100, 150, 150, 50), "Minimum changes: " + minGravityChange, skin.GetStyle("centeredLabel"));
			GUI.Label(new Rect( resultWindowWidth - 300, 150, 200, 50), "Gravity changes: " + gravityChangeCount, skin.GetStyle("centeredLabel"));

            if (GUI.Button(new Rect(280, 300, 100, 20), "Restart"))
            {
                Application.LoadLevel(Application.loadedLevel);
            }

            if (GUI.Button(new Rect(420, 300, 100, 20), "Exit"))
			{
				Application.LoadLevel("main");
            }
        }

    }

    void pauseWindow(int windowID)
    {
        if (isPaused)
		{
			
			if (GUI.Button(new Rect(25, 100, 100, 20), "Restart"))
			{
				Application.LoadLevel(Application.loadedLevel);
				Time.timeScale = 1;
			}

            if (GUI.Button(new Rect(150, 100, 100, 20), "Resume"))
            {
                isPaused = false;
            }

            if (GUI.Button(new Rect(275, 100, 100, 20), "Exit"))
            {
                Application.LoadLevel("main");
            }
        }

	}
	
	void textWindow(int windowID)
	{
		if (isTextScreen)
		{
			GUI.Label(new Rect( 0, 0, 250, 100), textPages[ pageId ], skin.GetStyle("gravityCounter"));
			
			if ( pageId < ( textPages.Length - 1 ) )
			{
				if (GUI.Button(new Rect(275, 100, 100, 20), "Next"))
				{
					pageId++;
				}
			}
			if ( pageId > 0 )
			{
				if (GUI.Button(new Rect(275, 130, 100, 20), "Previous"))
				{
					pageId--;
				}
			}

			GUIStyle buttonStyle = skin.GetStyle("close");

			if (GUI.Button(new Rect(375, 5, 20, 20), "", buttonStyle))
			{
				isTextScreen = false;
			}
		}
		
	}

	public void DisplayNarrativeText( string[] pages )
	{
		pageId = 0;
		textPages = pages;

		if ( textPages.Length > 0 )
			isTextScreen = true;
	}
}
