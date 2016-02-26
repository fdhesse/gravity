using UnityEngine;
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
[RequireComponent(typeof(CapsuleCollider))]
//[RequireComponent(typeof(Animator))]
//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Screen))]
public class Pawn : MonoBehaviour
{
	// #WORLD#
	[HideInInspector] public World world;

	private static Pawn s_Instance = null;
	public static Pawn Instance { get { return s_Instance; } }

	// #PAWN#
	public float speed = 30.0f;					// Speed of the pawn
	public float maxTranslation = 2.5f;			// Max translation of the pawn
	public float turnDelay = .5f;				// Time of pawn's rotation
	public float fallDelay = .5f;				// Time of pawn's fall
	public float fallInterval = .5f;			// Gap between tile and pawn before fall
	public float jumpAnimationLength = 0.3f;

	private float height;
	private float width;
	private bool newTarget = true;
	private Vector3 desiredRotation;
	private bool isWalking;
	private bool isWalkingInStairs;

	private int tilesLayer;
	private LayerMask tilesLayerMask;
	private CapsuleCollider capsuleCollider;
	
	[HideInInspector] private bool isGlued;

	[HideInInspector] public bool isJumping = false;
	[HideInInspector] public bool isFalling = true;
	[HideInInspector] public RigidbodyConstraints nextConstraint;
	private RigidbodyConstraints transformConstraints;
	
	// #ANIMATIONS#
	// animState
	// 0 = idle
	// 1 = walk
	// 2 = fall
	// 3 = land
	private IEnumerator lookCoroutine;
	private Animator animator;
	private int animState;
	private int idleState;
	private float idleWait;
	
	// #SPAWN#
	private Vector3 spawnPosition;// position of the spawn GameObject
	private Quaternion spawnRotation;// rotation of the spawn GameObject
	
	// #TILES#
	private List<Tile> path = new List<Tile> (); // List of tiles in the current path
	private Tile pawnTile = null; // Tile beneath the Pawn
	private Tile clickedTile = null; // Tile the player clicked
	private Tile focusedTile = null; // Tile the cursor focus

	// #GUI#
	public Texture fadeinoutTexture;
	public float fadeSpeed = 1.5f;				// Speed that the screen fades to and from black.
	private float alphaFadeValue;
	private bool fading; // fading state
	private HUD hud; //script responsible for the HUD
	
	// #MOUSE#
	[HideInInspector] public bool isCameraMode = false;
	private float clickCountdown = 0.0f;

	// #SPHERES#
    private GameObject[] orientationSpheres = new GameObject[6];

	private Vector3 position
	{
		get
		{
			return transform.position;
		}
		set
		{
			if ( GetFeltVerticality() == TileOrientation.Down || GetFeltVerticality() == TileOrientation.Up )
				transform.position = new Vector3( value.x, transform.position.y, value.z );
			else if ( GetFeltVerticality() == TileOrientation.Right || GetFeltVerticality() == TileOrientation.Left )
				transform.position = new Vector3( transform.position.x, value.y, value.z );
			else
				transform.position = new Vector3( value.x, value.y, transform.position.z );
		}
	}

	void Awake()
	{
		s_Instance = this;

		tilesLayer = LayerMask.NameToLayer ("Tiles");
		tilesLayerMask = LayerMask.GetMask(new string[]{"Tiles"});

		world = gameObject.AddComponent<World>() as World;
	}

    void Start()
	{
		desiredRotation = Vector3.zero;

		isWalking = false;
		isWalkingInStairs = false;
		
		animator = transform.FindChild("OldGuy").GetComponent<Animator>();

		capsuleCollider = GetComponent<CapsuleCollider>();
		height = capsuleCollider.height * capsuleCollider.transform.localScale.y;
		width = capsuleCollider.radius * capsuleCollider.transform.localScale.x;

		// Game cursor
		Assets.SetMouseCursor();

		initSpawn();
        initHUD();
		initOrientationSpheres();

		world.Init();
		world.GameStart();
	}

	void Update()
	{
		if (!(world.IsGameOver() || hud.isPaused)) // is the game active?, i.e. is the game not paused and not finished?
		{
			UpdateAnimation();
			computeFocusedAndClickableTiles();
			manageMouse();
			movePawn();
			checkUnderneath();
		}
	}
	
