using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Monobehaviour class responsible for the player's pawn logic.
/// It computes all the movement logic, and drive the animator by triggering
/// state changes (but let the animator states performs visual stuff).
/// This class also manage the input, like tap/click and mouse moves.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Pawn : MonoBehaviour
{
	public static readonly int ANIM_IDLE_TRIGGER = Animator.StringToHash("Idle");
	private static readonly int ANIM_WALK_TRIGGER = Animator.StringToHash("Walk");
	private static readonly int ANIM_JUMP_TO_TILE_TRIGGER = Animator.StringToHash("Jump to Tile");
	private static readonly int ANIM_BORDER_DIRECTION_INT = Animator.StringToHash("Border Direction");
	private static readonly int ANIM_FALL_TRIGGER = Animator.StringToHash("Fall");
	private static readonly int ANIM_LAND_TRIGGER = Animator.StringToHash("Land");

	private static Pawn s_Instance = null;
	public static Pawn Instance { get { return s_Instance; } }

	/// <summary>
	/// This enum is used to set a relative direction of the border, relative to the pawn direction
	/// for helping the Animator to choose the right animation to play.
	/// For example if the pawn is facing the border he wants to jump to, then the BorderDirection
	/// should be FACE, and if the border is on his right, it should be right, etc...
	/// </summary>
	private enum BorderDirection
	{
		FRONT = 0,
		RIGHT,
		BACK,
		LEFT,
	}

	// #PAWN#
	public float speed = 30.0f;					// Speed of the pawn
	public float maxTranslation = 2.5f;			// Max translation of the pawn
	public float turnDelay = .5f;				// Time of pawn's rotation
	public float fallDelay = .5f;				// Time of pawn's fall
	public float fallInterval = .5f;			// Gap between tile and pawn before fall
	public float jumpAnimationLength = 0.3f;

	private float height = 1f;					// will be init in Awake from the capsule height
	private float width = 1f;					// will be init in Awake from the capsule width
	private bool newTarget = true;
	private Vector3 desiredRotation = Vector3.zero;
	private bool isWalking = false;
	private bool isWalkingInStairs = false;

	private int tilesLayer = 0;						// will be init in Awake
	private LayerMask tilesLayerMask;				// will be init in Awake
	private CapsuleCollider capsuleCollider = null; // will be init in Awake
	private Rigidbody rigidBody = null;             // will be init in Awake

	private bool isGlued = false;
	private bool isJumping = false;
	private bool isFalling = true;
	[HideInInspector] public RigidbodyConstraints nextConstraint = RigidbodyConstraints.None;
	private RigidbodyConstraints transformConstraints = RigidbodyConstraints.None;

	// #VFX#
	public ParticleSystem fallingVFX = null;

	// #ANIMATIONS#
	private IEnumerator lookCoroutine = null;
	private Animator animator = null;							  // will be init in Awake
	private RootMotionController m_RootMotionController = null;   // will be init in Awake
	private AnimStateJump m_AnimStateJump = null;                 // will be init in OnEnable or Start

	// #SPAWN#
	private Vector3 spawnPosition = Vector3.zero;			// position of the spawn GameObject
	private Quaternion spawnRotation = Quaternion.identity;	// rotation of the spawn GameObject
	
	// #TILES#
	private List<Tile> path = new List<Tile> (); // List of tiles in the current path
	private Tile pawnTile = null; // Tile beneath the Pawn
	private Tile clickedTile = null; // Tile the player clicked
	private Tile focusedTile = null; // Tile the cursor focus

	private bool IsThereAPath
	{
		get { return (path != null) && (path.Count > 0); }
	}

	// #MOUSE#
	private bool isCameraMode = false;
	private float clickCountdown = 0.0f;

	// #SPHERES#
	private List<Tile> clickableTilesToChangeGravity = new List<Tile>(6);

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

	#region Unity standard method
	void Awake()
	{
		s_Instance = this;

		// init the tile layers
		tilesLayer = LayerMask.NameToLayer ("Tiles");
		tilesLayerMask = LayerMask.GetMask(new string[]{"Tiles"});

		// get my animator and root motion controller
		animator = GetComponentInChildren<Animator>();
		m_RootMotionController = GetComponentInChildren<RootMotionController>();
		m_RootMotionController.ResetAllParameters(false); // disable it by default, the anim state will enable it if they need it

		// get my rigid body
		rigidBody = GetComponent<Rigidbody>();

		// get my collider
		capsuleCollider = GetComponent<CapsuleCollider>();
		height = capsuleCollider.height * capsuleCollider.transform.localScale.y;
		width = capsuleCollider.radius * capsuleCollider.transform.localScale.x;

		// Game cursor
		Assets.SetMouseCursor();

		// init the spawn position
		InitSpawn();
	}

	void Start()
	{
		// start the game
		World.Instance.GameStart();
	}

	private void OnEnable()
	{
		// according to Unity doc, you can only get the animator state behavior in Start() or OnEnable()
		// but the state are reinstantiated when the animator get disabled, so I prefer to get them here.
		m_AnimStateJump = animator.GetBehaviour<AnimStateJump>();
	}

	void Update()
	{
		if (!(World.Instance.IsGameOver() || HUD.Instance.IsPaused)) // is the game active?, i.e. is the game not paused and not finished?
		{
			ComputeFocusedAndClickableTiles();
			ManageMouse();
			MovePawn();
			CheckUnderneath();
		}
	}
	#endregion

	#region init and respawn
	/// <summary>
	/// Fetches the position of the spawn GameObject.
	/// Incase there is no spawn it will use the Pawn's initial position as spawnPoint
	/// </summary>
	private void InitSpawn()
	{
		GameObject spawn = GameObject.FindGameObjectWithTag("Spawn");
		spawnPosition = (spawn == null) ? transform.position : spawn.transform.position;
		spawnRotation = (spawn == null) ? transform.rotation : spawn.transform.rotation;
	}
	
	public void Respawn(TileOrientation startingOrientation)
	{
		path = null;
		clickedTile = null;
		focusedTile = null;

		animator.ResetTrigger(ANIM_IDLE_TRIGGER);
		animator.ResetTrigger(ANIM_WALK_TRIGGER);
		animator.ResetTrigger(ANIM_LAND_TRIGGER);
		animator.SetTrigger(ANIM_FALL_TRIGGER);
		isFalling = true;
		isJumping = false;
		isWalking = false;

		// teleport the pawn at the spawn position
		transform.position = spawnPosition;
		transform.rotation = spawnRotation;

		// reset my root controller
		m_RootMotionController.ResetAllParameters(false);

		// please teleport the pawn first before reseting the pawn tile
		OnEnterTile(null);

		ResetDynamic();

		rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		capsuleCollider.enabled = true;

		StartCoroutine( DelayedReset ());
	}
	
	private IEnumerator DelayedReset()
	{
		yield return new WaitForSeconds(0.1f);

		rigidBody.constraints = RigidbodyConstraints.FreezeRotation & ~RigidbodyConstraints.FreezePositionY;
	}
	
	private void ResetDynamic()
	{
		rigidBody.velocity = Vector3.zero;
		rigidBody.angularVelocity = Vector3.zero;
		
		//World.SetGravity (GetWorldGravity());
	}
	#endregion

	#region collision management
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
					Crush();
			}
			else if (pos.y != 0)
			{
				if (pos.y > _pos.y)
					Crush();
			}
			else if (pos.z != 0)
			{
				if (pos.z > _pos.z)
					Crush();
			}
		}
		else // gravité supérieur à 0, le cube doit etre au dessous
		{
			if (pos.x != 0)
			{
				if (pos.x < _pos.x)
					Crush();
			}
			else if (pos.y != 0)
			{
				if (pos.y < _pos.y)
					Crush();
			}
			else if (pos.z != 0)
			{
				if (pos.z < _pos.z)
					Crush();
			}
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.layer != tilesLayer)
			return;

		if (isFalling)
		{
			animator.ResetTrigger(ANIM_FALL_TRIGGER);
			animator.SetTrigger(ANIM_LAND_TRIGGER);
			isFalling = false;
			isJumping = false;

			// stop the vfx if any
			// if there's some falling sfx, start them
			if (fallingVFX != null)
				fallingVFX.Stop();			

			// Snap to the tile
			OnEnterTile(collision.collider.gameObject.GetComponent<Tile>());

			MoveTo( pawnTile.transform.position );
		}

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
	/// <param name="useGravity">This parameter will be used to set the UseGravity of the rigid body, except if the tile entered is glued, in that case the use gravity will always be set to false.</param>
	public void OnEnterTile(Tile tile, bool useGravity = true)
	{
		// save the new pawntile (can be null)
		pawnTile = tile;

		// and now check stuff related with glue, and set the glue flag
		if ((tile != null) && tile.IsGlueTile)
		{
			isGlued = true;
			rigidBody.useGravity = false;
		}
		else
		{
			isGlued = false;
			rigidBody.useGravity = useGravity;
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
		this.transform.parent = shouldAttachToTile ? tile.transform : null;
		// and notify my root motion controller
		m_RootMotionController.NotifyGrandParentAttachementChange();
	}
	#endregion

	#region move
	/// <summary>
	///  Moves the pawn.
	///  Applies player requested movement and gravity.
	/// </summary>
	private void MovePawn()
	{
		if (IsGrounded()) // is the player touching a tile "beneath" him?
		{
			if (pawnTile.Type.Equals(TileType.Exit)) //if this tile is an exit tile, make the game end
				World.Instance.GameOver(true);

			MoveAlongPath(); //otherwise, move along the path to the player selected tile
		}
		else if (isWalkingInStairs || isJumping)
		{
			MoveAlongPath();
		}
    }
	
    /// <summary>
    /// Moves along the path.
    /// If there is a path that has been previously decided the Pawn should take, finish the path.
    /// Otherwise, if there isn't a path, but there is a clickedTile, a valid tile that the player has clicked. 
    /// This clickedTile can be in a place not directly accessible to the Pawn, for example if there is a gap or if the tile is lower and the Pawn is supposed to fall.
    /// </summary>
    private void MoveAlongPath()
	{
		if (IsThereAPath)
		{
			Tile nextTile = path[0];

			// set the walking flag
			isWalking = true;

			// look at the new target if any
			if (newTarget)
				StartToLookAt(nextTile.transform.position);
			
            // if there is, move the pawn towards the next point in that path
			if (MoveTo(nextTile.transform.position))
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
						StartToWalkToTile(path[0], clickedTile);
						// if the path is broken, clear the click tile to avoid the pawn to try to jump on it
						if (path == null)
							clickedTile = null;
					}
				}
			}

			// path can be null because we may have recompute it if we are on a moving platform
			if (!IsThereAPath)
			{
				animator.SetTrigger(ANIM_IDLE_TRIGGER);
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
					isJumping = true;

					// compute the height (in grid step) between the pawn tile and the clicked tile
					int tileRelativeGridHeight = World.Instance.GetTileRelativeGridHeight(pawnTile, clickedTile, GetFeltVerticality());

					// compute the border direction and set it in any case (both type of animation needs it)
					animator.SetInteger(ANIM_BORDER_DIRECTION_INT, (int)GetBorderDirectionToGoToThisTile(clickedTile));

					// if the height is just one step, the pawn will jump, otherwise he will use his rope
					if (tileRelativeGridHeight == 1)
					{
						isFalling = false;

						// disable the collider during this animation
						capsuleCollider.enabled = false;

						// set the parameters to the anim state jump
						m_AnimStateJump.SetStartAndEndTile(pawnTile, clickedTile);

						// the tile is just under me, we just do a simple jump
						animator.SetTrigger(ANIM_JUMP_TO_TILE_TRIGGER);
					}
					else
					{
						isFalling = true;

						// the tile is too low under me, trigger the jump with rope
						animator.SetTrigger(ANIM_FALL_TRIGGER);

						// the modification in height
						StartCoroutine(JumpToTile());

						// the modification in orientation
						StartToLookAt(clickedTile.transform.position);
					}

					// reset the pawn tile when starting to jump, because if you jump from
					// a moving platform, you don't want to jump relative to the plateform
					OnEnterTile(null, (tileRelativeGridHeight > 1));
				}

				// calculate the vector from the Pawns position to the landing tile position at the same height
				Vector3 landingPositionAtGroundHeight = GetGroundHeightPosition(clickedTile.transform.position);
				if (isFalling && MoveTo(landingPositionAtGroundHeight)) // move the pawn towards the landing tile
					OnJumpFinished();
			}
	    }
    }

    /// <summary>
    /// Moves the pawn to the specified destination.
    /// Answers the question "am I there yet?"
    /// </summary>
	/// <param name="destination">the destination where the player should be moved to in world coord</param>
    /// <returns>returns true if the vector is small, i.e. smaller than 1 of magnitude, in this case, the Pawn has reached his destination</returns>
    private bool MoveTo(Vector3 destination)
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

	/// <summary>
	/// This function will calculate a path between the specified start and goal tiles.
	/// This path will be saved in the "path" member.
	/// If there's any path, it will also trigger the walk animation.
	/// </summary>
	/// <param name="start">The tile from which the pawn start to walk</param>
	/// <param name="goal">The goal tile that the pawn tries to reach</param>
	private void StartToWalkToTile(Tile start, Tile goal)
	{
		// ask the path from the A*
		path = AStarHelper.Calculate(start, goal);

		// if a valid path is returned, trigger the walk anim
		if (IsThereAPath)
			animator.SetTrigger(ANIM_WALK_TRIGGER);
		else
			animator.SetTrigger(ANIM_IDLE_TRIGGER);
	}

	private void StartToLookAt(Vector3 point)
	{
		// stop the look coroutine if it is already running
		if (lookCoroutine != null)
			StopCoroutine(lookCoroutine);

		// start the new look at coroutine
		lookCoroutine = LookAt(point);
		StartCoroutine(lookCoroutine);
	}

	private IEnumerator LookAt(Vector3 point)
	{
		Vector3 down = GetMyVerticality();

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
    private void CheckUnderneath()
	{
		if (isJumping || isFalling)
			return;
		
		Vector3 down = GetMyVerticality();

		RaycastHit hit = new RaycastHit();

		// casting a ray down, we need a sphereCast because the capsule has thickness, and we only need tiles layer
		if (Physics.SphereCast (transform.position, width * 0.4f, down, out hit, height * 0.5f, tilesLayerMask))
		{
			GameObject hitTileGameObject = hit.collider.gameObject;
			Tile hitTile = hitTileGameObject.GetComponent<Tile>();

			// if the pawn change the tile, call the notification
			if (hitTile != pawnTile)
				OnEnterTile(hitTile);

			// check if we are on the stairs
			isWalkingInStairs = (hitTileGameObject.tag == "Stairway");
		}
		else
		{
			OnEnterTile(null);
        }
	}
	#endregion

	#region jump and fall
	private IEnumerator JumpToTile()
	{
		float elapsedTime = 0;
		Vector3 up = new Vector3(0f, 0.25f, 0f);

		while (elapsedTime < jumpAnimationLength)
		{
			float t = elapsedTime / jumpAnimationLength;

			transform.Translate(up * Mathf.Cos(t), Space.Self);

			elapsedTime += Time.deltaTime;

			yield return null;
		}
	}

	/// <summary>
	/// This callback is called by the animator state machine when all the animations are finished after a jump.
	/// </summary>
	public void OnJumpFinished()
	{
		clickedTile = null; // target reached, forget it
		isJumping = false;
		// reenable the collider
		capsuleCollider.enabled = true;
	}

	private IEnumerator DelayedPawnFall(TileOrientation orientation)
	{
		Vector3 desiredPosition = new Vector3(0, height * 0.5f * fallInterval, 0);

		// Block the "manageMouse" loop
		isFalling = true;

		// if there's some falling sfx, start them
		if (fallingVFX != null)
			fallingVFX.Play();

		//		collider.gameObject.layer = 12;
		nextConstraint = rigidBody.constraints;

		ResetDynamic();
		rigidBody.useGravity = false;
		rigidBody.constraints = transformConstraints;

		float timer = .0f;
		float delay = fallDelay * 0.5f;

		// Make the pawn float in the airs a little
		while (timer < delay)
		{
			timer += Time.deltaTime;
			Vector3 toPos = desiredPosition * timer / delay;
			desiredPosition = desiredPosition - toPos;
			transform.Translate(toPos, Space.Self);
			yield return null;
		}

		SetPawnOrientation(orientation);

		Quaternion fromRot = transform.rotation;
		Quaternion toRot = Quaternion.Euler(desiredRotation);

		timer = .0f;

		// Rotate the pawn in order to face the correct direction
		while (timer < delay)
		{
			timer += Time.deltaTime;
			transform.rotation = Quaternion.Lerp(fromRot, toRot, timer / delay);
			yield return null;
		}

		transform.rotation = toRot;

		// Fall animation
		animator.SetTrigger(ANIM_FALL_TRIGGER);

		rigidBody.constraints = nextConstraint;
		rigidBody.useGravity = true;

		World.Instance.SetGravity(orientation);
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
	#endregion

	#region focused tile, clickable tile, and destination marks
	private void RemoveDestinationMarks()
	{
		// clear the flag on all the tiles
		foreach (Tile tile in clickableTilesToChangeGravity)
			tile.IsClickableToChangeGravity = false;

		// and clear the list
		clickableTilesToChangeGravity.Clear();
	}

	private bool PutDestinationMarks(Tile tileToCheck)
	{
		RemoveDestinationMarks();

		if ( isWalking || isWalkingInStairs || isFalling || isJumping )
			return false;

		bool result = false;
		TileOrientation currentWorldOrientation = GetWorldVerticality();

		// ray cast in the 6 direction of the world from the pawn position
		for (int i = 0 ; i < 6 ; ++i)
		{
			TileOrientation orientation = (TileOrientation)(i + 1);

			RaycastHit hit = new RaycastHit();

			// Casting a ray towards 'orientation', SphereCast needed because of Pawn's capsule thickness and ignoring Pawn's collider
			if (Physics.SphereCast(transform.position, width * 0.4f, World.GetGravityNormalizedVector(orientation), out hit, 10000, tilesLayerMask))
			{
				Tile tile = hit.collider.gameObject.GetComponent<Tile>();
				
				if ( (tile != null) && (tile.orientation != TileOrientation.None) && 
				    (tile.orientation != currentWorldOrientation) && TileSelection.isClickableType( tile.Type ) )
				{
					// check if the current tile equals the tile to check
					if (tile == tileToCheck)
						result = true;

					// make the tile clickable, and add it to the list
					tile.IsClickableToChangeGravity = true;
					clickableTilesToChangeGravity.Add(tile);
				}
			}
		}

		return result;
	}
	
	/// <summary>
	/// Compute and set the tile focused by cursor, if valid.
	/// Also clear and refill the array of clickable tiles.
	/// </summary>
	private void ComputeFocusedAndClickableTiles()
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
				bool isAccessibleByFall = IsTileBelow(focusedTile) && (pawnTile.orientation == GetWorldVerticality());

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
		bool canFocusedTileClhangeGravity = PutDestinationMarks(focusedTile);
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

	#region mouse management
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
	/// Manages the interaction with the mouse.
	/// </summary>
	private void ManageMouse()
	{
		// for very low framerate, we give at least 3 frames to switch to camera mode,
		// otherwise for normal framerate, we use a fixed value of 1 quarter of second.
		float durationToSwitchToCameraMode = Math.Max(0.25f, 3.0f * Time.deltaTime);

		// reset the click down duration if the button is up
		if (!InputManager.isClickHeldDown())
			clickCountdown = 0;

		if( !isCameraMode )
		{
			if (isFalling || isJumping || IsThereAPath)
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
					if ( isWalking || isFalling || focusedTile == null || World.Instance.IsThereAnyCubeFalling() )
						return;

					// if the focussed tile is highlighted that means it is clickable
					if (focusedTile.IsHighlighted)
					{
						// get the camera sound to play a 2D UI sound when clicking
						Camera2DSound sound = Camera.main.GetComponent<Camera2DSound>();

						// play the click sound, as the player clicked a tile
						sound.playSound(Camera2DSound.SoundId.CLICK_TILE);
							
						// If the player clicked a tile with different orientation
						if ( ( focusedTile.orientation != pawnTile.orientation ) || 
						     ( isGlued && (focusedTile == pawnTile)) )
						{
							// player has changed the gravity, increase the counter
							HUD.Instance.IncreaseGravityChangeCount();

							// play the gravity change sound)
							sound.playSound(Camera2DSound.SoundId.GRAVITY_CHANGE);
								
							// If the pawn is on a glue tile, the change of gravity is managed differently
							if ( isGlued )
							{
								World.Instance.SetGravity( focusedTile.orientation );
							}
							else
							{
								// asked the clicked tile to play it's attraction VFX
								focusedTile.playActivationVFX();
								//for punishing gravity take the tile == null here
								OnEnterTile(null);
								StartCoroutine( DelayedPawnFall ( focusedTile.orientation ));
							}
						}
						else
						{
							// memorised the clicked tile
							clickedTile = focusedTile;
							// ask a new path if we go to a different tile
							if (pawnTile != clickedTile)
								StartToWalkToTile(pawnTile, clickedTile);
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
	#endregion

	#region verticality and border direction
	private TileOrientation GetFeltVerticality()
	{
		return GetTileOrientationFromDownVector( GetMyVerticality() );
	}

	private TileOrientation GetWorldVerticality()
	{
		return GetTileOrientationFromDownVector( Physics.gravity );
	}

	private TileOrientation GetTileOrientationFromDownVector(Vector3 down)
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

	/// <summary>
	/// Gets my verticality, which equals to the gravity normally, but which can be different if the pawn is
	/// glued on a wall.
	/// </summary>
	/// <returns>The my verticality for the pawn.</returns>
	private Vector3 GetMyVerticality()
	{
		if (isGlued && (pawnTile != null))
			return pawnTile.getDownVector();
		else
			return Physics.gravity.normalized;
	}

	/// <summary>
	/// Compute the direction of the border of the current pawn tile, if the pawn
	/// needs to go to the specified tile.
	/// </summary>
	/// <param name="targetTile">The tile where the pawn wants to go.</param>
	/// <returns>a relative direction of the border, relative to the current orientation of the pawn</returns>
	private BorderDirection GetBorderDirectionToGoToThisTile(Tile targetTile)
	{
		// compute the position of the target tile in the pawn local coordinates
		Vector3 localTargetTilePosition = transform.worldToLocalMatrix * targetTile.Position;

		// now check if the target position if roughly in front, back, right or left
		if (localTargetTilePosition.z > 5f)
			return BorderDirection.FRONT;
		else if (localTargetTilePosition.z < -5f)
			return BorderDirection.BACK;
		else if (localTargetTilePosition.x > 5f)
			return BorderDirection.RIGHT;
		else
			return BorderDirection.LEFT;		
	}
	#endregion

	#region ground detection
	/// <summary>
	/// Checks if the pawn is grounded.
	/// Answers the question " is the player touching a tile "beneath" him?" where beneath relates to the current gravitational orientation.
	/// </summary>
	private bool IsGrounded()
	{
		//is there even a tile beneath the Pawn
		if (pawnTile != null) 
		{
			Vector3 down = GetMyVerticality();
			return Physics.SphereCast( new Ray( transform.position, down ), width * 0.5f, height * 0.5f, tilesLayerMask );
		}
		
		return false; // if there isn't a tile beneath him, he isn't grounded
	}

	/// <summary>
	/// Is the target tile above the Pawn?
	/// </summary>
	private bool IsTileBelow(Tile target)
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

    /// <summary>
    /// Gets the player ground position, i.e. the position of the "feet" of the Pawn, pawn has 8 height
    /// </summary>
    private Vector3 GetGroundPosition()
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
    private Vector3 GetGroundHeightPosition(Vector3 position)
	{
		TileOrientation pawnOrientation = GetFeltVerticality();

		if (pawnOrientation == TileOrientation.Up || pawnOrientation == TileOrientation.Down)
            return new Vector3(position.x, GetGroundPosition().y, position.z);
		else if (pawnOrientation == TileOrientation.Left || pawnOrientation == TileOrientation.Right)
            return new Vector3(GetGroundPosition().x, position.y, position.z);
        else
            return new Vector3(position.x, position.y, GetGroundPosition().z);
	}
	#endregion

	#region pawn death
	/// <summary>
	/// call this method when the player die by being crushed by a falling cube.
	/// This will trigger a game over
	/// </summary>
	private void Crush()
	{
		World.Instance.GameOver(false);
	}

	/// <summary>
	/// Call this method when the Pawn is out of bounds, i.e. leaves the game space.
	/// This will trigger a game over
	/// </summary>
	public void OutOfBounds()
	{
		World.Instance.GameOver(false);
	}

	/// <summary>
	/// Call this method when the Pawn falls on spikes.
	/// This will trigger a game over
	/// </summary>
	public void DieOnSpikes()
	{
		World.Instance.GameOver(false);
	}
	#endregion
}
