using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

	[Tooltip("The change gravity counts for having two or three stars")]
	public Stars stars;

	// for hud
	private UnityEngine.UI.Text mGravityCounterText = null;
	private int mGravityChangeCount = 0;// times that the gravity has been changed

	// for result page
	private GameObject mResultPage = null;
	private UnityEngine.UI.Button mStar1 = null;
	private UnityEngine.UI.Button mStar2 = null;
	private UnityEngine.UI.Button mStar3 = null;
	private UnityEngine.UI.Text mMinimumGravityChangeText = null;
	private UnityEngine.UI.Text mCurrentGravityChangeText = null;

	// for pause page
	private bool mIsPaused = false; // is the game pause screen up ?
	private GameObject mPausePage = null;

	// for tutorial pages
	private GameObject mTutorialPage = null;
	private UnityEngine.UI.Text mTutorialText = null;
	private string[] textPages;
	private int pageId;

	// fps related
	public bool drawFPS = true;
	private float fps = 0.0f;
	private float lastSampledTimeForFPS = 0.0f;

	// Camera reference
	private CameraControl cameraControl = null;

	public bool IsPaused
	{
		get { return mIsPaused; }
	}

	public int GravityChangeCount
	{
		get { return mGravityChangeCount; }
		set
		{
			mGravityChangeCount = value;
			mGravityCounterText.text = value.ToString();
		}
	}

	void Awake()
	{
		// hud
		mGravityCounterText = this.transform.FindChild("HUD/CounterText").GetComponent<UnityEngine.UI.Text>();
		this.GravityChangeCount = 0;

		// result
		mResultPage = this.transform.FindChild("ResultPage").gameObject;
		mStar1 = mResultPage.transform.FindChild("Stars/Star1").GetComponent<UnityEngine.UI.Button>();
		mStar2 = mResultPage.transform.FindChild("Stars/Star2").GetComponent<UnityEngine.UI.Button>();
		mStar3 = mResultPage.transform.FindChild("Stars/Star3").GetComponent<UnityEngine.UI.Button>();
		mMinimumGravityChangeText = mResultPage.transform.FindChild("MinimumChange").GetComponent<UnityEngine.UI.Text>();
		mCurrentGravityChangeText = mResultPage.transform.FindChild("GravityChange").GetComponent<UnityEngine.UI.Text>();
			
		// pause
		mPausePage = this.transform.FindChild("PauseMenu").gameObject;

		// tutorial
		mTutorialPage = this.transform.FindChild("TutorialPage").gameObject;
		mTutorialText = mTutorialPage.transform.FindChild("Text").GetComponent<UnityEngine.UI.Text>();
	}

    // Use this for initialization
    void Start()
	{
		cameraControl = Camera.main.GetComponent<CameraControl>();
		
		// activate of deactivate the HUD pages
		showResultPage(false);
		showPauseMenu(false);
		showNarrativePage(false);
    }
	
	// Update is called once per frame
	void Update()
	{
		if (drawFPS)
		{
			// compute the FPS
			const int nbFrameForAverage = 100;
			if ((Time.frameCount % nbFrameForAverage) == 0)
			{
				float averageDeltaTime = (Time.time - lastSampledTimeForFPS) / (float)nbFrameForAverage;
				fps = 1.0f / averageDeltaTime;
				lastSampledTimeForFPS = Time.time;
			}
		}
	}

	private void freezeGameTime(bool shouldPause)
	{
		if (shouldPause)
		{
			Time.timeScale = 0;
			if (cameraControl != null)
				cameraControl.enabled = false;
		}
		else
		{
			Time.timeScale = 1;
			if (cameraControl != null)
				cameraControl.enabled = true;
		}
	}

    // Update is called once per frame
    void OnGUI()
    {
		// debug print the fps
		if (drawFPS)
		{
			GUI.color = Color.black;
			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.UpperRight;
			GUI.Label(new Rect(10, 5, 100, 30), fps.ToString("f1"), style);
		}
    }

	#region HUD
	public void onPauseButtonPressed()
	{
		showPauseMenu(true);
	}
	#endregion

	#region Result Page
	public void showResultPage()
	{
		this.showResultPage(true);
	}

	private void showResultPage(bool shouldShow)
	{
		// freeze the game if we show the page
		this.freezeGameTime(shouldShow);

		// set the state of the 3 stars
		if ( mGravityChangeCount <= stars.three )
		{
			// Three gold stars
			mStar1.interactable = true;
			mStar2.interactable = true;
			mStar3.interactable = true;
		}
		else
		{
			if ( mGravityChangeCount <= stars.two )
			{
				// Two gold stars
				mStar1.interactable = true;
				mStar2.interactable = true;
				mStar3.interactable = false;
			}
			else
			{
				// One gold star
				mStar1.interactable = true;
				mStar2.interactable = false;
				mStar3.interactable = false;
			}
		}

		mMinimumGravityChangeText.text = "Minimum changes: " + stars.three.ToString();
		mCurrentGravityChangeText.text = "Magnetic changes: " + mGravityChangeCount.ToString();

		// then display or hide the narrative page
		mResultPage.SetActive(shouldShow);
	}

	public void onResultPageRestart()
	{
		this.freezeGameTime(false);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void onResultPageExit()
	{
		this.freezeGameTime(false);
		SceneManager.LoadScene("main");
	}

	public void onResultPageNextLevel()
	{
		this.freezeGameTime(false);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}
	#endregion

	#region Pause Page
	private void showPauseMenu(bool shouldShow)
	{
		// set  the pause flag
		this.mIsPaused = shouldShow;

		// freeze the game if we show the page
		this.freezeGameTime(shouldShow);

		// then display or hide the narrative page
		mPausePage.SetActive(shouldShow);
	}

	public void onPauseMenuRestart()
	{
		this.freezeGameTime(false);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void onPauseMenuResume()
	{
		this.freezeGameTime(false);
		showPauseMenu(false);
	}

	public void onPauseMenuExit()
	{
		this.freezeGameTime(false);
		SceneManager.LoadScene("main");
	}
	#endregion

	#region Narrative Page
	private void showNarrativePage(bool shouldShow)
	{
		// freeze the game if we show the page
		this.freezeGameTime(shouldShow);

		// then display or hide the narrative page
		mTutorialPage.SetActive(shouldShow);
	}

	public void onNarrativeTextButtonNext()
	{
		if (pageId < ( textPages.Length - 1 ) )
		{
			pageId++;
			mTutorialText.text = textPages[ pageId ];
		}
		else
		{
			showNarrativePage(false);
		}
	}

	public void DisplayNarrativeText( string[] pages )
	{
		// display the narrative page if there's something to display
		if ( pages.Length > 0 )
		{
			// memorise the pages and reset the page id
			pageId = 0;
			textPages = pages;
			mTutorialText.text = textPages[0];
			// display the narrative page
			showNarrativePage(true);
		}
	}

	#endregion
}
