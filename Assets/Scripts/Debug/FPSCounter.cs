using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// From http://wiki.unity3d.com/index.php?title=FramesPerSecond
/// Updated for Unity 4.6
/// Modified by Alban
/// </summary>
public class FPSCounter : MonoBehaviour
{
	#pragma warning disable
	[Tooltip("Format string suitable for System.String.Format()")]
	[SerializeField]
	private string m_DisplayFormat = "{0:F2}";

	[Tooltip("Format string suitable for System.String.Format()")]
	[SerializeField]
	private int m_FontSize = 20;

	[Tooltip("Position where the FPS will be drawn")]
	[SerializeField]
	private Rect m_FPSPosition = new Rect(10, 10, 100, 20);

	[Tooltip("Interval in second for which the FPS will be updated")]
	[SerializeField]
	private float m_UpdateIntervalTime = 0.5f;

	private Text m_TextWidget = null;

	private float m_AccumulatedTime = 0f;
	private int   m_FrameCount = 0; // Frames drawn over the interval
	private float m_TimeLeft = 0f; // Left time for current interval
	private float m_Fps = 0f;

    private static bool m_ShouldDisableTheFPS = false;
	#pragma warning restore

	public static bool ShouldDisableTheFPS
    {
        get { return m_ShouldDisableTheFPS; } 
        set { m_ShouldDisableTheFPS = value; }
    }

	#if FPS
    void Start()
	{
		createTextWidget();
		m_TimeLeft = m_UpdateIntervalTime;
	}

	private void createTextWidget()
	{
		GameObject newText = new GameObject("FPSCounter", typeof(RectTransform));
		newText.transform.SetParent(transform, false);
		newText.layer = LayerMask.NameToLayer("UI");
		m_TextWidget = newText.AddComponent<Text>();
		m_TextWidget.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		m_TextWidget.fontSize = m_FontSize;
		m_TextWidget.horizontalOverflow = HorizontalWrapMode.Overflow;
		m_TextWidget.verticalOverflow = VerticalWrapMode.Overflow;
		// set the position
		(m_TextWidget.transform as RectTransform).anchorMin = new Vector2(0, 1);
		(m_TextWidget.transform as RectTransform).anchorMax = new Vector2(0, 1);
		(m_TextWidget.transform as RectTransform).pivot = new Vector2(0, 1);
		(m_TextWidget.transform as RectTransform).anchoredPosition3D = new Vector3(m_FPSPosition.x, -m_FPSPosition.y, 0);
	}

	void Update()
	{
		m_TimeLeft -= Time.deltaTime;
		m_AccumulatedTime += Time.timeScale/Time.deltaTime;
		++m_FrameCount;
		
		// Interval ended - update GUI text and start new interval
		if( m_TimeLeft <= 0.0 )
		{
			m_Fps = m_AccumulatedTime / m_FrameCount;
			m_TimeLeft = m_UpdateIntervalTime;
			m_AccumulatedTime = 0.0f;
			m_FrameCount = 0;
			// update the text widget
			UpdateTextWidgetContent();
		}
	}

	void UpdateTextWidgetContent()
	{
		if (!m_ShouldDisableTheFPS)
		{
			if (m_Fps < 10)
				m_TextWidget.color = Color.red;
			else if (m_Fps < 29)
				m_TextWidget.color = Color.yellow;
			else
				m_TextWidget.color = Color.green;

			m_TextWidget.text = System.String.Format(m_DisplayFormat, m_Fps);
		}
		else
		{
			m_TextWidget.text = "";
		}
	}
	#endif
}
