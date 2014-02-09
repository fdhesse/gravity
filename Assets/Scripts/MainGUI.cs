using UnityEngine;
using System.Collections;

/// <summary>
/// GUI monobehaviour class. Drop it into a gameobject and violá.
/// Makes the menus and buttons.
/// </summary>
public class MainGUI : MonoBehaviour
{

    //custom GUI skin to specify the "looks" of the GUI
    public GUISkin skin;

    //size of the level "window"
    static private int width = 800;
    static private int height = 400;

    //Rectangles with positioning of the 2 GUI elements in the 
    private Rect titleRect = new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 4 - 100, width, 80);
    private Rect windowRect = new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 4, width, height);

    private string[] buttonStrings; // name of all levels
    private int nLevels = 20;
    private int nRows = 4; //number of rows we want in the level list
    private int nCols;

    /// <summary>
    /// Class initializations.
    /// Names all buttons.
    /// </summary>
    void Start()
    {
        //populate the array holding the names of the levels
        // in this case each level will be called "Level " suffixed with the number of the level, e.g. "Level 13"
        buttonStrings = new string[nLevels];
        for (int i = 0; i != nLevels; i++)
        {
            buttonStrings[i] = "Level " + i;
        }

        nCols = nLevels / nRows; // assigning the correct number of rows
    }


    /// <summary>
    /// Draws GUI Elements on screen.
    /// Uses GUILayouts to organize content.
    /// </summary>
    void OnGUI()
    {
        GUI.Label(titleRect, "Select a Level", skin.GetStyle("title")); // create a label

        // Here I use GUILayouts to distribute the buttons in a table like fashion
        GUILayout.BeginArea(windowRect);
        GUILayout.BeginVertical("box");
        for (int i = 0; i != nRows; i++)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j != nCols; j++)
            {
                int lvl = (i * nCols + j + 1);
                string text = lvl < 10 ? "Level  " + lvl : "Level " + lvl;
                if (GUILayout.Button(text))
                {
                    //foreach button load the correct scene
                    Application.LoadLevel("level" + lvl);
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}