	private void UpdateAnimation()
	{
		idleWait += Time.deltaTime;
		
		if ( idleWait > 1.0f )
		{
			idleWait = 0;

			float rand = UnityEngine.Random.value;

			if ( rand > 0.65f )
				idleState = Mathf.RoundToInt( rand );
		}

		if ( idleWait != animator.GetFloat( "idle_wait" ) )
			animator.SetFloat("idle_wait", idleWait);

		if ( animState != animator.GetInteger( "anim_state" ) )
			animator.SetInteger("anim_state", animState);

		if ( idleState != animator.GetInteger( "idle_state" ) )
		{
			animator.SetInteger("idle_state", idleState);
			StartCoroutine( SetIdleToZero() );
		}
	}

	private IEnumerator SetIdleToZero()
	{
		yield return new WaitForSeconds (1);
		idleState = 0;
	}
	
	private IEnumerator SetCameraCursor()
	{
		yield return null;
		// get the camera control of the main camera
		CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
		if (cameraControl != null)
			cameraControl.SetCameraCursor();
	}

	private IEnumerator SetNormalCursor()
	{
		yield return null;
		// get the camera control of the main camera
		CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
		if (cameraControl != null)
			cameraControl.SetNormalCursor();
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
	}
	
	private void initHUD()
	{
		hud = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUD>();
	}
	
	private void initOrientationSpheres()
	{
		GameObject dotsGroup = new GameObject("Orientation Dots");
		dotsGroup.hideFlags = HideFlags.HideInHierarchy;

		for ( int i = 0, l = orientationSpheres.Length; i < l; i++ )
		{
			// create a sphere primitive
			GameObject orientationSphere = GameObject.Instantiate(Assets.getGravityChangeMarkerPrefab(), Vector3.zero, Quaternion.identity) as GameObject;
			orientationSphere.name = "dot " + i;
			orientationSphere.transform.parent = dotsGroup.transform;
			// disable the sphere at first
			orientationSphere.SetActive(false);
			// and assign it in the array
			orientationSpheres[i] = orientationSphere;
		}
	}
	
	public void respawn(TileOrientation startingOrientation)
	{
		path = null;
		clickedTile = null;
		focusedTile = null;

		animState = 2;
		isFalling = true;
		isJumping = false;
		isWalking = false;

		// teleport the pawn at the spawn position
		transform.position = spawnPosition;
		transform.rotation = spawnRotation;

		// please teleport the pawn first before reseting the pawn tile
		onEnterTile(null);

		ResetDynamic();
		
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		capsuleCollider.enabled = true;

		StartCoroutine( DelayedReset ());
	}
	
	private IEnumerator DelayedReset()
	{
		GetComponent<AudioSource>().enabled = false;

		yield return new WaitForSeconds(0.1f);

		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation & ~RigidbodyConstraints.FreezePositionY;
		GetComponent<AudioSource>().enabled = true;
	}
	
	private void ResetDynamic()
	{
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		
		//World.SetGravity (GetWorldGravity());
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

	public void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.layer != tilesLayer)
			return;

		if (isFalling)
		{
			animState = 3;
			isFalling = false;
			isJumping = false;

			// Snap to the tile
			onEnterTile(collision.collider.gameObject.GetComponent<Tile>());

			moveTo( pawnTile.transform.position );
		}

		if (collision.relativeVelocity.magnitude > 1 && GetComponent<AudioSource>().enabled)
			GetComponent<AudioSource>().Play();

