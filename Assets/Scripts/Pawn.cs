﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// <para>Monobehaviour class responsible for the player's pawn.</para>
/// <para>Since it is a monobehaviour, its supposed to be attached to a gameobject.</para>
/// <para>It has Pawn Movement, pathfinding, interactions and also some gamelogic.</para>
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
//[RequireComponent(typeof(Animator))]
//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Screen))]
public class Pawn : MonoBehaviour
{
	// #WORLD#
	public World world;
	private float G;
	
	// #PAWN#
	public float speed = 30.0f;					// Speed of the pawn
	public float turnDelay = 0.5f; // 1.0f		// délai avant la chute
	public float fallInterval = 4.0f;
	private float height;
	private bool newTarget = true;
	private bool lookAtDestroyCoincidents;
	private Vector3 desiredRotation;
	private Vector3 desiredPosition;

//	private query
	
	private BoxCollider boxCollider;
	
	public bool isFalling;
	public RigidbodyConstraints nextConstraint;
	private RigidbodyConstraints transformConstraints;
	
	// #ANIMATIONS#
	private Animator animator;
	private int animState;
	private float idleWait;
	
	// #SPAWN#
	private Vector3 spawnPosition;// position of the spawn GameObject
	private Quaternion spawnRotation;// rotation of the spawn GameObject
	
	// #PLATFORMS#
	private List<Platform> path = new List<Platform> (); // List of platforms in the current path
	private Platform platform;// Platform beneath the Pawn
	private Platform targetPlatform;// Platform the player targeted
	
	// #GUI#
	public Texture fadeinoutTexture;
	public float fadeSpeed = 1.5f;				// Speed that the screen fades to and from black.
	private float alphaFadeValue;
	private bool fading; // fading state
	private bool INOUT;
	private HUD hud; //script responsible for the HUD
	
	// #MOUSE#
	public bool isCameraMode = false;
	private float lastClick;
	private float countdown;
	
	// #SPHERES#
    private GameObject[] orientationSpheres = new GameObject[6];

    /// <summary>
    ///  START
    ///  THIS IS CALLED ONCE IN THE BEGINNING
    /// </summary>
    void Start()
	{
		world = gameObject.AddComponent( "World" ) as World;
		world.Init();
		G = world.G;
		
		animator = transform.FindChild("Animations").GetComponent<Animator>();
		boxCollider = GetComponent<BoxCollider>();
		height = boxCollider.size.y * boxCollider.transform.localScale.y; // z because "Fennec" child is rotated;
//		height = collider.size.y;
		
		initSpawn();
        initHUD();
		initOrientationSpheres();
		checkUnderneath();
	}
	
	/// <summary>
	///  UPDATE
	///  THIS IS CALLED ONCE PER FRAME
	/// </summary>
	void Update()
	{
		if (!(world.IsGameOver() || hud.isPaused)) // is the game active?, i.e. is the game not paused and not finished?
		{
			UpdateAnimation();
			manageMouse();
			movePawn();
			checkUnderneath();
		}
	}
	
	private void UpdateAnimation()
	{
		idleWait += Time.deltaTime;
		
		if ( idleWait > 6.0f )
			idleWait = 0;
				
		animator.SetFloat("idle_wait", idleWait);
		animator.SetInteger("anim_state", animState);
	}
	
	private IEnumerator SetCameraCursor() {
		yield return null;
		Texture2D tex = (Texture2D)Resources.Load("cameraCursor", typeof(Texture2D));
		Cursor.SetCursor(tex,Vector2.zero,CursorMode.Auto);
	}

	private IEnumerator SetNormalCursor() {
		yield return null;
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}
	
	
	/// <summary>
	/// Fetches the position of the spawn GameObject.
	/// Incase there is no spawn it will use the Pawn's initial position as spawnPoint
	/// </summary>
	private void initSpawn()
	{
		GameObject spawn = GameObject.FindGameObjectWithTag("Spawn");
		spawnPosition = (spawn == null) ? transform.position : spawn.transform.position;
		spawnRotation = (spawn == null) ? transform.rotation : spawn.transform.rotation;

		world.GameStart();
		
	}
	
