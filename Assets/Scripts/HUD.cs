using UnityEngine;
using System.Collections;

/// <summary>
/// Class responsible for the HUD
/// </summary>
public class HUD : MonoBehaviour
{

    private GUISkin skin;// skin we are using, should be assigned via editor

    public float ClickDelay = 0.1f;
    public int minGravityChange = 0;// minimum gravity change for this level
    public int gravityChangeCount = 0;// times that the gravity has been changed

    public bool isEndScreen = false;// is the end screen up?
    public bool isPaused = false;// is the game pause screen up?

    public Color dotColor = Color.cyan;
    public float dotSize = 2.0f;
    public bool dotIsInside = true;

    //values for positioning
    static private int resultWindowWidth = 800;
    static private int resultWindowHeight = 400;
    static private int pauseWindowWidth = 400;
    static private int pauseWindowHeight = 200;
    private Rect pauseWindowRect = new Rect(Screen.width / 2 - pauseWindowWidth / 2, Screen.height / 2 - pauseWindowHeight / 4, pauseWindowWidth, pauseWindowHeight);
    private Rect resultWindowRect = new Rect(Screen.width / 2 - resultWindowWidth / 2, Screen.height / 2 - resultWindowHeight / 4, resultWindowWidth, resultWindowHeight);
    private Rect pauseRect = new Rect(10, 10, 50, 50);

    // Use this for initialization
    void Start()
    {
        skin = Resources.Load("skin") as GUISkin;
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (isEndScreen || isPaused)
        {
            GUI.Box(new Rect(0,0,Screen.width,Screen.height),GUIContent.none,skin.GetStyle("overlay"));
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

        if (isPaused)
        {
            pauseWindowRect = GUI.Window(0, pauseWindowRect, pauseWindow, "Pause", skin.GetStyle("window"));
        }
    }


    void resultWindow(int windowID)
    {

        if (isEndScreen)
        {
            //GUI.Label(new Rect(50, 50, width-100, 50), "The minimum amount of gravity changes to complete this level is " + minGravityChange, skin.GetStyle("centeredLabel"));
            GUI.Label(new Rect(50, 50, resultWindowWidth - 100, 50), "Gravity changes necessary: " + minGravityChange, skin.GetStyle("centeredLabel"));
            GUI.Label(new Rect(50, 150, resultWindowWidth - 100, 50), "Times you changed gravity: " + gravityChangeCount, skin.GetStyle("centeredLabel"));

            if (GUI.Button(new Rect(200, 300, 100, 20), "Replay"))
            {
                Application.LoadLevel(Application.loadedLevel);
            }

            if (GUI.Button(new Rect(350, 300, 100, 20), "Change level"))
            {
                Application.LoadLevel("main");
            }

            if (GUI.Button(new Rect(500, 300, 100, 20), "Exit"))
            {
                Application.Quit();
            }
        }

    }

    void pauseWindow(int windowID)
    {

        if (isPaused)
        {

            if (GUI.Button(new Rect(25, 100, 100, 20), "Resume"))
            {
                isPaused = false;
            }

            if (GUI.Button(new Rect(150, 100, 100, 20), "Restart"))
            {
                Application.LoadLevel(Application.loadedLevel);
            }

            if (GUI.Button(new Rect(275, 100, 100, 20), "Change level"))
            {
                Application.LoadLevel("main");
            }
        }

    }


}