		ResetDynamic();
	}

	/// <summary>
	/// Call this notification every time the pawn enter on a new tile, or leave any tile
	/// meaning he has no tile under his feet. In such case call this notification specifying null
	/// as parameter. This method will set the pawnTile parameter. Please not not set the pawnTile
	/// directly, call this function instead, as other parameters need to be sync when the pawnTile
	/// changes or becomes null
	/// </summary>
	/// <param name="tile">The new tile the pawn has under his feet. CAN be null.</param>
	public void onEnterTile(Tile tile)
	{
		// save the new pawntile (can be null)
		pawnTile = tile;

		// and now check stuff related with glue, and set the glue flag
		if ((tile != null) && tile.IsGlueTile)
		{
			isGlued = true;
			GetComponent<Rigidbody>().useGravity = false;
		}
		else
		{
			isGlued = false;
			GetComponent<Rigidbody>().useGravity = true;
		}

		// if the tile is not null, check if we need to attach the pawn to the tile
		// after setting the glue flag!
		bool shouldAttachToTile = false;
		if (tile != null)
		{
			bool isWorldGravityLikePawnTile = (tile.orientation == this.GetWorldVerticality());

			// attach the pawn to the platform if is a moving platform with the gravity in the right direction
			// or if the pawn is glued to a moving or gravity platform
			shouldAttachToTile = ( (tile.tag == "MovingPlatform" && (isGlued || isWorldGravityLikePawnTile)) ||
			                      (tile.tag == "GravityPlatform" && isGlued) );
		}
		
		// attach or detach the parent of the pawn
		if (shouldAttachToTile)
			this.transform.parent = tile.transform;
		else
			this.transform.parent = null;
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
			if (pawnTile != null && pawnTile.Type.Equals(TileType.Exit)) //Has the player reached an exit Tile?
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
				fading = false;
			
		}
		else if (alphaFadeValue > 0)
		{
			alphaFadeValue -= Mathf.Clamp01(Time.deltaTime / 1);
			
			GUI.color = new Color(0, 0, 0, alphaFadeValue);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeinoutTexture);
			
			if (world.IsGameOver()) //is the game over? 
				world.GameStart();
		}
		
	}
		
	/// <summary>
    ///  Moves the pawn.
    ///  Applies player requested movement and gravity.
    /// </summary>
    private void movePawn()
	{
		if (isGrounded() ) // is the player touching a tile "beneath" him?
		{
			if( pawnTile.Type.Equals(TileType.Exit) ) //if this tile is an exit tile, make the game end
				world.GameOver();
				
            moveAlongPath(); //otherwise, move along the path to the player selected tile
        }
		else if (isWalkingInStairs || isJumping)
			moveAlongPath();
    }
	
    /// <summary>
    /// Moves along the path.
    /// If there is a path that has been previously decided the Pawn should take, finish the path.
    /// Otherwise, if there isn't a path, but there is a clickedTile, a valid tile that the player has clicked. 
    /// This clickedTile can be in a place not directly accessible to the Pawn, for example if there is a gap or if the tile is lower and the Pawn is supposed to fall.
    /// </summary>
    private void moveAlongPath()
	{
		if (path != null && path.Count > 0) //is there a path?
		{
			Tile nextTile = path[0];

			// play animation: walk
			animState = 1;
			isWalking = true;

			if (newTarget)
			{
				if ( lookCoroutine != null )
					StopCoroutine( lookCoroutine );

				lookCoroutine = LookAt ( nextTile.transform.position );
				StartCoroutine( lookCoroutine );
			}
			
            // if there is, move the pawn towards the next point in that path
			if (moveTo(nextTile.transform.position))
			{
				newTarget = true;
				position = nextTile.transform.position;

				path.RemoveAt(0); // if we have reached this path point, delete it from the list so we can go to the next one next time

				// check if the remaining path involve a move between a moving platform and an non moving platform
				// because if some tile are moving and other not along the path, the path may become broken,
				// so recompute the path everytime we move on the next tile
				if ( path.Count > 0 )
				{
					bool isStaticFound = false;
					bool isMovingFound = false;
					foreach (Tile pathTile in path)
					{
						if (pathTile.tag == "MovingPlatform")
							isMovingFound = true;
						else
							isStaticFound = true;
						// if we found both, we can stop searching
						if (isMovingFound && isStaticFound)
							break;
					}

					// if we found a path with a passage between static and moving tile, recompute the path
					if (isMovingFound && isStaticFound && (clickedTile != null))
					{
						path = AStarHelper.Calculate(path[0], clickedTile);
						// if the path is broken, clear the click tile to avoid the pawn to try to jump on it
						if (path == null)
							clickedTile = null;
					}
				}
			}

			// path can be null because we may have recompute it if we are on a moving platform
			if ((path == null) || (path.Count == 0))
			{
				animState = 0;
				isWalking = false;

				ResetDynamic();
			}
			
        }
        else if (clickedTile != null) // Case where there is no path but a target tile, ie: target tile is not aligned to tile
		{
			if ( clickedTile == pawnTile )
			{
				clickedTile = null;
			}
			else
			{
				// tile is not accessible but in valid space, so that means the pawn will jump on the tile
				// normally my verticality is the same as the world gravity, otherwise we won't have a clicked tile
				Debug.Assert(GetFeltVerticality() == GetWorldVerticality());

				// check if we didn't start jumping yet
				if ( !isJumping )
				{
					animState = 2;
					isJumping = true;
					isFalling = true;
					
					// reset the pawn tile when starting to jump, because if you jump from
					// a moving platform, you don't want to jump relative to the plateform
					onEnterTile(null);

					// the modification in height
					StartCoroutine( JumpToTile());

					// the modification in orientation
					if( lookCoroutine != null )
						StopCoroutine( lookCoroutine );					
					lookCoroutine = LookAt ( clickedTile.transform.position );
					StartCoroutine( lookCoroutine );

				}
					// calculate the vector from the Pawns position to the landing tile position at the same height
				Vector3 landingPositionAtGroundHeight = getGroundHeightPosition(clickedTile.transform.position);

				if (moveTo(landingPositionAtGroundHeight)) // move the pawn towards the landing tile
				{
					clickedTile = null; // target reached, forget it
					isJumping = false;
				}
			}
	    }
    }

	private IEnumerator JumpToTile()
	{
		float elapsedTime = 0;
		Vector3 up = new Vector3(0f, 0.25f, 0f);

		while ( elapsedTime < jumpAnimationLength )
		{
			float t = elapsedTime / jumpAnimationLength;

			transform.Translate( up * Mathf.Cos( t ), Space.Self );

			elapsedTime += Time.deltaTime;

			yield return null;
		}
	}

    /// <summary>
    /// Moves the pawn to the specified destination.
    /// Answers the question "am I there yet?"
    /// </summary>
	/// <param name="destination">the destination where the player should be moved to in world coord</param>
    /// <returns>returns true if the vector is small, i.e. smaller than 1 of magnitude, in this case, the Pawn has reached his destination</returns>
    private bool moveTo(Vector3 destination)
	{
		// Convert the world destination into local space
		Vector3 moveDirection = transform.worldToLocalMatrix.MultiplyPoint(destination);
		// stay on the ground (in local coord of the pawn)
		moveDirection.y += height * 0.5f;

		if ( moveDirection.magnitude > 1 )
		{
			Vector3 translate = Vector3.ClampMagnitude ( ( moveDirection.normalized * Time.deltaTime * speed ), maxTranslation );
			transform.Translate( translate, Space.Self );
            return false;
        }
        else
		{
			transform.Translate( moveDirection * Time.deltaTime * speed, Space.Self );
            return true;
        }
	}

	private IEnumerator LookAt( Vector3 point )
	{
		Vector3 down = getMyVerticality();

		newTarget = false;
		float elapsedTime = .0f;

		// Absolute gravity
		Vector3 absolutGravity = Vector3.Scale ( down, down );

		// 'Y' relative
		Vector3 targetPoint = Vector3.Scale ( absolutGravity, point );

		// Remove the 'Y' relative from target
		targetPoint = point - targetPoint;

		// pawn's 'Y'
		Vector3 pawnPoint = Vector3.Scale( absolutGravity, transform.position );
		
		// The final point: merged targetPoint with pawn's 'Y'
		targetPoint = targetPoint + pawnPoint;

		Vector3 forward = targetPoint - transform.position;
		
		Quaternion fromRot = transform.rotation;
		Quaternion toRot = Quaternion.identity;
		
		if ( forward != Vector3.zero )
			toRot = Quaternion.LookRotation( forward, -down );

		//float angle = Quaternion.Angle (fromRot, toRot);
		//float delay = turnDelay;

		//if (angle > 90)
		//	delay *= .5f;
		
		while ( elapsedTime < turnDelay )
		{
			elapsedTime += Time.deltaTime;
			transform.rotation = Quaternion.Lerp( fromRot, toRot, elapsedTime / turnDelay );
			yield return null;
		}

		transform.rotation = toRot;
	}

    /// <summary>
    /// Updates the value of tile to the tile beneath the Pawn.
    /// Checks the space underneath the Pawn
    /// Assigns the spheres/dots to the tiles of other orientations where the Pawn would land after gravity changes
    /// </summary>
    private void checkUnderneath()
	{
		if (isJumping || isFalling)
			return;
		
		Vector3 down = getMyVerticality();

		RaycastHit hit = new RaycastHit();

		// casting a ray down, we need a sphereCast because the capsule has thickness, and we only need tiles layer
		if (Physics.SphereCast (transform.position, width * 0.4f, down, out hit, height * 0.5f, tilesLayerMask))
		{
			GameObject hitTileGameObject = hit.collider.gameObject;
			Tile hitTile = hitTileGameObject.GetComponent<Tile>();

			// if the pawn change the tile, call the notification
			if (hitTile != pawnTile)
				onEnterTile(hitTile);

			// check if we are on the stairs
			isWalkingInStairs = false;
			if (hitTileGameObject.tag == "Stairway")
			{
				animState = 1;
				isWalkingInStairs = true;
			}
		}
		else
		{
			onEnterTile(null);
        }
	}
	
	#region focused tile, clickable tile, and destination marks
	private void removeDestinationMarks()
	{
		foreach (GameObject sphere in orientationSpheres)
		{
			//sphere.transform.position = Vector3.one * float.MaxValue; //sphere is moved to infinity muhahahaha, tremble before my power
			sphere.SetActive(false);
		}
	}

	private bool putDestinationMarks(Tile tileToCheck)
	{
		removeDestinationMarks();

		if ( isWalking || isWalkingInStairs || isFalling || isJumping )
			return false;

		bool result = false;
		TileOrientation currentWorldOrientation = GetWorldVerticality();

		for (int i = 0 ; i < orientationSpheres.Length ; ++i)
		{
			TileOrientation orientation = (TileOrientation)(i + 1);

			RaycastHit hit = new RaycastHit();

			// Casting a ray towards 'orientation', SphereCast needed because of Pawn's capsule thickness and ignoring Pawn's collider
			if (Physics.SphereCast(transform.position, width * 0.4f, World.getGravityVector(orientation), out hit, 10000, tilesLayerMask))
			{
				Tile tile = hit.collider.gameObject.GetComponent<Tile>();
				
				if ( (tile != null) && (tile.orientation != TileOrientation.None) && 
				    (tile.orientation != currentWorldOrientation) && TileSelection.isClickableType( tile.Type ) )
				{
					// check if the current tile equals the tile to check
					if (tile == tileToCheck)
						result = true;

					// reactivate the sphere
					orientationSpheres[i].SetActive(true);

					// and position it on the tile
					if (hud.dotIsInside)
						orientationSpheres[i].transform.position = tile.transform.position;
					else
						orientationSpheres[i].transform.position = tile.transform.position - (World.getGravityVector (GetFeltVerticality ()) * orientationSpheres[i].transform.localScale.y * .5f );
				}
			}
		}

		return result;
	}
	
	/// <summary>
	/// Compute and set the tile focused by cursor, if valid.
	/// Also clear and refill the array of clickable tiles.
	/// </summary>
	private void computeFocusedAndClickableTiles()
	{
		// unhighlight the previous focused tile if not null
		if (focusedTile != null)
			focusedTile.unHighlight();

		// get the tile currently pointed by the player's cursor
		Tile pointedTile = TileSelection.getTile();

		// the set the focused tile with the pointed one if it is not null and clickable
		if ((pointedTile != null) && TileSelection.isClickableType(pointedTile.Type))
			focusedTile = pointedTile;
		else
			focusedTile = null;

		// Now we will check if the focused tile is clickable or not.
		bool isFocusedTileClickable = false;

		// For that will we ask a valid AStar for normal walk navigation from the pawntile,
		// or we will check if the tile is accessible by fall from pawntile.
		if ((focusedTile != null) && (pawnTile != null))
		{	
			// first check if there's a path from the pawntile to the focused tile
			List<Tile> accessibleTiles = AStarHelper.Calculate( pawnTile, focusedTile );

			// check if the tile is accessible by walk (astar)
			if ( accessibleTiles != null && accessibleTiles.Count > 0 )
			{
				// we may have found a path from the pawntile to the focused tile,
				// which means normally we should be able to click on that focused tile.
				// But if the pawn tile is glued, and the destination focused tile is not glued,
				// (actually if the whole path is not glued), then we cannot directly go there,
				// as the pawn will fall while following the path. So the focussed tile destination 
				// is not clickable in that case. Unless of course the gravity is currently in the
				// direction of the glue tile, which is the only situation for WALKING outside 
				// a glue tile to a non glued tile.
				// Let's assume it is a valid path first:
				isFocusedTileClickable = true;
				// and check the special case
				if (isGlued && (focusedTile.orientation != GetWorldVerticality()))
				{
					foreach (Tile tile in accessibleTiles)
						if (!tile.IsGlueTile)
						{
							// we found a non glue tile in the path, so we cannot click on destination
							isFocusedTileClickable = false;
							break;
						}
				}
			}		
			// Check if the tile is accessible "by fall"
			else if ( focusedTile.orientation == pawnTile.orientation )
			{
				// the tile must be below the pawn tile and the gravity must be in the right direction
				// so if the pawn is glued on the pawntile with a gravity in different direction,
				// he cannot jump
				bool isAccessibleByFall = isTileBelow(focusedTile) && (pawnTile.orientation == GetWorldVerticality());

				// iff (if and only if)
				if ( isAccessibleByFall && ( pawnTile.orientation == TileOrientation.Down || pawnTile.orientation == TileOrientation.Up ) )
				{
					float xDist = Mathf.Abs( pawnTile.transform.position.x - focusedTile.transform.position.x );
					float zDist = Mathf.Abs( pawnTile.transform.position.z - focusedTile.transform.position.z );
					bool xTest = xDist > .1f;
					bool zTest = zDist > .1f;

					if ( xTest && zTest )
						isAccessibleByFall = false;
					else if ( !xTest && !zTest )
						isAccessibleByFall = false;

					if ( xDist > 10.1f )
						isAccessibleByFall = false;
					if ( zDist > 10.1f )
						isAccessibleByFall = false;
				}
				
				if ( isAccessibleByFall && ( pawnTile.orientation == TileOrientation.Left || pawnTile.orientation == TileOrientation.Right ) )
				{
					float yDist = Mathf.Abs( pawnTile.transform.position.y - focusedTile.transform.position.y );
					float zDist = Mathf.Abs( pawnTile.transform.position.z - focusedTile.transform.position.z );
					bool yTest = yDist > .1f;
					bool zTest = zDist > .1f;
					
					if ( yTest && zTest )
						isAccessibleByFall = false;
					else if ( !yTest && !zTest )
						isAccessibleByFall = false;

					if ( yDist > 10.1f )
						isAccessibleByFall = false;
					if ( zDist > 10.1f )
						isAccessibleByFall = false;
				}
				
				if ( isAccessibleByFall && ( pawnTile.orientation == TileOrientation.Front || pawnTile.orientation == TileOrientation.Back ) )
				{
					float xDist = Mathf.Abs( pawnTile.transform.position.x - focusedTile.transform.position.x );
					float yDist = Mathf.Abs( pawnTile.transform.position.y - focusedTile.transform.position.y );
					bool xTest = xDist > .1f && xDist < 10.1f;
					bool yTest = yDist > .1f && yDist < 10.1f;
					
					if ( xTest && yTest )
						isAccessibleByFall = false;
					else if ( !xTest && !yTest )
						isAccessibleByFall = false;
					
					if ( xDist > 10.1f )
						isAccessibleByFall = false;
					if ( yDist > 10.1f )
						isAccessibleByFall = false;
				}

				// now set the clickable flag if we can jump on it
				isFocusedTileClickable = isAccessibleByFall;
			}
		}

		// now compute the destination marks for the gravity change.
		// This function will also mark some tiles as clickable
		bool canFocusedTileClhangeGravity = putDestinationMarks(focusedTile);
		// update the clickable flag
		isFocusedTileClickable = isFocusedTileClickable || canFocusedTileClhangeGravity;

		// now check if the focused tile is the same as the pawn tile, we only authorize the click
		// if the player is glued and the gravity is not under his feet,
		// in order for the player to put back the gravity under his feet.
		if ((focusedTile != null) && (focusedTile == pawnTile))
			isFocusedTileClickable = isGlued && (focusedTile.orientation != TileOrientation.None) && (focusedTile.orientation != GetWorldVerticality());

		// now highlight the focused tile if it is clickable (may happen with AStar navigation, fall or gravity change)
		if (focusedTile != null)
		{
			if (isFocusedTileClickable)
				focusedTile.highlight();
			else
				focusedTile.unHighlight();
		}
	}
	#endregion

    /// <summary>
    /// Manages the interaction with the mouse.
    /// </summary>
    private void manageMouse()
	{
		// for very low framerate, we give at least 3 frames to switch to camera mode,
		// otherwise for normal framerate, we use a fixed value of 1 quarter of second.
		float durationToSwitchToCameraMode = Math.Max(0.25f, 3.0f * Time.deltaTime);

		// reset the click down duration if the button is up
		if (!InputManager.isClickHeldDown())
			clickCountdown = 0;

		if( !isCameraMode )
		{
			if (isFalling || isJumping || (path != null && path.Count > 0))
				return;

	        if (InputManager.isClickHeldDown())
			{
				clickCountdown += Time.deltaTime;

				if(clickCountdown > durationToSwitchToCameraMode)
				{
					isCameraMode = true;
					StartCoroutine(SetCameraCursor());
					if (focusedTile != null)
						focusedTile.unHighlight();
				}
	        }

			if (InputManager.isClickUp())
			{
				if (clickCountdown > durationToSwitchToCameraMode)
				{
					StartCoroutine(SetNormalCursor());
					isCameraMode = false;
					if (focusedTile != null)
						focusedTile.unHighlight();
				}
				else
				{
					if ( isWalking || isFalling || focusedTile == null || world.FallingCubes() )
						return;

					// if the focussed tile is highlighted that means it is clickable
					if (focusedTile.IsHighlighted)
					{
						// If the player clicked a tile with different orientation
						if ( ( focusedTile.orientation != pawnTile.orientation ) || 
						     ( isGlued && (focusedTile == pawnTile)) )
						{
							// player has changed the gravity, increase the counter
							hud.gravityChangeCount++;

							// If the pawn is on a glue tile, the change of gravity is managed differently
							if ( isGlued )
							{
								world.SetGravity( focusedTile.orientation );
							}
							else //for punishing gravity take the tile == null here
							{
								onEnterTile(null);
								StartCoroutine( DelayedPawnFall ( focusedTile.orientation ));
							}
						}
						else
						{
							// memorised the clicked tile
							clickedTile = focusedTile;
							// ask a new path if we go to a different tile
							if (pawnTile != clickedTile)
								path = AStarHelper.Calculate(pawnTile, clickedTile);
						}
					}
	            }
	        }
		}
		else if (InputManager.isClickUp())
		{
			StartCoroutine(SetNormalCursor());
			isCameraMode = false;
		}
	}
	
	private IEnumerator DelayedPawnFall( TileOrientation orientation )
	{
		Vector3 desiredPosition = new Vector3( 0, height * 0.5f * fallInterval, 0 );

		// Block the "manageMouse" loop
		isFalling = true;

//		collider.gameObject.layer = 12;
		nextConstraint = GetComponent<Rigidbody>().constraints;

		ResetDynamic();
		GetComponent<Rigidbody>().useGravity = false;
		GetComponent<Rigidbody>().constraints = transformConstraints;

		float timer = .0f;
		float delay = fallDelay * 0.5f;

		// Make the pawn float in the airs a little
		while( timer < delay )
		{
			timer += Time.deltaTime;
			Vector3 toPos = desiredPosition * timer / delay;
			desiredPosition = desiredPosition - toPos;
			transform.Translate( toPos, Space.Self );
			yield return null;
		}
		
		SetPawnOrientation( orientation );
		
		Quaternion fromRot = transform.rotation;
		Quaternion toRot = Quaternion.Euler ( desiredRotation );

		timer = .0f;

		// Rotate the pawn in order to face the correct direction
		while( timer < delay )
		{
			timer += Time.deltaTime;
			transform.rotation = Quaternion.Lerp(fromRot, toRot, timer / delay);
			yield return null;
		}

		transform.rotation = toRot;

		// Fall animation
		animState = 2;
		
		GetComponent<Rigidbody>().constraints = nextConstraint;
		GetComponent<Rigidbody>().useGravity = true;

		world.SetGravity( orientation );
	}

	private void SetPawnOrientation(TileOrientation orientation)
	{
		switch (orientation)
		{
		case TileOrientation.Front:
			desiredRotation = new Vector3(270, 0, 0);
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationX;
			break;
		case TileOrientation.Back:
			desiredRotation = new Vector3(90, 0, 0);
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationX;
			break;
		case TileOrientation.Right:
			desiredRotation = new Vector3(0, 0, 90);
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationZ;
			break;
		case TileOrientation.Left:
			desiredRotation = new Vector3(0, 0, 270);
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationZ;
			break;
		case TileOrientation.Up:
			desiredRotation = new Vector3(0, 0, 0);
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationY;
			break;
		case TileOrientation.Down:
			desiredRotation = new Vector3(180, 0, 0);
			transformConstraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationY;
			break;
		}
	}

	private TileOrientation GetFeltVerticality()
	{
		return getTileOrientationFromDownVector( getMyVerticality() );
	}

	private TileOrientation GetWorldVerticality()
	{
		return getTileOrientationFromDownVector( Physics.gravity );
	}

	private TileOrientation getTileOrientationFromDownVector(Vector3 down)
	{
		if (down.x > 0.7f)
			return TileOrientation.Right;
		if (down.x < -0.7f)
			return TileOrientation.Left;
		if (down.y > 0.7f)
			return TileOrientation.Down;
		if (down.y < -0.7f)
			return TileOrientation.Up;
		if (down.z > 0.7f)
			return TileOrientation.Front;
		if (down.z < -0.7f)
			return TileOrientation.Back;

		return TileOrientation.Up;
	}


	// ----- CHECKERS ----- //
	// 

	/// <summary>
	/// Gets my verticality, which equals to the gravity normally, but which can be different if the pawn is
	/// glued on a wall.
	/// </summary>
	/// <returns>The my verticality for the pawn.</returns>
	private Vector3 getMyVerticality()
	{
		if (isGlued && (pawnTile != null))
			return pawnTile.getDownVector();
		else
			return Physics.gravity.normalized;
	}

	/// <summary>
	/// Checks if the pawn is grounded.
	/// Answers the question " is the player touching a tile "beneath" him?" where beneath relates to the current gravitational orientation.
	/// </summary>
	private bool isGrounded()
	{
		//is there even a tile beneath the Pawn
		if (pawnTile != null) 
		{
			Vector3 down = getMyVerticality();
			return Physics.SphereCast( new Ray( transform.position, down ), width * 0.5f, height * 0.5f, tilesLayerMask );
		}
		
		return false; // if there isn't a tile beneath him, he isn't grounded
	}


	/// <summary>
	/// Is the target tile above the Pawn?
	/// </summary>
	private bool isTileBelow(Tile target)
	{
		switch (pawnTile.orientation)
        {
            default:
				return pawnTile.transform.position.y > target.transform.position.y;
            case TileOrientation.Down:
				return pawnTile.transform.position.y < target.transform.position.y;
			case TileOrientation.Left:
				return pawnTile.transform.position.x > target.transform.position.x;
			case TileOrientation.Right:
				return pawnTile.transform.position.x < target.transform.position.x;
            case TileOrientation.Front:
				return pawnTile.transform.position.z < target.transform.position.z;
            case TileOrientation.Back:
				return pawnTile.transform.position.z > target.transform.position.z;
        }
	}

    /// ----- GETTERS ----- ///

    /// <summary>
    /// Gets the player ground position, i.e. the position of the "feet" of the Pawn, pawn has 8 height
    /// </summary>
    private Vector3 getGroundPosition()
	{
		float halfHeight = height * 0.5f;

		switch (GetFeltVerticality())
		{
		default:
			return new Vector3 (transform.position.x, transform.position.y - halfHeight, transform.position.z);
		//case TileOrientation.Up:
		//	return new Vector3 (transform.position.x, transform.position.y - halfHeight, transform.position.z);
		case TileOrientation.Down:
			return new Vector3 (transform.position.x, transform.position.y + halfHeight, transform.position.z);
		case TileOrientation.Left:
			return new Vector3 (transform.position.x - halfHeight, transform.position.y, transform.position.z);
		case TileOrientation.Right:
			return new Vector3 (transform.position.x + halfHeight, transform.position.y, transform.position.z);
		case TileOrientation.Front:
			return new Vector3 (transform.position.x, transform.position.y, transform.position.z + halfHeight);
		case TileOrientation.Back:
			return new Vector3 (transform.position.x, transform.position.y, transform.position.z - halfHeight);
		}
    }

    /// <summary>
    /// Gets the ground height vector for the target position.
    /// </summary>
    /// <param name="position">Position of something</param>
    /// <returns>The position of something at the same height as the Pawn</returns>
    private Vector3 getGroundHeightPosition(Vector3 position)
	{
		TileOrientation pawnOrientation = GetFeltVerticality();

		if (pawnOrientation == TileOrientation.Up || pawnOrientation == TileOrientation.Down)
            return new Vector3(position.x, getGroundPosition().y, position.z);
		else if (pawnOrientation == TileOrientation.Left || pawnOrientation == TileOrientation.Right)
            return new Vector3(getGroundPosition().x, position.y, position.z);
        else
            return new Vector3(position.x, position.y, getGroundPosition().z);
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