	private void initHUD()
	{
		hud = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUD>();
	}
	
	private void initOrientationSpheres()
	{
		GameObject o = new GameObject();
		o.name = "Orientation Dots";
		
		for (int i = 0; i != orientationSpheres.Length; i++)
		{
			orientationSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			orientationSpheres[i].renderer.material = Assets.getSphereMat();
			orientationSpheres[i].renderer.material.color = hud.dotColor;
			orientationSpheres[i].transform.localScale = Vector3.one * hud.dotSize;
			orientationSpheres[i].layer = 10;
			orientationSpheres[i].collider.enabled = false;
			orientationSpheres[i].name = "dot " + i;
			orientationSpheres[i].transform.parent = o.transform;
		}
	}
	
	public void respawn()
	{
		path = null;
		platform = null;
		targetPlatform = null;
		
		isFalling = true;
		
		transform.position = spawnPosition;
		transform.rotation = spawnRotation;

		ResetDynamic();
		
		animState = 0;
		
		rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		boxCollider.enabled = true;
		Physics.gravity = new Vector3(0, -G, 0);

		StartCoroutine( DelayedReset ());
	}
	
	private IEnumerator DelayedReset() {

		audio.enabled = false;

		yield return new WaitForSeconds(0.1f);

		rigidbody.constraints = RigidbodyConstraints.FreezeRotation & ~RigidbodyConstraints.FreezePositionY;
		
		audio.enabled = true;
	}
	
	private void ResetDynamic()
	{
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		
		SetWorldGravity (GetWorldGravity());
	}
	
	// TODO -- Pawn.CubeContact() -> traverser le joueur / pass through player
	// TODO -- Pawn.CubeContact() -> réécriture / rewritting
	public void CubeContact(Vector3 pos)
	{
		Vector3 _g = Vector3.Normalize (Physics.gravity);
		Vector3 _pos;
		float _a = 0;
		
		pos = Vector3.Scale (pos,Vector3.Normalize (Physics.gravity));
		pos = Vector3.Scale (pos,Vector3.Normalize (Physics.gravity));
		_pos = Vector3.Scale (transform.position, Vector3.Normalize (Physics.gravity));
		_pos = Vector3.Scale (transform.position, Vector3.Normalize (Physics.gravity));
		
		
		if (_g.x != 0)
			_a = _g.x;
		else if (_g.y != 0)
			_a = _g.y;
		else if (_g.z != 0)
			_a = _g.z;
		
		if (_a < 0 ) // gravité inférieure à 0, le cube doit etre au dessus
		{
			if (pos.x != 0)
			{
				if (pos.x > _pos.x)
					Crush ();
			}
			else if (pos.y != 0)
			{
				if (pos.y > _pos.y)
					Crush ();
			}
			else if (pos.z != 0)
			{
				if (pos.z > _pos.z)
					Crush ();
			}
		}
		else // gravité supérieur à 0, le cube doit etre au dessous
		{
			if (pos.x != 0)
			{
				if (pos.x < _pos.x)
					Crush ();
			}
			else if (pos.y != 0)
			{
				if (pos.y < _pos.y)
					Crush ();
			}
			else if (pos.z != 0)
			{
				if (pos.z < _pos.z)
					Crush ();
			}
		}
	}
	
	private void Crush()
	{
		world.GameOver();
		fading = true;
	}

	private void SnapToPlatform( Platform p )
	{
		path = AStarHelper.Calculate(platform, p); //give me a path towards the nearest platform
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (isFalling)
		{
			isFalling = false;
			
			if( platform != null )
			{
				Platform[] platforms = GameObject.FindObjectsOfType<Platform>();
				List<Platform> platformsList = new List<Platform>();
				
				for (int i = 0; i != platforms.Length; i++)
				{
					if (platforms[i].orientation == platform.orientation)
						platformsList.Add(platforms[i]);
				}
				
				if ( platformsList.Count > 0 )
				{
					Platform nearest = Platform.Closest(platformsList, transform.position); //nearest platform: the directly accessible platform from the platform bellow the Pawn, thats closest to the target platform
					SnapToPlatform( nearest );
				}
				else
				{
					Debug.LogError("No Platform found !");
				}
			}
		}

		if (collision.relativeVelocity.magnitude > 1 && audio.enabled)
			audio.Play();

		ResetDynamic();
	}

