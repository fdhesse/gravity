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

	// the singleton for the HUD
	private static HUD s_Instance = null;
	public static HUD Instance { get { return s_Instance; } }

	// for hud
	private UnityEngine.UI.Text mGravityCounterText = null;
	private int mGravityChangeCount = 0;// times that the gravity has been changed

	// fadeout screen
	public Texture fadeinoutTexture;
	public float fadeSpeed = 1.5f;              // Speed that the screen fades to and from black.
	private float alphaFadeValue;
	private bool isFading; // fading state

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

	public void IncreaseGravityChangeCount()
	{
		GravityChangeCount = GravityChangeCount + 1;
	}

	void Awake()
	{
		s_Instance = this;

		// hud
		mGravityCounterText = this.transform.Find("HUD/CounterText").GetComponent<UnityEngine.UI.Text>();
		this.GravityChangeCount = 0;

		// result
		mResultPage = this.transform.Find("ResultPage").gameObject;
		mStar1 = mResultPage.transform.Find("Stars/Star1").GetComponent<UnityEngine.UI.Button>();
		mStar2 = mResultPage.transform.Find("Stars/Star2").GetComponent<UnityEngine.UI.Button>();
		mStar3 = mResultPage.transform.Find("Stars/Star3").GetComponent<UnityEngine.UI.Button>();
		mMinimumGravityChangeText = mResultPage.transform.Find("MinimumChange").GetComponent<UnityEngine.UI.Text>();
		mCurrentGravityChangeText = mResultPage.transform.Find("GravityChange").GetComponent<UnityEngine.UI.Text>();
			
		// pause
		mPausePage = this.transform.Find("PauseMenu").gameObject;

		// tutorial
		mTutorialPage = this.transform.Find("TutorialPage").gameObject;
		mTutorialText = mTutorialPage.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
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
		// #FADEINOUT_TEXTURE#
		if (fadeinoutTexture != null)
		{
			if (isFading)
			{
				alphaFadeValue += Mathf.Clamp01(Time.deltaTime / 1);

				GUI.color = new Color(0, 0, 0, alphaFadeValue);
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeinoutTexture);

				if (alphaFadeValue > 1)
					isFading = false;

			}
			else if (alphaFadeValue > 0)
			{
				alphaFadeValue -= Mathf.Clamp01(Time.deltaTime / 1);

				GUI.color = new Color(0, 0, 0, alphaFadeValue);
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeinoutTexture);

				if (World.Instance.IsGameOver()) //is the game over? 
					World.Instance.GameStart();
			}
		}
	}

	#region fade in / fade out
	public void StartFadeOut()
	{
		isFading = true;
	}
	#endregion

	#region HUD
	public void onPauseButtonPressed()
	{
		showPauseMenu(true);
	}
	#endregion

	#region Result Page
	public void ShowResultPage()
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
