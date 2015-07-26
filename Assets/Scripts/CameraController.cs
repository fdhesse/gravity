using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	#region General Variables
	public static CameraController Instance;

	public enum RotationDirections {None, Left, Right, Up, Down};
	public RotationDirections RotationDirection;

	public int rotationDirection = -1;
	public int rotationStep = 10;

	private Vector3 currentRotation;
	private Vector3 targetRotation;

	public GameObject theCameraObject;
	public Transform theCameraTragetPosition;

	public int rotationInterval = 90;

	Quaternion tempRotation;
	#endregion

	
	#region Keyboard related Variables
	public int x = 0;
	public int y = 0;
	#endregion


	#region Swipe related variables
	private Vector2 mX = new Vector2(1.0f, 0.0f);
	private Vector2 mY = new Vector2(0.0f, 1.0f);
	private const float angle = 35;
	private const float minSwipeDist = 50.0f;
	private const float mininVelocity  = 2000.0f;
	private float swipeStart;
	private Vector2 swipeStartPosition;

	private const float STRAIGHT_LINE_Angle = 180.0f;
	#endregion


	#region Unity Functions
	void Awake () {
		Instance = this;
	}

	void Start ()
	{
		theCameraObject.transform.position = theCameraTragetPosition.position;
		theCameraObject.transform.rotation = theCameraTragetPosition.rotation;
	}

	void Update()
	{
		#region Input Keyboard
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			RotationDirection = RotationDirections.Right;
			rotateTheCameraAround();
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			RotationDirection = RotationDirections.Left;
			rotateTheCameraAround();
		}

		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			RotationDirection = RotationDirections.Up;
			rotateTheCameraAround();
		}

		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			RotationDirection = RotationDirections.Down;
			rotateTheCameraAround();
		}
		#endregion

		#region Input Mouse and Touch
		if (Input.GetMouseButtonDown(0))
		{
			swipeStartPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			swipeStart = Time.time;
		}
		
		if (Input.GetMouseButtonUp(0)) {
			float deltaTime = Time.time - swipeStart;
			Vector2 endPosition  = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
			Vector2 swipeVector = endPosition - swipeStartPosition;
			float velocity = swipeVector.magnitude/deltaTime;
			
			if (velocity > mininVelocity && swipeVector.magnitude > minSwipeDist)
			{
				swipeVector.Normalize();
				float angleOfSwipe = Vector2.Dot(swipeVector, mX);
				angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
				
				if (angleOfSwipe < angle)
				{
					RotationDirection = RotationDirections.Left;
					rotateTheCameraAround();
				}
				else if((STRAIGHT_LINE_Angle - angleOfSwipe) < angle)
				{
					RotationDirection = RotationDirections.Right;
					rotateTheCameraAround();
				}
				else
				{
					angleOfSwipe = Vector2.Dot(swipeVector, mY);
					angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;

					if (angleOfSwipe < angle)
					{
						RotationDirection = RotationDirections.Down;
						rotateTheCameraAround();
					}else if((STRAIGHT_LINE_Angle - angleOfSwipe) < angle)
					{
						RotationDirection = RotationDirections.Up;
						rotateTheCameraAround();
					}
					else
					{
						//nothing
					}
				}
			}
		}
		#endregion
	}
	#endregion

	#region Excution Functions
	private void rotateTheCameraAround()
	{
		switch (RotationDirection)
		{
		case RotationDirections.None:
			//nothing
			break;
		case RotationDirections.Right:
			currentRotation = gameObject.transform.localEulerAngles;
			rotationDirection = 1;
			targetRotation.y = (currentRotation.y + (rotationInterval * -rotationDirection));
			y-= rotationInterval;
			StartCoroutine (objectRotationProcess());
			break;
		case RotationDirections.Left:
			currentRotation = gameObject.transform.localEulerAngles;
			rotationDirection = -1;
			targetRotation.y = (currentRotation.y - (rotationInterval * rotationDirection));
			y+= rotationInterval;
			StartCoroutine (objectRotationProcess());
			break;
		case RotationDirections.Up:
			currentRotation = gameObject.transform.localEulerAngles;
			rotationDirection = 1;
			targetRotation.x = (currentRotation.x + (rotationInterval * -rotationDirection));
			x+= rotationInterval;
			StartCoroutine (objectRotationProcess());
			break;
		case RotationDirections.Down:
			currentRotation = gameObject.transform.localEulerAngles;
			rotationDirection = -1;
			targetRotation.x = (currentRotation.x - (rotationInterval * rotationDirection));
			x-= rotationInterval;
			StartCoroutine (objectRotationProcess());
			break;
		}

		tempRotation = transform.rotation;
		tempRotation.z = 0;
		transform.rotation = tempRotation;
	}


	IEnumerator objectRotationProcess()
	{
		switch (RotationDirection)
		{
		case RotationDirections.None:
			break;
		case RotationDirections.Right:
			currentRotation.y += (rotationStep * rotationDirection);
			gameObject.transform.localEulerAngles = currentRotation;
			yield return new WaitForSeconds (0);
			gameObject.transform.rotation = Quaternion.Euler(0,y,0);
			break;
		case RotationDirections.Left:
			currentRotation.y -= (rotationStep * rotationDirection);
			gameObject.transform.localEulerAngles = currentRotation;
			yield return new WaitForSeconds (0);
			gameObject.transform.rotation = Quaternion.Euler(0,y,0);
			break;
		case RotationDirections.Up:
			currentRotation.x += (rotationStep * rotationDirection);
			gameObject.transform.localEulerAngles = currentRotation;
			yield return new WaitForSeconds (0);
			gameObject.transform.rotation = Quaternion.Euler(x,0,0);
			break;
		case RotationDirections.Down:
			currentRotation.x -= (rotationStep * rotationDirection);
			gameObject.transform.localEulerAngles = currentRotation;
			yield return new WaitForSeconds (0);
			gameObject.transform.rotation = Quaternion.Euler(x,0,0);
			break;
		}
	}
	#endregion
}