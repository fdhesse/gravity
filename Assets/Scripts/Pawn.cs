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

	private LayerMask tilesLayer;
	private CapsuleCollider capsuleCollider;
	
	[HideInInspector] public TileOrientation orientation;
	[HideInInspector] public bool isGlued;
	[HideInInspector] public bool isLeavingGlueTile;
	[HideInInspector] public Vector3 tileGravityVector;

	[HideInInspector] public bool isJumping;
	[HideInInspector] public bool isFalling;
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
	private Tile pawnTile; // Tile beneath the Pawn
	private Tile clickedTile; // Tile the player clicked
	private Tile focusedTile; // Tile the cursor focus
	private List<Tile> clickableTiles = new List<Tile> ();
	
	// #GUI#
	public Texture fadeinoutTexture;
	public float fadeSpeed = 1.5f;				// Speed that the screen fades to and from black.
	private float alphaFadeValue;
	private bool fading; // fading state
	private HUD hud; //script responsible for the HUD
	
	// #MOUSE#
	[HideInInspector] public bool isCameraMode = false;
	private float lastClick;
	private float clickCountdown;

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
			if ( GetWorldGravity() == TileOrientation.Down || GetWorldGravity() == TileOrientation.Up )
				transform.position = new Vector3( value.x, transform.position.y, value.z );
			else if ( GetWorldGravity() == TileOrientation.Right || GetWorldGravity() == TileOrientation.Left )
				transform.position = new Vector3( transform.position.x, value.y, value.z );
			else
				transform.position = new Vector3( value.x, value.y, transform.position.z );
		}
	}

	void Awake()
	{
		orientation = TileOrientation.Up;

		tilesLayer = LayerMask.NameToLayer ("Tiles");

		world = gameObject.AddComponent<World>() as World;
		World.Init( this );
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
		Assets.SetMouseCursor ();

		initSpawn();
        initHUD();
		initOrientationSpheres();
		checkUnderneath();
	}

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

	void FixedUpdate()
	{
		putDestinationMarks ();
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
		Texture2D tex = (Texture2D)Resources.Load("HUD/cameraCursor", typeof(Texture2D));
		Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
	}

	private IEnumerator SetNormalCursor()
	{
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
		GameObject dotsGroup = new GameObject();
		dotsGroup.hideFlags = HideFlags.HideInHierarchy;
		dotsGroup.name = "Orientation Dots";

		LayerMask layer = LayerMask.NameToLayer ("Player");

		for ( int i = 0, l = orientationSpheres.Length; i < l; i++ )
		{
			GameObject orientationSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			orientationSphere.name = "dot " + i;
			orientationSphere.layer = layer;
			orientationSphere.transform.parent = dotsGroup.transform;
			orientationSphere.transform.localScale = Vector3.one * hud.dotSize;
			Renderer oRenderer = orientationSphere.GetComponent<Renderer>();
			oRenderer.material = Assets.getSphereMat();
			oRenderer.material.color = hud.dotColor;
			oRenderer.receiveShadows = false;
			oRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			orientationSphere.GetComponent<Collider>().enabled = false;

			orientationSpheres[i] = orientationSphere;
		}
	}
	
	public void respawn(TileOrientation startingOrientation)
	{
		path = null;
		pawnTile = null;
		clickedTile = null;

		animState = 2;
		isFalling = true;
		isJumping = false;
		isWalking = false;
		
		transform.position = spawnPosition;
		transform.rotation = spawnRotation;

		orientation = startingOrientation;
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

			// Snap to the tile
			pawnTile = collision.collider.gameObject.GetComponent<Tile>();

			moveMe ( pawnTile.transform.position );

			if ( path != null && path.Count == 0 )
				putDestinationMarks();
		}

		if (collision.relativeVelocity.magnitude > 1 && GetComponent<AudioSource>().enabled)
			GetComponent<AudioSource>().Play();

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
			if (pawnTile != null && pawnTile.type.Equals(TileType.Exit)) //Has the player reached an exit Tile?
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
			if( pawnTile.type.Equals(TileType.Exit) ) //if this tile is an exit tile, make the game end
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
			Vector3 vec = nextTile.transform.position - getGroundPosition();
			
            if (moveMe(vec))
			{
				newTarget = true;
				position = nextTile.transform.position;

				path.RemoveAt(0); // if we have reached this path point, delete it from the list so we can go to the next one next time

				if ( isLeavingGlueTile )
				{
					// Remove the glue effect
					isGlued = false;
					isLeavingGlueTile = false;
					tileGravityVector = Physics.gravity.normalized;

					pawnTile = null;
					clickedTile = null;

					path.Clear();

					StartCoroutine( DelayedPawnFall ( GetWorldGravity() ));
				}
			}
			
			if ( path.Count == 0 )
			{
				animState = 0;
				isWalking = false;
				putDestinationMarks();

				ResetDynamic ();
			}
			
        }
        else if (clickedTile != null) // Case where there is no path but a target tile, ie: target tile is not aligned to tile
		{
			if ( clickedTile == pawnTile )
				clickedTile = null;
			else
			{
	        	// tile is not accessible but in valid space, so:
	        	// pawn will go towards the tile, then
	        	//  (- he will land on a neighbourg tile) -- no more possible yet
	        	//  - he will land on the tile
	        	//  (- he will fall into the void) -- this case won't happen anymore
				
				// [TODO] It seems bad:
				// The pawn is glued to a tile, not the gravity,
				// so it can't jump on another tile, gravity
				// would make it fall !
				Vector3 down = Physics.gravity.normalized;
				
				if (isGlued)
					down = tileGravityVector;

				// nearest tile: the directly accessible tile from the tile bellow the Pawn, thats closest to the target tile
				Tile nearest = Tile.Closest(pawnTile.AllAccessibleTiles(), clickedTile.transform.position);

				if (nearest.Equals(pawnTile)) //is the nearest the one bellow the Pawn?
				{
					// landing tile: the directly accessible tile from the target tile, thats closest to the nearest tile
					Tile landing = Tile.Closest(clickedTile.AllAccessibleTiles(), nearest.transform.position);
					
	                // check if landing tile is not EXACTLY under the nearest tile
					if ( Mathf.Abs( Vector3.Scale ( ( landing.transform.position - nearest.transform.position ), Vector3.Scale ( down, down ) - Vector3.one ).magnitude ) < .1f )
					{
						landing = clickedTile;
						
						//Debug.Log("landing: " + landing.transform.parent.name, landing.transform.parent );
					}

					// calculate the vector from the Pawns position to the landing tile position at the same height
	                Vector3 vec = getGroundHeightVector(landing.transform.position) - getGroundPosition();

					// There is definitely a tile to fall, jump and go
					if ( vec != Vector3.zero )
					{
						if ( !isJumping )
						{
							animState = 2;
							isJumping = true;
							isFalling = true;
							StartCoroutine( JumpToTile());
							
							if( lookCoroutine != null )
								StopCoroutine( lookCoroutine );

							lookCoroutine = LookAt ( landing.transform.position );
							StartCoroutine( lookCoroutine );
						}

						if (moveMe(vec)) // move the pawn towards that vector
						{
							path = AStarHelper.Calculate(landing, clickedTile); //give me a path towards the clicked tile
							clickedTile = null; // target reached, forget it
							isJumping = false;
							putDestinationMarks();
						}
					}
					else
					{
						//path = AStarHelper.Calculate(nearest, clickedTile); //give me a path towards the clicked tile
						clickedTile = null;
					}
	            }
	            else
				{
					path = AStarHelper.Calculate(pawnTile, nearest); //give me a path towards the nearest tile
	            }
			}
	    }
    }

	private IEnumerator JumpToTile()
	{
		float elapsedTime = 0;

		Vector3 jumpPos = Vector3.zero;

		Vector3 down = Physics.gravity.normalized;
		
		if (isGlued)
			down = tileGravityVector;

		while ( elapsedTime < jumpAnimationLength )
		{
			float t = elapsedTime / jumpAnimationLength;

			jumpPos = transform.position - (down * Mathf.Cos( t ) * 0.75f );
			transform.position = jumpPos;

			elapsedTime += Time.deltaTime;

			yield return null;
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
		if ( vec.magnitude > 1 )
		{
			Vector3 translate = Vector3.ClampMagnitude ( ( vec.normalized * Time.deltaTime * speed ), maxTranslation );
			transform.Translate( translate, Space.World );
            return false;
        }
        else
		{
			transform.Translate( vec * Time.deltaTime * speed, Space.World );
            return true;
        }
	}

	private IEnumerator LookAt( Vector3 point )
	{
		Vector3 down = Physics.gravity.normalized;
		
		if (isGlued)
			down = tileGravityVector;

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
		
		Vector3 down = Physics.gravity.normalized;
		
		if (isGlued)
			down = tileGravityVector;

		RaycastHit hit = new RaycastHit();

		// casting a ray down, we need a sphereCast because the capsule has thickness, and we only need tiles layer
		if (Physics.SphereCast (transform.position, width * 0.4f, down, out hit, height * 0.5f, (1 << tilesLayer)))
		{
			GameObject hitTile = hit.collider.gameObject;

			pawnTile = hitTile.GetComponent<Tile> ();
			orientation = pawnTile.orientation;

			isWalkingInStairs = false;
			
			if (hitTile.tag == "Stairway")
			{
				animState = 1;
				isWalkingInStairs = true;
			}

			if (hitTile.tag == "MovingPlatform")
			{
				// [TODO]
				// It would be nice that pawn's speed would
				// be relative to the moving platforms'

				// dirty snapping to a moving tile
				// actually, a mask is used to mix up both player & tile positions
				Vector3 _vec = Vector3.zero;

				// the gravity
				Vector3 _g = Vector3.Scale (down, down);

				if (path.Count > 0) {
					// the player asked the pawn to move

					_vec = path[0].transform.position - getGroundPosition ();
					_vec = _vec - Vector3.Scale (_g, _vec);

					if (_vec.x != 0)
						_vec.x = 1;
					if (_vec.y != 0)
						_vec.y = 1;
					if (_vec.z != 0)
						_vec.z = 1;
				}

				// _ppos: tile position 
				Vector3 _ppos = pawnTile.transform.position - Vector3.Scale (_g, pawnTile.transform.position);

				// first mask: actual gravity
				Vector3 _mask = _g;

				// second mask: playerPawn's movement
				_mask = _mask - _vec;

				// get absolute value
				_mask = Vector3.Scale (_mask, _mask);

				// mask the pawn's position
				_vec = Vector3.Scale (_mask, transform.position);

				// revert the mask
				_mask = - (_mask - Vector3.one);
				
				// mask the tile's position
				_ppos = Vector3.Scale (_mask, _ppos);

				// compute the new position
				position = _vec + _ppos;
			}
		}
		else if ( !isGlued )
		{
			// [TODO]
			// Of course, if the pawn is glued, we should still
			// check if there is something under
			pawnTile = null;
        }
	}

	private void removeDestinationMarks()
	{
		foreach (TileOrientation orientation in Enum.GetValues(typeof(TileOrientation)))
		{
			if ((int)orientation == 0)
				continue;

			getOrientationSphere (orientation).transform.position = Vector3.one * float.MaxValue; //sphere is moved to infinity muhahahaha, tremble before my power

		}
	}

	private void putDestinationMarks()
	{
		if ( isWalking || isWalkingInStairs || isFalling || isJumping )
			return;

		removeDestinationMarks ();

		foreach (TileOrientation orientation in Enum.GetValues(typeof(TileOrientation)))
		{
			if ( (int) orientation == 0 )
				continue;

			RaycastHit hit = new RaycastHit ();

			// Casting a ray towards 'orientation', SphereCast needed because of Pawn's capsule thickness and ignoring Pawn's collider
			if (Physics.SphereCast (transform.position, width * 0.4f, World.getGravityVector (orientation).normalized, out hit, 10000, (1 << tilesLayer)))
			{
				Tile tile = hit.collider.gameObject.GetComponent<Tile> ();
				
				if ( tile != null && tile != pawnTile && TileSelection.isClickableType( tile.type ) )
				{
					//if ( TileSelection.isClickableType( tile.type ) && !clickableTiles.Contains (tile) )
					tile.isClickable = true;

					if ( !clickableTiles.Contains (tile) )
						clickableTiles.Add (tile);

					if (hud.dotIsInside)
						getOrientationSphere (orientation).transform.position = tile.transform.position;
					else
						getOrientationSphere (orientation).transform.position = tile.transform.position - (World.getGravityVector (GetWorldGravity ()) * hud.dotSize * .5f );
				}
				else
				{
					//tile.isClickable = false;
					getOrientationSphere(orientation).transform.position = Vector3.one * float.MaxValue; // dot is moved to infinity
				}
			}
		}
	}
	
	/// <summary>
	/// Get the tile focused by cursor, if valid.
	/// </summary>
	private Tile getCursorTile()
	{
		Tile tile = TileSelection.getTile();

		// Cursor tile is null
		if ( tile == null )
			return null;
		
		// Cursor tile is unvalid type
		if ( !TileSelection.isClickableType( tile.type ) )
			return null;
		
		//  Pawn's tile is null or same as cursor's
		if ( pawnTile == null || tile == pawnTile )
			return null;

		if ( focusedTile != null )
		{
			// The player tile is a moving platform, force a total recheck
			if ( pawnTile.tag == "MovingPlatform" )
			{
				focusedTile.isClickable = false;
				focusedTile = null;
			}
			// The previous tile was a moving platform, force a recheck
			else if ( focusedTile.tag == "MovingPlatform" )
			{
				focusedTile = null;
				tile.isClickable = false;
				pawnTile.isClickable = false;

				return tile;
			}
			
			// The focused tile didn't changed
			else if ( tile == focusedTile )
				return focusedTile;
		}

		//Debug.Log( "Ok, tile is accessible, valid type, etc. ! Let's work !" );

		focusedTile = tile;

		List<Tile> accessibleTiles = AStarHelper.Calculate( pawnTile, tile );
		
		if ( accessibleTiles != null && accessibleTiles.Count > 0 )
		{
			foreach ( Tile accessibleTile in accessibleTiles )
			{
				// Keep only valid types
				if ( TileSelection.isClickableType( accessibleTile.type ) )
					clickableTiles.Add( accessibleTile );
			}
			
			tile.isClickable = true;
			tile.highlight();
		}
		
		// Check if the tile is accessible "by fall"
		else if ( tile.orientation == pawnTile.orientation )
		{
			bool isAccessibleByFall = !tileIsAbove( tile );

			// iff (if and only if)
			if ( isAccessibleByFall && ( pawnTile.orientation == TileOrientation.Down || pawnTile.orientation == TileOrientation.Up ) )
			{
				float xDist = Mathf.Abs( pawnTile.transform.position.x - tile.transform.position.x );
				float zDist = Mathf.Abs( pawnTile.transform.position.z - tile.transform.position.z );
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
				float yDist = Mathf.Abs( pawnTile.transform.position.y - tile.transform.position.y );
				float zDist = Mathf.Abs( pawnTile.transform.position.z - tile.transform.position.z );
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
				float xDist = Mathf.Abs( pawnTile.transform.position.x - tile.transform.position.x );
				float yDist = Mathf.Abs( pawnTile.transform.position.y - tile.transform.position.y );
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
			
			if ( isAccessibleByFall )
			{
				clickableTiles.Add( tile );
				
				if ( TileSelection.isClickableType( tile.type ) )
				{
					tile.isClickable = true;
					tile.highlight();
				}
			}
		}

		// Pawn tile is never clickable
		pawnTile.isClickable = false;

		return tile;
	}

    /// <summary>
    /// Manages the interaction with the mouse.
    /// </summary>
    private void manageMouse()
	{
		if( !isCameraMode )
		{
			if (isFalling || isJumping || (path != null && path.Count > 0))
				return;
			
			Tile tile = getCursorTile();

	        if (Input.GetMouseButton(0))
			{
				if(Time.time - lastClick < .1)
					clickCountdown += Time.deltaTime;
				else
					clickCountdown = 0;

	            lastClick = Time.time;

				if(clickCountdown > .25f)
				{
					isCameraMode = true;
					StartCoroutine(SetCameraCursor());
				}
	        }

			if (Input.GetMouseButtonUp(0) && clickCountdown > .25f)
			{
				StartCoroutine(SetNormalCursor());
				isCameraMode = false;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				clickCountdown = 0;

				if ( isWalking || isFalling || tile == null || world.FallingCubes() )
					return;

				if (tile.isClickable)
				{
					removeDestinationMarks();
					ClearClickableTiles();

					focusedTile = null;
					
					// If the player clicked a tile with different orientation
					if ( tile.orientation != pawnTile.orientation )
					{
						// If the pawn is on a glue tile
						// We only consider the click if the gravity change
						if ( isGlued && World.getGravityVector( tile.orientation ) != Physics.gravity.normalized )
						{
							hud.gravityChangeCount++;

							GetComponent<Rigidbody>().useGravity = false;
							tileGravityVector = World.getGravityVector( pawnTile.orientation );
							
							World.SetGravity( tile.orientation );
							world.ChangeGravity ( tile.orientation );
						}
						else //for punishing gravity take the tile == null here
						{
							hud.gravityChangeCount++;
							pawnTile = null;
							StartCoroutine( DelayedPawnFall ( tile.orientation ));
						}
					}
					else
					{
						clickedTile = tile;
						path = AStarHelper.Calculate(pawnTile, clickedTile);
					}
	            }
	        }
		}
		else if (Input.GetMouseButtonUp(0))
		{
			StartCoroutine(SetNormalCursor());
			isCameraMode = false;
		}
	}

	private void ClearClickableTiles()
	{
		// clear the "clickable tiles"
		for ( int i = 0, l = clickableTiles.Count; i < l; i++ )
			clickableTiles[i].isClickable = false;

		clickableTiles.Clear ();
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

		World.SetGravity( orientation );
		world.ChangeGravity ( orientation );
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

	public TileOrientation GetWorldGravity()
	{
		Vector3 down = Physics.gravity.normalized;
		
		if (isGlued)
			down = tileGravityVector;
		
		if (down.x > 0)
			return TileOrientation.Right;
		if (down.x < 0)
			return TileOrientation.Left;
		if (down.y > 0)
			return TileOrientation.Down;
		if (down.y < 0)
			return TileOrientation.Up;
		if (down.z > 0)
			return TileOrientation.Front;
		if (down.z < 0)
			return TileOrientation.Back;

		return TileOrientation.Up;
	}

	/// ----- CHECKERS ----- ///
	/// 
	
	/// <summary>
	/// Checks if the pawn is grounded.
	/// Answers the question " is the player touching a tile "beneath" him?" where beneath relates to the current gravitational orientation.
	/// </summary>
	private bool isGrounded()
	{
		//is there even a tile beneath the Pawn
		if (pawnTile != null) 
		{
			Vector3 down = Physics.gravity.normalized;
			
			if ( isGlued )
				down = tileGravityVector;

			return Physics.SphereCast( new Ray( transform.position, down ), width * 0.5f, height * 0.5f, (1 << tilesLayer) );
		}
		
		return false; // if there isn't a tile beneath him, he isn't grounded
	}


	/// <summary>
	/// Is the target tile above the Pawn?
	/// </summary>
	private bool tileIsAbove(Tile target)
	{
		switch (pawnTile.orientation)
        {
            default:
				return pawnTile.transform.position.y < target.transform.position.y;
            case TileOrientation.Down:
				return pawnTile.transform.position.y > target.transform.position.y;
			case TileOrientation.Left:
				return pawnTile.transform.position.x < target.transform.position.x;
			case TileOrientation.Right:
				return pawnTile.transform.position.x > target.transform.position.x;
            case TileOrientation.Front:
				return pawnTile.transform.position.z > target.transform.position.z;
            case TileOrientation.Back:
				return pawnTile.transform.position.z < target.transform.position.z;
        }
	}

    /// ----- GETTERS ----- ///

    /// <summary>
    /// Gets the player ground position, i.e. the position of the "feet" of the Pawn, pawn has 8 height
    /// </summary>
    private Vector3 getGroundPosition()
	{
		float n = (transform.localScale.y * height) *.5f;

		switch (GetWorldGravity())
		{
		default:
			return new Vector3 (transform.position.x, transform.position.y - n, transform.position.z);
		//case TileOrientation.Up:
		//	return new Vector3 (transform.position.x, transform.position.y - n, transform.position.z);
		case TileOrientation.Down:
			return new Vector3 (transform.position.x, transform.position.y + n, transform.position.z);
		case TileOrientation.Left:
			return new Vector3 (transform.position.x - n, transform.position.y, transform.position.z);
		case TileOrientation.Right:
			return new Vector3 (transform.position.x + n, transform.position.y, transform.position.z);
		case TileOrientation.Front:
			return new Vector3 (transform.position.x, transform.position.y, transform.position.z + n);
		case TileOrientation.Back:
			return new Vector3 (transform.position.x, transform.position.y, transform.position.z - n);
		}
    }



    /// <summary>
    /// Gets the ground height vector for the target position.
    /// </summary>
    /// <param name="position">Position of something</param>
    /// <returns>The position of something at the same height as the Pawn</returns>
    private Vector3 getGroundHeightVector(Vector3 position)
	{
		if (pawnTile.orientation.Equals(TileOrientation.Up) || pawnTile.orientation.Equals(TileOrientation.Down))
            return new Vector3(position.x, getGroundPosition().y, position.z);
		else if (pawnTile.orientation.Equals(TileOrientation.Left) || pawnTile.orientation.Equals(TileOrientation.Right))
            return new Vector3(getGroundPosition().x, position.y, position.z);
        else
            return new Vector3(position.x, position.y, getGroundPosition().z);
	}

    /// <summary>
    /// Gets the game object of the sphere for a given TileOrientation
    /// </summary>
    private GameObject getOrientationSphere(TileOrientation orientation)
    {
		return orientationSpheres [(int)orientation - 1];
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
