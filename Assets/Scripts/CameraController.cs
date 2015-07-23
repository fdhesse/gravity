using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public static CameraController Instance;
	
	// Use this for initialization
	void Awake () {
		Instance = this;
	}
	
	public int rotationDirection = -1; // -1 for clockwise
	// 1 for anti-clockwise
	public int rotationStep = 10; // should be less than 90
	// All the objects with which collision will be checked
	public GameObject[] objectsArray;
	private Vector3 currentRotation, targetRotation;

	public GameObject theCameraObject;
	public Transform theCameraTragetPosition;

	public enum RotationDirections {None, Left, Right, Up, Down};
	public RotationDirections RotationDirection;

	public int x = 0;
	public int y = 0;

	void Start ()
	{
		theCameraObject.transform.position = theCameraTragetPosition.position;
		theCameraObject.transform.rotation = theCameraTragetPosition.rotation;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			RotationDirection = RotationDirections.Right;
			rotateObject();
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			RotationDirection = RotationDirections.Left;
			rotateObject();
		}

		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			RotationDirection = RotationDirections.Up;
			rotateObject();
		}

		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			RotationDirection = RotationDirections.Down;
			rotateObject();
		}
	}

	private void rotateObject()
	{
		switch (RotationDirection)
		{
		case RotationDirections.None:
			break;
		case RotationDirections.Right:
			currentRotation = gameObject.transform.localEulerAngles;
			rotationDirection = 1;
			targetRotation.y = (currentRotation.y + (90 * -rotationDirection));
			y-= 90;
			StartCoroutine (objectRotationAnimation());
			break;
		case RotationDirections.Left:
			currentRotation = gameObject.transform.localEulerAngles;
			rotationDirection = -1;
			targetRotation.y = (currentRotation.y - (90 * rotationDirection));
			y+= 90;
			StartCoroutine (objectRotationAnimation());
			break;
		case RotationDirections.Up:
			currentRotation = gameObject.transform.localEulerAngles;
			rotationDirection = 1;
			targetRotation.x = (currentRotation.x + (90 * -rotationDirection));
			x+= 90;
			StartCoroutine (objectRotationAnimation());
			break;
		case RotationDirections.Down:
			currentRotation = gameObject.transform.localEulerAngles;
			rotationDirection = -1;
			targetRotation.x = (currentRotation.x - (90 * rotationDirection));
			x-= 90;
			StartCoroutine (objectRotationAnimation());
			break;
		}


		Quaternion tempRotation = transform.rotation;
		tempRotation.z = 0;
		transform.rotation = tempRotation;
	}


	IEnumerator objectRotationAnimation()
	{


		switch (RotationDirection)
		{
		case RotationDirections.None:
			break;
		case RotationDirections.Right:
			currentRotation.y += (rotationStep * rotationDirection);
			gameObject.transform.localEulerAngles = currentRotation;
			yield return new WaitForSeconds (0);
			//if (((int)currentRotation.y >
			    // (int)targetRotation.y && rotationDirection < 0) || // for clockwise
			    //((int)currentRotation.y < (int)targetRotation.y && rotationDirection > 0)) // for anti-clockwise
			//{
				//StartCoroutine (objectRotationAnimation());
			//}
			//else
			//{
			//gameObject.transform.localEulerAngles = targetRotation;
			gameObject.transform.rotation = Quaternion.Euler(0,y,0) /** transform.rotation*/;
			//}
			break;
		case RotationDirections.Left:
			currentRotation.y -= (rotationStep * rotationDirection);
			gameObject.transform.localEulerAngles = currentRotation;
			yield return new WaitForSeconds (0);
			//if (((int)currentRotation.y >
			    // (int)targetRotation.y && rotationDirection < 0) || // for clockwise
			    //((int)currentRotation.y < (int)targetRotation.y && rotationDirection > 0)) // for anti-clockwise
			//{
				//StartCoroutine (objectRotationAnimation());
			//}
			//else
			//{
			//gameObject.transform.localEulerAngles = targetRotation;
			gameObject.transform.rotation = Quaternion.Euler(0,y,0) /**transform.rotation*/;
			//}
			break;
		case RotationDirections.Up:
			currentRotation.x += (rotationStep * rotationDirection);
			gameObject.transform.localEulerAngles = currentRotation;
			yield return new WaitForSeconds (0);
			//if (((int)currentRotation.x >
			     //(int)targetRotation.x && rotationDirection < 0) || // for clockwise
			    //((int)currentRotation.x < (int)targetRotation.x && rotationDirection > 0)) // for anti-clockwise
			//{
				//StartCoroutine (objectRotationAnimation());
			//}
			//else
			//{
			//gameObject.transform.localEulerAngles = targetRotation;
			gameObject.transform.rotation = Quaternion.Euler(x,0,0) /**transform.rotation*/;
			//}
			break;
		case RotationDirections.Down:
			currentRotation.x -= (rotationStep * rotationDirection);
			gameObject.transform.localEulerAngles = currentRotation;
			yield return new WaitForSeconds (0);
			//if (((int)currentRotation.x >
			     //(int)targetRotation.x && rotationDirection < 0) || // for clockwise
			    //((int)currentRotation.x < (int)targetRotation.x && rotationDirection > 0)) // for anti-clockwise
			//{
				//StartCoroutine (objectRotationAnimation());
			//}
			//else
			//{
			//gameObject.transform.localEulerAngles = targetRotation;
			gameObject.transform.rotation = Quaternion.Euler(x,0,0) /**transform.rotation*/;
			//}
			break;
		}


	}
}