    /// <summary>
    ///  ONGUI
    ///  THIS IS CALLED ONCE PER FRAME
    ///  
    /// Checks if the game as ended, if it has, it activates the HUD's endscreen
    /// </summary>
    void OnGUI()
    {
		if (world.IsGameOver()) //is the game over? 
        {
			if (platform != null && platform.type.Equals(PlatformType.Exit))//Has the player reached an exit Platform?
                hud.isEndScreen = true; //activate the endscreen
		}
		
		// #FADEINOUT_TEXTURE#
		if (!fadeinoutTexture)
		{
			Debug.LogError("Missing texture");
			return;
		}
		
		
		if (fading)
		{
			alphaFadeValue += Mathf.Clamp01(Time.deltaTime / 1);
			
			GUI.color = new Color(0, 0, 0, alphaFadeValue);
			
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeinoutTexture);
			
			if (alphaFadeValue > 1)
			{
				fading = false;
			}
			
		}
		else if (alphaFadeValue > 0)
		{
			alphaFadeValue -= Mathf.Clamp01(Time.deltaTime / 1);
			
			GUI.color = new Color(0, 0, 0, alphaFadeValue);
			
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeinoutTexture);
			
			if (world.IsGameOver()) //is the game over? 
			{
				world.GameStart();
			}
		}
		
	}
		
	/// <summary>
    ///  Moves the pawn.
    ///  Applies player requested movement and gravity.
    /// </summary>
    private void movePawn()
	{
        if (isGrounded()) // is the player touching a platform "beneath" him?
		{
			if( platform.type.Equals(PlatformType.Exit) ) //if this platform is an exit platform, make the game end
				world.GameOver();
				
            moveAlongPath(); //otherwise, move along the path to the player selected platform
        }
    }
	
    /// <summary>
    /// Moves along the path.
    /// If there is a path that has been previously decided the Pawn should take, finish the path.
    /// Otherwise, if there isn't a path, but there is a targetPlatform, a valid platform that the player has clicked. 
    /// This targetPlatform can be in a place not directly accessible to the Pawn, for example if there is a gap or if the platform is lower and the Pawn is supposed to fall.
    /// </summary>
    private void moveAlongPath()
	{
		ResetDynamic ();
		
		if (path != null && path.Count > 0) //is there a path?
		{
//			Debug.Log("Ok, ok, I go !");
			// play animation: walk
			animState = 2;
			
			if (newTarget)
				StartCoroutine( LookAt ( path[0].transform.position ) );
			
            //if there is, move the pawn towards the next point in that path
            Vector3 vec = path[0].transform.position - getGroundPosition();
			
//			Debug.Log( "vector: " + vec );
            if (moveMe(vec))
			{
				newTarget = true;
				path.RemoveAt(0); //if we have reached this path point, delete it from the list so we can go to the next one next time
			}
			
			if (path.Count == 0)
				animState = 0;
			
        }
        else if (targetPlatform != null) //ok, there is no path, but is there a target platform ? (this happends when the chosen platform is not directly accessible)
		{
			
			if (targetPlatformIsAbove(targetPlatform) || targetPlatform.type.Equals(PlatformType.Invalid))//is targetPlatform is above the pawn or of type invalid?
			{
	            GetComponent<AudioSource>().PlayOneShot(Assets.invalidSound); //play a failed action sound
	            targetPlatform = null; //forget target platform
	        }
	        else //the platform is in a valid place
	        {
	            // the platform isn't directly accessible but it is valid
	            // the pawn will either go towards the platform and land on a place where them it can access directly the platform
	            // or it will land on the platform
	            // or it will fall into the void

	            Platform nearest = Platform.Closest(platform.AllAccessiblePlatforms(), targetPlatform.transform.position); //nearest platform: the directly accessible platform from the platform bellow the Pawn, thats closest to the target platform
	            
	            if (nearest.Equals(platform)) //is the nearest the one bellow the Pawn?
				{
	                Platform landing = Platform.Closest(targetPlatform.AllAccessiblePlatforms(), nearest.transform.position);//landing platform: the directly accessible platform from the target platform, thats closest to the nearest platform

	                Vector3 vec = getGroundHeightVector(landing.transform.position) - getGroundPosition(); // calculate the vector from the Pawns position to the landing platform position at the same height

	                if (moveMe(vec)) //move the pawn towards that vector
						targetPlatform = null; //if we are already there, forget targetPlatform
	            }
	            else
				{
	                path = AStarHelper.Calculate(platform, nearest); //give me a path towards the nearest platform
	            }
			}
	    }
    }

    /// <summary>
    /// Moves the pawn in the direction of the vector.
    /// Answers the question "am I there yet?"
    /// </summary>
    /// <param name="vec">direction the player should be moved to</param>
    /// <returns>returns true if the vector is small, i.e. smaller than 1 of magnitude, in this case, the Pawn has reached his destination</returns>
    private bool moveMe(Vector3 vec)
	{
		if (Vector3.Magnitude(vec) > 1)
		{
			transform.Translate(Vector3.Normalize(vec) * Time.deltaTime * speed, Space.World);

//			GetComponent<BoxCollider>().transform.Translate(Vector3.Normalize(vec) * Time.deltaTime * speed, Space.World);
			
            return false;
        }
        else
		{
			transform.Translate(vec * Time.deltaTime * speed, Space.World);

//			GetComponent<BoxCollider>().transform.Translate(vec * Time.deltaTime * speed, Space.World);

            return true;
        }
	}

	private IEnumerator LookAt( Vector3 point )
	{
		newTarget = false;
		float timer = 0.0f;
		
//		Vector3 target = path[0].transform.position +  -Vector3.Normalize(Physics.gravity) * height / 2.0f;

		// get the Y relative
		// point is scaled to inverted square of G
		Vector3 target = Vector3.Scale (Vector3.Scale ( - Vector3.Normalize (Physics.gravity), Vector3.Normalize (Physics.gravity)), point);
//		Debug.Log ("Y relative: " + target);

		// get the X, Z relatives
		// remove the Y relative from target
		target = point + target;
//		Debug.Log ("X, Z relatives: " + target);

		// get the final point
		// remove pawn's & platform's heights from target
		target = target - Vector3.Normalize (Physics.gravity) * height / 2.0f - Vector3.Scale ( - Vector3.Normalize (Physics.gravity), platform.transform.position);
//		Debug.Log ("Final point: " + target);

		Quaternion _look = Quaternion.LookRotation( target - transform.position, -Vector3.Normalize(Physics.gravity) );
		
		lookAtDestroyCoincidents = true;
		yield return 0;
		lookAtDestroyCoincidents = false;
		
		while (true)
		{
			if (lookAtDestroyCoincidents)
				break;

			timer += Time.deltaTime;
			
			Quaternion _rot = transform.rotation;
			transform.rotation = Quaternion.Slerp( transform.rotation, _look, timer / turnDelay );
			
			if ( _rot == transform.rotation || Vector3.Magnitude(target - transform.position) < 2 )
				break;
			
			yield return 0;
		}
	}

    /// <summary>
    /// Updates the value of platform to the platform beneath the Pawn.
    /// Checks the space underneath the Pawn
    /// Assigns the spheres/dots to the platforms of other orientations where the Pawn would land after gravity changes
    /// </summary>
    private void checkUnderneath()
    {
        //checks underneath the pawn
		Platform p = null;

		RaycastHit hit = new RaycastHit();

		Vector3 _pos = transform.position - Vector3.Normalize (Physics.gravity) * 0.4f;

		// TODO -- Careful, 10 was 10000
		if (Physics.SphereCast(_pos, height / 2.0f + 0.2f, Physics.gravity, out hit, 10, ~(1 << 10)))//casting a ray down, we need a sphereCast because the capsule has thickness, and we need to ignore the Pawn collider
        {
			p = hit.collider.gameObject.GetComponent<Platform>();

            if (p != null) // if it is a platform
                platform = p;
            else
				platform = null;
			
			if ( hit.collider.gameObject.tag == "MovingPlatform" )
			{
//				Debug.Log( "on a Moving Platform" );

				// dirty snapping to a moving platform
				// actually, a mask is used to mix up both player & platform positions

				Vector3 _vec = new Vector3( 0.0f, 0.0f, 0.0f );

				if (path.Count > 0 )
				{
					// if the player asked the pawn to move

					_vec = path[0].transform.position - getGroundPosition();
					_vec = _vec - Vector3.Scale ( Vector3.Scale ( Vector3.Normalize (Physics.gravity), Vector3.Normalize (Physics.gravity)), _vec);

					if (_vec.x != 0)
						_vec.x = 1;
					if (_vec.y != 0)
						_vec.y = 1;
					if (_vec.z != 0)
						_vec.z = 1;
				}

				// _ppos: platform position 
				Vector3 _ppos = p.transform.position - Vector3.Scale ( Vector3.Scale ( Vector3.Normalize (Physics.gravity), Vector3.Normalize (Physics.gravity)), p.transform.position);

				// first mask: actual gravity
				Vector3 _mask = Vector3.Scale ( Vector3.Normalize (Physics.gravity), Vector3.Normalize (Physics.gravity));

				// second mask: playerPawn's movement
				_mask = _mask - _vec;

				// get absolute
				_mask = Vector3.Scale( _mask, _mask );

				// mask the pawn's position
				_vec = Vector3.Scale( _mask, transform.position );
//				Debug.Log( "_vec: " + _vec + ", masque " + _mask );

				// revert the mask
				_mask = - (_mask - new Vector3( 1.0f, 1.0f, 1.0f ) );
				
				// mask the platform's position
				_ppos = Vector3.Scale( _mask, _ppos );
//				Debug.Log( "_ppos: " + _ppos + ", masque " + _mask );

				// compute the new position
				transform.position =  _vec + _ppos;
//				Debug.Log( "transform: " + transform.position );
			}
        }
        else
		{
            platform = null;
        }

        // puts dots
        foreach (PlatformOrientation orientation in Enum.GetValues(typeof(PlatformOrientation)))
        {
            p = null;
            RaycastHit hitc = new RaycastHit();
			if (Physics.SphereCast(_pos, height / 2.0f + 0.2f, Physics.gravity, out hitc, 10000, ~(1 << 10)))//casting a ray down, we need a sphereCast because the capsule has thickness, and we need to ignore the Pawn collider
            {
                p = hitc.collider.gameObject.GetComponent<Platform>();
                if (p != null && p != platform)
                {
                    if (hud.dotIsInside)
                        getOrientationSphere(orientation).transform.position = p.transform.position;
                    else
                        getOrientationSphere(orientation).transform.position = p.transform.position - (getGravityVector(orientation) * hud.dotSize / 2);
                }
                else
                {
                    getOrientationSphere(orientation).transform.position = Vector3.one * float.MaxValue; //sphere is moved to infinity muhahahaha, tremble before my power
                }

            }
            else
            {
                getOrientationSphere(orientation).transform.position = Vector3.one * float.MaxValue; //BEGONE
            }
        }
    }

    /// <summary>
    /// Manages the interaction with the mouse.
    /// </summary>
    private void manageMouse()
	{
        PlatformSelection.highlightTargetPlatform();
        Platform p = PlatformSelection.getPlatform();

		if(!isCameraMode)
		{
	        if (Input.GetMouseButton(0))
			{
				if(Time.time - lastClick < .1)
					countdown += Time.deltaTime;
				else
					countdown = 0;

	            lastClick = Time.time;

				if(countdown > .25)
				{
					isCameraMode = true;
					StartCoroutine(SetCameraCursor());
				}
	        }

			if (Input.GetMouseButtonUp(0) && !isCameraMode && rigidbody.useGravity)
			{
				countdown = 0;

				if (p != null && !isFalling && !world.FallingCubes())
				{
					if ( platform == null || p.orientation != platform.orientation ) //for punishing gravity take the platform == null here
					{
						hud.gravityChangeCount++;
						platform = null;
						
						desiredPosition = new Vector3( 0, height / 2 * fallInterval, 0 );
												
						world.ChangeGravity ();
						SetWorldGravity( p.orientation );
						StartCoroutine( DelayedPawnFall ());
					}
					else
					{
						path = AStarHelper.Calculate(platform, p);
						targetPlatform = p;
						StartCoroutine( LookAt ( targetPlatform.transform.position ) );
						newTarget = true;
					}
	            }
	        }
		}
		else
		{
			if (Input.GetMouseButtonUp(0))
			{
				StartCoroutine(SetNormalCursor());
				isCameraMode = false;
			}
		}
	}
	
	private IEnumerator DelayedPawnFall()
	{
//		collider.gameObject.layer = 12;
		nextConstraint = rigidbody.constraints;

		ResetDynamic();

		float timer = 0.0f;

		isFalling = true;
		
		while(true)
		{
			rigidbody.useGravity = false;
			rigidbody.constraints = transformConstraints;
			
			if(adjustPawnPosition( ref timer ))
			{
				timer = 0.0f;
				break;
			}
			
			yield return 0;
		}
		
		while(true)
		{
			rigidbody.useGravity = false;
			rigidbody.constraints = transformConstraints;
			
			if(adjustPawnRotation( ref timer ))
			{
				timer = 0.0f;
				break;
			}

			yield return 0;
		}
		
		rigidbody.constraints = nextConstraint;
		rigidbody.useGravity = true;

//		yield return new WaitForSeconds (0.1f);

//		gameObject.layer = 0;
	}
	
	private bool adjustPawnPosition( ref float timer )
	{
		timer += Time.deltaTime;
		
		Vector3 _pos = transform.position;
		Vector3 _to = desiredPosition * timer / turnDelay;
		
		transform.Translate( _to, Space.Self );
		
		// compute the difference between before & now
		// in order to stop at the relative goal
		desiredPosition = desiredPosition - _to;
		
		if ( _pos == transform.position )
			return true;
		else
			return false;
	}
	
	/// <summary>
	/// Align the pawn with the orientation provided by SetWorldGravity()
	/// Also determines the "animation" of this movement. (not yet)
	/// </summary>
	private bool adjustPawnRotation( ref float timer )
	{
		timer += Time.deltaTime;
			
		Quaternion _to = Quaternion.Euler (desiredRotation);

		Quaternion _rot = transform.rotation;
		transform.rotation = Quaternion.Slerp(transform.rotation, _to, timer / turnDelay);
		
		if ( _rot == transform.rotation )
			return true;
		else
			return false;
	}

	private void SetWorldGravity(PlatformOrientation orientation)
	{
		switch (orientation)
		{
		default:
			break;
		case PlatformOrientation.Front:
//			Debug.Log("Front");
			rigidbody.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionZ;
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationX;
			Physics.gravity = new Vector3(0, 0, G);
			desiredRotation = new Vector3(270, 0, 0);
			break;
		case PlatformOrientation.Back:
//			Debug.Log("Back");
			rigidbody.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionZ;
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationX;
			Physics.gravity = new Vector3(0, 0, -G);
			desiredRotation = new Vector3(90, 0, 0);
			break;
		case PlatformOrientation.Right:
//			Debug.Log("Right");
			rigidbody.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionX;
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationZ;
			Physics.gravity = new Vector3(G, 0, 0);
			desiredRotation = new Vector3(0, 0, 90);
			break;
		case PlatformOrientation.Left:
//			Debug.Log("Left");
			rigidbody.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionX;
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationZ;
			Physics.gravity = new Vector3(-G, 0, 0);
			desiredRotation = new Vector3(0, 0, 270);
			break;
		case PlatformOrientation.Up:
//			Debug.Log("Up");
			rigidbody.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationY;
			Physics.gravity = new Vector3(0, -G, 0);
			desiredRotation = new Vector3(0, 0, 0);
			break;
		case PlatformOrientation.Down:
//			Debug.Log("Down");
			rigidbody.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationY;
			Physics.gravity = new Vector3(0, G, 0);
			desiredRotation = new Vector3(180, 0, 0);
			break;
		}
	}

	private PlatformOrientation GetWorldGravity()
	{
		if (Physics.gravity.x > 0)
			return PlatformOrientation.Right;
		if (Physics.gravity.x < 0)
			return PlatformOrientation.Left;
		if (Physics.gravity.y > 0)
			return PlatformOrientation.Down;
		if (Physics.gravity.y < 0)
			return PlatformOrientation.Up;
		if (Physics.gravity.z > 0)
			return PlatformOrientation.Front;
		if (Physics.gravity.z < 0)
			return PlatformOrientation.Back;
		else
			return PlatformOrientation.Up;
	}

	/// ----- CHECKERS ----- ///
	/// 
	
	/// <summary>
	/// Checks if the pawn is grounded.
	/// Answers the question " is the player touching a platform "beneath" him?" where beneath relates to the current gravitational orientation.
	/// </summary>
	private bool isGrounded()
	{
		if (platform != null) //is there even a platform beneath the Pawn
		{
		//	TODO -- potentially better but not implemented
//			return Physics.Raycast(transform.position, Vector3.Normalize( Physics.gravity ), height + 0.2f);
			
			float proximityThreshold = 0.2f; // the value bellow which can be said that the platform is touching the Pawn, it's like an error margin
			
			if (platform.orientation.Equals(PlatformOrientation.Up) || platform.orientation.Equals(PlatformOrientation.Down)) //check for up and down
				return Math.Abs(platform.transform.position.y - getGroundPosition().y) < proximityThreshold; //check distance
			
			if (platform.orientation.Equals(PlatformOrientation.Left) || platform.orientation.Equals(PlatformOrientation.Right)) //check for left and right
				return Math.Abs(platform.transform.position.x - getGroundPosition().x) < proximityThreshold; //check distance
			
			if (platform.orientation.Equals(PlatformOrientation.Front) || platform.orientation.Equals(PlatformOrientation.Back)) //check for front and back
				return Math.Abs(platform.transform.position.z - getGroundPosition().z) < proximityThreshold; //check distance
		}
		
		return false; // if there isn't a platform beneath him, he isn't grounded
	}
	
	/// <summary>
	/// Is the target platform above the Pawn?
	/// </summary>
	private bool targetPlatformIsAbove(Platform target)
	{
        switch (platform.orientation)
        {
            default:
                return platform.transform.position.y < target.transform.position.y;
            case PlatformOrientation.Down:
				return platform.transform.position.y > target.transform.position.y;
			case PlatformOrientation.Left:
				return platform.transform.position.x < target.transform.position.x;
			case PlatformOrientation.Right:
                return platform.transform.position.x > target.transform.position.x;
            case PlatformOrientation.Front:
                return platform.transform.position.z > target.transform.position.z;
            case PlatformOrientation.Back:
                return platform.transform.position.z < target.transform.position.z;
        }
	}

    /// ----- GETTERS ----- ///

    /// <summary>
    /// Gets the player rotation according to the current gravitational orientation.
    /// </summary>
    private Vector3 getPlayerRotation()
	{
		switch (GetWorldGravity())
		{
		default:
                return Vector3.zero;
            case PlatformOrientation.Down:
				return new Vector3(0, 180, 180);
			case PlatformOrientation.Left:
				return new Vector3(0, 180, 90);
			case PlatformOrientation.Right:
                return new Vector3(0, 0, 90);
            case PlatformOrientation.Front:
                return new Vector3(0, 270, 90);
            case PlatformOrientation.Back:
                return new Vector3(0, 90, 90);
        }
	}

    /// <summary>
    /// Gets the player ground position, i.e. the position of the "feet" of the Pawn, pawn has 8 height
    /// </summary>
    private Vector3 getGroundPosition()
	{
		float n = (transform.localScale.y * height) / 2.0f;

		switch (GetWorldGravity())
		{/*
		default:
			return new Vector3 (collider.transform.position.x, collider.transform.position.y - n, collider.transform.position.z);
		case PlatformOrientation.Up:
			return new Vector3 (collider.transform.position.x, collider.transform.position.y - n, collider.transform.position.z);
		case PlatformOrientation.Down:
			return new Vector3 (collider.transform.position.x, collider.transform.position.y + n, collider.transform.position.z);
		case PlatformOrientation.Left:
			return new Vector3 (collider.transform.position.x - n, collider.transform.position.y, collider.transform.position.z);
		case PlatformOrientation.Right:
			return new Vector3 (collider.transform.position.x + n, collider.transform.position.y, collider.transform.position.z);
		case PlatformOrientation.Front:
			return new Vector3 (collider.transform.position.x, collider.transform.position.y, collider.transform.position.z + n);
		case PlatformOrientation.Back:
			return new Vector3 (collider.transform.position.x, collider.transform.position.y, collider.transform.position.z - n);
			/*/
		default:
			return new Vector3 (transform.position.x, transform.position.y - n, transform.position.z);
		case PlatformOrientation.Up:
			return new Vector3 (transform.position.x, transform.position.y - n, transform.position.z);
		case PlatformOrientation.Down:
			return new Vector3 (transform.position.x, transform.position.y + n, transform.position.z);
		case PlatformOrientation.Left:
			return new Vector3 (transform.position.x - n, transform.position.y, transform.position.z);
		case PlatformOrientation.Right:
			return new Vector3 (transform.position.x + n, transform.position.y, transform.position.z);
		case PlatformOrientation.Front:
			return new Vector3 (transform.position.x, transform.position.y, transform.position.z + n);
		case PlatformOrientation.Back:
			return new Vector3 (transform.position.x, transform.position.y, transform.position.z - n);
			//*/
		}
    }

    /// <summary>
    /// Gets the gravitational orientation vector.
    /// </summary>
    public Vector3 getGravityVector(PlatformOrientation vec)
	{
        switch (vec)
        {
            default:
                return new Vector3(0, -1, 0);
            case PlatformOrientation.Down:
                return new Vector3(0, 1, 0);
            case PlatformOrientation.Right:
                return new Vector3(-1, 0, 0);
            case PlatformOrientation.Left:
                return new Vector3(1, 0, 0);
            case PlatformOrientation.Front:
                return new Vector3(0, 0, 1);
            case PlatformOrientation.Back:
                return new Vector3(0, 0, -1);
        }
    }


    /// <summary>
    /// Gets the ground height vector for the target position.
    /// </summary>
    /// <param name="position">Position of something</param>
    /// <returns>The position of something at the same height as the Pawn</returns>
    private Vector3 getGroundHeightVector(Vector3 position)
	{
		if (platform.orientation.Equals(PlatformOrientation.Up) || platform.orientation.Equals(PlatformOrientation.Down))
            return new Vector3(position.x, getGroundPosition().y, position.z);
		else if (platform.orientation.Equals(PlatformOrientation.Left) || platform.orientation.Equals(PlatformOrientation.Right))
            return new Vector3(getGroundPosition().x, position.y, position.z);
        else
            return new Vector3(position.x, position.y, getGroundPosition().z);
	}

    /// <summary>
    /// Gets the game object of the sphere for a given PlatformOrientation
    /// </summary>
    private GameObject getOrientationSphere(PlatformOrientation orientation)
    {
        return orientationSpheres[(int)orientation];
	}
	/// <summary>
	/// Is the Pawn out of bounds?
	/// </summary>
	public void outOfBounds()
	{
		world.GameOver();
		fading = true;
	}
}
