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
	public static readonly int ANIM_EXIT_STATE_TRIGGER = Animator.StringToHash("Exit State");
	public static readonly int ANIM_IDLE_TRIGGER = Animator.StringToHash("Idle");
	private static readonly int ANIM_WALK_TRIGGER = Animator.StringToHash("Walk");
	private static readonly int ANIM_FALL_TRIGGER = Animator.StringToHash("Fall");
	private static readonly int ANIM_FALL_AND_ABSEIL_ABOVE_TRIGGER = Animator.StringToHash("Fall and Abseil Above");
	private static readonly int ANIM_FALL_AND_ABSEIL_SIDEWAY_TRIGGER = Animator.StringToHash("Fall and Abseil Sideway");
	private static readonly int ANIM_LAND_TRIGGER = Animator.StringToHash("Land");
	public static readonly int ANIM_BORDER_DIRECTION_INT = Animator.StringToHash("Border Direction");
	public static readonly int ANIM_ABSEIL_HEIGHT_INT = Animator.StringToHash("Abseil Height");
	private static readonly int ANIM_JUMP_TO_TILE_TRIGGER = Animator.StringToHash("Jump to Tile");
	private static readonly int ANIM_JUMP_AND_ABSEIL_TRIGGER = Animator.StringToHash("Jump and Abseil");
	private static readonly int ANIM_ROLL_TO_TILE_TRIGGER = Animator.StringToHash("Roll to Tile");

	private static Pawn s_Instance = null;
	public static Pawn Instance { get { return s_Instance; } }

	/// <summary>
	/// This enum is used to set a relative direction of the border, relative to the pawn direction
	/// for helping the Animator to choose the right animation to play.
	/// For example if the pawn is facing the border he wants to jump to, then the BorderDirection
	/// should be FACE, and if the border is on his right, it should be right, etc...
	/// </summary>
	public enum BorderDirection
	{
		FRONT = 0,
		RIGHT,
		BACK,
		LEFT,
	}

	// Serialized parameters
	[SerializeField]
	[Tooltip("The speed of the pawn when he's walking.")]
	private float speed = 30.0f;

	[SerializeField]
	[Tooltip("The Max translation of the pawn when he's walking.")]
	private float maxTranslation = 2.5f;

	[SerializeField]
	[Tooltip("Time of pawn's rotation.")]
	private float turnDelay = .5f;				// Time of pawn's rotation

	// #COLLISION#
	private int m_TilesLayer = 0;						// will be init in Awake
	private LayerMask m_TilesLayerMask;					// will be init in Awake
	private CapsuleCollider m_CapsuleCollider = null;	// will be init in Awake
	private Rigidbody m_RigidBody = null;				// will be init in Awake
	private float m_Height = 1f;						// will be init in Awake from the capsule height
	private float m_Width = 1f;				            // will be init in Awake from the capsule width

	// #ANIMATIONS#
	private Animator m_Animator = null;							  // will be init in Awake
	private RootMotionController m_RootMotionController = null;   // will be init in Awake
	private AnimStateJumpToTile m_AnimStateJumpToTile = null;     // will be init in OnEnable or Start
	private AnimStateJumpAbseil m_AnimStateJumpAbseil = null;     // will be init in OnEnable or Start
	private AnimStateRollToTile m_AnimStateRollToTile = null;     // will be init in OnEnable or Start
	private AnimStateFallAbseilAbove m_AnimStateFallAbseilAbove = null; // will be init in OnEnable or Start
	private AnimStateFallAbseilSide m_AnimStateFallAbseilSide = null; // will be init in OnEnable or Start

	// #SPAWN#
	private Vector3 m_SpawnPosition = Vector3.zero;			// position of the spawn GameObject
	private Quaternion m_SpawnRotation = Quaternion.identity;   // rotation of the spawn GameObject

	// #TILES#
	private Tile m_PawnTile = null; // Tile beneath the Pawn
	private Tile m_ClickedTile = null; // Tile the player clicked
	private Tile m_FocusedTile = null; // Tile the cursor focus
	private List<Tile> m_ClickableTilesToChangeGravity = new List<Tile>(6);

	// #STATE FLAGS#
	private bool m_IsWalking = false;
	private bool m_IsWalkingInStairs = false;
	private bool m_IsGlued = false;
	private bool m_IsJumping = false;
	private bool m_IsFalling = true;

	// #WALK#
	private bool m_HasANewTargetDuringWalk = true;
	private IEnumerator m_LookAtCoroutine = null;
	private List<Tile> m_CurrentPath = new List<Tile>(); // List of tiles in the current path

	private bool IsThereAPath
	{
		get { return (m_CurrentPath != null) && (m_CurrentPath.Count > 0); }
	}

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
		m_TilesLayer = LayerMask.NameToLayer ("Tiles");
		m_TilesLayerMask = LayerMask.GetMask(new string[]{"Tiles"});

		// get my animator and root motion controller
		m_Animator = GetComponentInChildren<Animator>();
		m_RootMotionController = GetComponentInChildren<RootMotionController>();
		m_RootMotionController.ResetAllParameters(false); // disable it by default, the anim state will enable it if they need it

		// get my rigid body
		m_RigidBody = GetComponent<Rigidbody>();

		// get my collider
		m_CapsuleCollider = GetComponent<CapsuleCollider>();
		m_Height = m_CapsuleCollider.height * m_CapsuleCollider.transform.localScale.y;
		m_Width = m_CapsuleCollider.radius * m_CapsuleCollider.transform.localScale.x;

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
		m_AnimStateJumpToTile = m_Animator.GetBehaviour<AnimStateJumpToTile>();
		m_AnimStateJumpAbseil = m_Animator.GetBehaviour<AnimStateJumpAbseil>();
		m_AnimStateRollToTile = m_Animator.GetBehaviour<AnimStateRollToTile>();
		m_AnimStateFallAbseilAbove = m_Animator.GetBehaviour<AnimStateFallAbseilAbove>();
		m_AnimStateFallAbseilSide = m_Animator.GetBehaviour<AnimStateFallAbseilSide>();
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
		m_SpawnPosition = (spawn == null) ? transform.position : spawn.transform.position;
		m_SpawnRotation = (spawn == null) ? transform.rotation : spawn.transform.rotation;
	}
	
	public void Respawn(TileOrientation startingOrientation)
	{
		m_CurrentPath = null;
		m_ClickedTile = null;
		m_FocusedTile = null;

		m_Animator.ResetTrigger(ANIM_EXIT_STATE_TRIGGER);
		m_Animator.ResetTrigger(ANIM_IDLE_TRIGGER);
		m_Animator.ResetTrigger(ANIM_WALK_TRIGGER);
		m_Animator.ResetTrigger(ANIM_LAND_TRIGGER);
		m_Animator.ResetTrigger(ANIM_JUMP_TO_TILE_TRIGGER);
		m_Animator.ResetTrigger(ANIM_JUMP_AND_ABSEIL_TRIGGER);
		m_Animator.ResetTrigger(ANIM_ROLL_TO_TILE_TRIGGER);
		m_Animator.ResetTrigger(ANIM_FALL_AND_ABSEIL_ABOVE_TRIGGER);
		m_Animator.ResetTrigger(ANIM_FALL_AND_ABSEIL_SIDEWAY_TRIGGER);		
		m_Animator.SetTrigger(ANIM_FALL_TRIGGER);
		m_IsFalling = true;
		m_IsJumping = false;
		m_IsWalking = false;

		// teleport the pawn at the spawn position
		transform.position = m_SpawnPosition;
		transform.rotation = m_SpawnRotation;

		// reset my root controller
		m_RootMotionController.ResetAllParameters(false);

		// please teleport the pawn first before reseting the pawn tile
		OnEnterTile(null);

		ResetDynamic();

		m_RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		m_CapsuleCollider.enabled = true;

		StartCoroutine( DelayedReset ());
	}
	
	private IEnumerator DelayedReset()
	{
		yield return new WaitForSeconds(0.1f);

		m_RigidBody.constraints = RigidbodyConstraints.FreezeRotation & ~RigidbodyConstraints.FreezePositionY;
	}
	
	private void ResetDynamic()
	{
		m_RigidBody.velocity = Vector3.zero;
		m_RigidBody.angularVelocity = Vector3.zero;
		
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
		if (collision.collider.gameObject.layer != m_TilesLayer)
			return;

		if (m_IsFalling)
		{
			m_Animator.ResetTrigger(ANIM_FALL_TRIGGER);
			m_Animator.SetTrigger(ANIM_LAND_TRIGGER);
			m_IsFalling = false;
			m_IsJumping = false;

			// Snap to the tile
			OnEnterTile(collision.collider.gameObject.GetComponent<Tile>());

			MoveTo( m_PawnTile.transform.position );
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
		m_PawnTile = tile;

		// and now check stuff related with glue, and set the glue flag
		if ((tile != null) && tile.IsGlueTile)
		{
			m_IsGlued = true;
			m_RigidBody.useGravity = false;
		}
		else
		{
			m_IsGlued = false;
			m_RigidBody.useGravity = useGravity;
		}

		// if the tile is not null, check if we need to attach the pawn to the tile
		// after setting the glue flag!
		bool shouldAttachToTile = false;
		if (tile != null)
		{
			bool isWorldGravityLikePawnTile = (tile.orientation == this.GetWorldVerticality());

			// attach the pawn to the platform if is a moving platform with the gravity in the right direction
			// or if the pawn is glued to a moving or gravity platform
			shouldAttachToTile = ( (tile.tag == GameplayCube.MOVING_PLATFORM_TAG && (m_IsGlued || isWorldGravityLikePawnTile)) ||
			                      (tile.tag == GameplayCube.GRAVITY_PLATFORM_TAG && m_IsGlued) );
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
			if (m_PawnTile.Type.Equals(TileType.Exit)) //if this tile is an exit tile, make the game end
				World.Instance.GameOver(true);

			MoveAlongPath(); //otherwise, move along the path to the player selected tile
		}
		else if (m_IsWalkingInStairs || m_IsJumping)
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
			Tile nextTile = m_CurrentPath[0];

			// set the walking flag
			m_IsWalking = true;

			// look at the new target if any
			if (m_HasANewTargetDuringWalk)
				StartToLookAt(nextTile.transform.position);
			
            // if there is, move the pawn towards the next point in that path
			if (MoveTo(nextTile.transform.position))
			{
				m_HasANewTargetDuringWalk = true;
				position = nextTile.transform.position;

				m_CurrentPath.RemoveAt(0); // if we have reached this path point, delete it from the list so we can go to the next one next time

				// check if the remaining path involve a move between a moving platform and an non moving platform
				// because if some tile are moving and other not along the path, the path may become broken,
				// so recompute the path everytime we move on the next tile
				if ( m_CurrentPath.Count > 0 )
				{
					bool isStaticFound = false;
					bool isMovingFound = false;
					foreach (Tile pathTile in m_CurrentPath)
					{
						if (pathTile.tag == GameplayCube.MOVING_PLATFORM_TAG)
							isMovingFound = true;
						else
							isStaticFound = true;
						// if we found both, we can stop searching
						if (isMovingFound && isStaticFound)
							break;
					}

					// if we found a path with a passage between static and moving tile, recompute the path
					if (isMovingFound && isStaticFound && (m_ClickedTile != null))
					{
						StartToWalkToTile(m_CurrentPath[0], m_ClickedTile);
						// if the path is broken, clear the click tile to avoid the pawn to try to jump on it
						if (m_CurrentPath == null)
							m_ClickedTile = null;
					}
				}
			}

			// path can be null because we may have recompute it if we are on a moving platform
			if (!IsThereAPath)
				m_Animator.SetTrigger(ANIM_IDLE_TRIGGER);
		}
        else if (m_ClickedTile != null) // Case where there is no path but a target tile, ie: target tile is not aligned to tile
		{
			if ( m_ClickedTile == m_PawnTile )
			{
				m_ClickedTile = null;
			}
			else
			{
				// tile is not accessible but in valid space, so that means the pawn will jump on the tile
				// normally my verticality is the same as the world gravity, otherwise we won't have a clicked tile
				Debug.Assert(GetFeltVerticality() == GetWorldVerticality());

				// check if we didn't start jumping yet
				if (!m_IsJumping)
					StartToJump(m_ClickedTile);

				// calculate the vector from the Pawns position to the landing tile position at the same height
				Vector3 landingPositionAtGroundHeight = GetGroundHeightPosition(m_ClickedTile.transform.position);
				if (m_IsFalling && MoveTo(landingPositionAtGroundHeight)) // move the pawn towards the landing tile
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
		moveDirection.y += m_Height * 0.5f;

		if ( moveDirection.magnitude > 0.1f )
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
		m_CurrentPath = AStarHelper.Calculate(start, goal);

		// if a valid path is returned, trigger the walk anim
		if (IsThereAPath)
			m_Animator.SetTrigger(ANIM_WALK_TRIGGER);
	}

	public void OnWalkToTileFinished()
	{
		m_IsWalking = false;
		ResetDynamic();
	}

	private void StartToLookAt(Vector3 point)
	{
		// stop the look coroutine if it is already running
		if (m_LookAtCoroutine != null)
			StopCoroutine(m_LookAtCoroutine);

		// start the new look at coroutine
		m_LookAtCoroutine = LookAt(point);
		StartCoroutine(m_LookAtCoroutine);
	}

	private IEnumerator LookAt(Vector3 point)
	{
		Vector3 down = GetMyVerticality();

		m_HasANewTargetDuringWalk = false;
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
		if (m_IsJumping || m_IsFalling)
			return;
		
		Vector3 down = GetMyVerticality();

		RaycastHit hit = new RaycastHit();

		// casting a ray down, we need a sphereCast because the capsule has thickness, and we only need tiles layer
		if (Physics.SphereCast (transform.position, m_Width * 0.4f, down, out hit, m_Height * 0.5f, m_TilesLayerMask))
		{
			GameObject hitTileGameObject = hit.collider.gameObject;
			Tile hitTile = hitTileGameObject.GetComponent<Tile>();

			// if the pawn change the tile, call the notification
			if (hitTile != m_PawnTile)
				OnEnterTile(hitTile);

			// check if we are on the stairs
			m_IsWalkingInStairs = (hitTileGameObject.tag == "Stairway");
		}
		else
		{
			OnEnterTile(null);
        }
	}
	#endregion

	#region jump and fall/roll
	private void StartToJump(Tile targetTile)
	{
		m_IsJumping = true;
		m_IsFalling = false;

		// disable the collider during this type of animations
		m_CapsuleCollider.enabled = false;

		// compute the height (in grid step) between the pawn tile and the clicked tile
		int tileRelativeGridHeight = World.Instance.GetTileRelativeGridDistance(m_PawnTile, targetTile);

		// compute the border direction and set it in any case (both type of animation needs it)
		m_Animator.SetInteger(ANIM_BORDER_DIRECTION_INT, (int)GetBorderDirectionToGoToThisTile(targetTile));
		m_Animator.SetInteger(ANIM_ABSEIL_HEIGHT_INT, tileRelativeGridHeight);

		// if the height is just one step, the pawn will jump, otherwise he will use his rope
		if (tileRelativeGridHeight == 1)
		{
			// set the parameters to the anim state jump
			m_AnimStateJumpToTile.SetStartAndEndTile(m_PawnTile, targetTile);

			// the tile is just under me, we just do a simple jump
			m_Animator.SetTrigger(ANIM_JUMP_TO_TILE_TRIGGER);
		}
		else
		{
			// set the parameters to the anim state jump
			m_AnimStateJumpAbseil.SetStartAndEndTile(m_PawnTile, targetTile);

			// the tile is too low under me, trigger the jump with rope
			m_Animator.SetTrigger(ANIM_JUMP_AND_ABSEIL_TRIGGER);
		}

		// reset the pawn tile when starting to jump, because if you jump from
		// a moving platform, you don't want to jump relative to the plateform
		OnEnterTile(null, false);
	}

	/// <summary>
	/// This callback is called by the animator state machine when all the animations are finished after a jump.
	/// </summary>
	public void OnJumpFinished()
	{
		m_ClickedTile = null; // target reached, forget it
		m_IsJumping = false;
		// reenable the collider
		m_CapsuleCollider.enabled = true;
	}

	/// <summary>
	/// This function should be called when the player click on one tile to change the gravity.
	/// This function will trigger the correct animation on the Pawn depending on the situation.
	/// It can be a roll-over if the player click on the immediate wall, 
	/// or a fall+abseil if the player change the gravity at a much farer distance.
	/// <param name="targetTile"/>The tile whe the player will land (usually the focused or clicked tile)</param>
	/// </summary>
	private void StartToRollOrFallAbseilDueToGravityChange(Tile targetTile)
	{
		// ask how far was the clicked tile
		int tileRelativeGridHeight = World.Instance.GetTileRelativeGridDistance(m_PawnTile, targetTile);

		// compute the border direction and set it in any case (both type of animation needs it)
		m_Animator.SetInteger(ANIM_BORDER_DIRECTION_INT, (int)GetBorderDirectionToGoToThisTile(targetTile));
		m_Animator.SetInteger(ANIM_ABSEIL_HEIGHT_INT, tileRelativeGridHeight);

		// disable the collider during this animation
		m_CapsuleCollider.enabled = false;
		ResetDynamic();
		m_RigidBody.useGravity = false;

		// check if we click on the tile just next to the pawn, or a bit farer away,
		// because we don't play the same kind of animations
		if (tileRelativeGridHeight == 0)
		{
			// set the jumping flag to disable the mouse input
			m_IsJumping = true;

			// set the parameters to the anim state jump
			m_AnimStateRollToTile.SetStartAndEndTile(m_PawnTile, targetTile);

			// trigger the animation
			m_Animator.SetTrigger(ANIM_ROLL_TO_TILE_TRIGGER);
		}
		else if (tileRelativeGridHeight == int.MaxValue)
		{
			// if the height is infinite (max value) that means we will do a free fall

			// Block the mouse input
			m_IsFalling = true;

			// set the target tile as null, but also reset the gravity for the pawn
			OnEnterTile(null, true);
			// re-enable the collider oftherwise we won't get the event telling that the player has left the game space
			m_CapsuleCollider.enabled = true;

			// Fall animation
			m_Animator.SetTrigger(ANIM_FALL_TRIGGER);
		}
		else
		{
			// Block the mouse input
			m_IsFalling = true;

			// if the two tiles are align on same axis, that means the player has click on a tile above,
			// otherwise he clicked on a tile on the side
			if (World.Instance.AreTileOrientedOnTheSameAxis(m_PawnTile, targetTile))
			{
				// set the parameters to the anim state fall and abseil and trigger the anim
				m_AnimStateFallAbseilAbove.SetStartAndEndTile(m_PawnTile, targetTile);
				m_Animator.SetTrigger(ANIM_FALL_AND_ABSEIL_ABOVE_TRIGGER);
			}
			else
			{
				// set the parameters to the anim state fall and abseil and trigger the anim
				m_AnimStateFallAbseilSide.SetStartAndEndTile(m_PawnTile, targetTile);
				m_Animator.SetTrigger(ANIM_FALL_AND_ABSEIL_SIDEWAY_TRIGGER);
			}

			//for punishing gravity take the tile == null here
			OnEnterTile(null, false);
		}

		// change the gravity
		World.Instance.SetGravity(targetTile.orientation);
	}

	/// <summary>
	/// This callback is called by the animator state machine 
	/// when all the animations are finished after a roll to tile, or change gravity with rope.
	/// </summary>
	public void OnRollOrFallAbseilFinished()
	{
		m_ClickedTile = null; // target reached, forget it
		m_IsJumping = false;
		m_IsFalling = false;

		// reenable the collider
		m_CapsuleCollider.enabled = true;
		m_RigidBody.useGravity = true;
	}
	#endregion

	#region focused tile, clickable tile, and destination marks
	private void RemoveDestinationMarks()
	{
		// clear the flag on all the tiles
		foreach (Tile tile in m_ClickableTilesToChangeGravity)
			tile.IsClickableToChangeGravity = false;

		// and clear the list
		m_ClickableTilesToChangeGravity.Clear();
	}

	private bool PutDestinationMarks(Tile tileToCheck)
	{
		RemoveDestinationMarks();

		if ( m_IsWalking || m_IsWalkingInStairs || m_IsFalling || m_IsJumping )
			return false;

		bool result = false;
		TileOrientation currentWorldOrientation = GetWorldVerticality();

		// ray cast in the 6 direction of the world from the pawn position
		for (int i = 0 ; i < 6 ; ++i)
		{
			TileOrientation orientation = (TileOrientation)(i + 1);

			RaycastHit hit = new RaycastHit();

			// Casting a ray towards 'orientation', SphereCast needed because of Pawn's capsule thickness and ignoring Pawn's collider
			if (Physics.SphereCast(transform.position, m_Width * 0.4f, World.GetGravityNormalizedVector(orientation), out hit, 10000, m_TilesLayerMask))
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
					m_ClickableTilesToChangeGravity.Add(tile);
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
		if (m_FocusedTile != null)
			m_FocusedTile.unHighlight();

		// get the tile currently pointed by the player's cursor
		Tile pointedTile = TileSelection.getTile();

		// the set the focused tile with the pointed one if it is not null and clickable
		if ((pointedTile != null) && TileSelection.isClickableType(pointedTile.Type))
			m_FocusedTile = pointedTile;
		else
			m_FocusedTile = null;

		// Now we will check if the focused tile is clickable or not.
		bool isFocusedTileClickable = false;

		// For that will we ask a valid AStar for normal walk navigation from the pawntile,
		// or we will check if the tile is accessible by fall from pawntile.
		if ((m_FocusedTile != null) && (m_PawnTile != null))
		{	
			// first check if there's a path from the pawntile to the focused tile
			List<Tile> accessibleTiles = AStarHelper.Calculate(m_PawnTile, m_FocusedTile);

			// check if the tile is accessible by walk (astar)
			if ((accessibleTiles != null) && (accessibleTiles.Count > 0))
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
				if (m_IsGlued && (m_FocusedTile.orientation != GetWorldVerticality()))
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
			else if (m_FocusedTile.orientation == m_PawnTile.orientation)
			{
				// the tile must be below the pawn tile and the gravity must be in the right direction
				// so if the pawn is glued on the pawntile with a gravity in different direction,
				// he cannot jump
				bool isAccessibleByFall = IsTileBelow(m_FocusedTile) && (m_PawnTile.orientation == GetWorldVerticality());

				// get a threshold value for testing a distance greater than a cube distance
				const float minDistThreshold = GameplayCube.CUBE_SIZE * 0.05f; // this equals to 10cm for now with the cube at 2m
				const float moreThanCubeDistance = GameplayCube.CUBE_SIZE + minDistThreshold;

				// iff (if and only if)
				if ( isAccessibleByFall && (m_PawnTile.orientation == TileOrientation.Down || m_PawnTile.orientation == TileOrientation.Up))
				{
					float xDist = Mathf.Abs(m_PawnTile.transform.position.x - m_FocusedTile.transform.position.x);
					float zDist = Mathf.Abs(m_PawnTile.transform.position.z - m_FocusedTile.transform.position.z);
					bool xTest = xDist > minDistThreshold;
					bool zTest = zDist > minDistThreshold;

					if (xTest && zTest)
						isAccessibleByFall = false;
					else if (!xTest && !zTest)
						isAccessibleByFall = false;

					if (xDist > moreThanCubeDistance)
						isAccessibleByFall = false;
					if (zDist > moreThanCubeDistance)
						isAccessibleByFall = false;
				}
				
				if (isAccessibleByFall && (m_PawnTile.orientation == TileOrientation.Left || m_PawnTile.orientation == TileOrientation.Right))
				{
					float yDist = Mathf.Abs(m_PawnTile.transform.position.y - m_FocusedTile.transform.position.y);
					float zDist = Mathf.Abs(m_PawnTile.transform.position.z - m_FocusedTile.transform.position.z);
					bool yTest = yDist > minDistThreshold;
					bool zTest = zDist > minDistThreshold;
					
					if (yTest && zTest)
						isAccessibleByFall = false;
					else if (!yTest && !zTest)
						isAccessibleByFall = false;

					if (yDist > moreThanCubeDistance)
						isAccessibleByFall = false;
					if (zDist > moreThanCubeDistance)
						isAccessibleByFall = false;
				}
				
				if (isAccessibleByFall && (m_PawnTile.orientation == TileOrientation.Front || m_PawnTile.orientation == TileOrientation.Back))
				{
					float xDist = Mathf.Abs(m_PawnTile.transform.position.x - m_FocusedTile.transform.position.x);
					float yDist = Mathf.Abs(m_PawnTile.transform.position.y - m_FocusedTile.transform.position.y);
					bool xTest = (xDist > minDistThreshold) && (xDist < moreThanCubeDistance);
					bool yTest = (yDist > minDistThreshold) && (yDist < moreThanCubeDistance);
					
					if (xTest && yTest)
						isAccessibleByFall = false;
					else if (!xTest && !yTest)
						isAccessibleByFall = false;
					
					if (xDist > moreThanCubeDistance)
						isAccessibleByFall = false;
					if (yDist > moreThanCubeDistance)
						isAccessibleByFall = false;
				}

				// now set the clickable flag if we can jump on it
				isFocusedTileClickable = isAccessibleByFall;
			}
		}

		// now compute the destination marks for the gravity change.
		// This function will also mark some tiles as clickable
		bool canFocusedTileClhangeGravity = PutDestinationMarks(m_FocusedTile);
		// update the clickable flag
		isFocusedTileClickable = isFocusedTileClickable || canFocusedTileClhangeGravity;

		// now check if the focused tile is the same as the pawn tile, we only authorize the click
		// if the player is glued and the gravity is not under his feet,
		// in order for the player to put back the gravity under his feet.
		if ((m_FocusedTile != null) && (m_FocusedTile == m_PawnTile))
			isFocusedTileClickable = m_IsGlued && (m_FocusedTile.orientation != TileOrientation.None) && (m_FocusedTile.orientation != GetWorldVerticality());

		// now highlight the focused tile if it is clickable (may happen with AStar navigation, fall or gravity change)
		if (m_FocusedTile != null)
		{
			if (isFocusedTileClickable)
				m_FocusedTile.highlight();
			else
				m_FocusedTile.unHighlight();
		}
	}
	#endregion

	#region mouse management
	/// <summary>
	/// Manages the interaction with the mouse.
	/// </summary>
	private void ManageMouse()
	{
		// get the camera control, to know if the input are not currently used to rotate the camera
		// which means that the camera has capture the input
		CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
		bool isInputCapturedByCamera = (cameraControl != null) && (cameraControl.HasCameraCapturedInput);

		// the normal case, the input doesn't control the camera
		if (!isInputCapturedByCamera)
		{
			if (m_IsWalking || m_IsFalling || m_IsJumping || (m_FocusedTile == null) || World.Instance.IsThereAnyCubeFalling())
				return;

			// if the focussed tile is highlighted that means it is clickable
			if (InputManager.isClickUp() && m_FocusedTile.IsHighlighted)
			{
				// get the camera sound to play a 2D UI sound when clicking
				Camera2DSound sound = Camera.main.GetComponent<Camera2DSound>();

				// play the click sound, as the player clicked a tile
				sound.playSound(Camera2DSound.SoundId.CLICK_TILE);
							
				// If the player clicked a tile with different orientation
				if ( ( m_FocusedTile.orientation != m_PawnTile.orientation ) || 
						( m_IsGlued && (m_FocusedTile == m_PawnTile)) )
				{
					// player has changed the gravity, increase the counter
					HUD.Instance.IncreaseGravityChangeCount();

					// play the gravity change sound)
					sound.playSound(Camera2DSound.SoundId.GRAVITY_CHANGE);
								
					// If the pawn is on a glue tile, the change of gravity is managed differently
					if ( m_IsGlued )
					{
						World.Instance.SetGravity( m_FocusedTile.orientation );
					}
					else
					{
						// asked the clicked tile to play it's attraction VFX
						m_FocusedTile.playActivationVFX();

						// then triggrer the animation
						StartToRollOrFallAbseilDueToGravityChange(m_FocusedTile);
					}
				}
				else
				{
					// memorised the clicked tile
					m_ClickedTile = m_FocusedTile;
					// ask a new path if we go to a different tile
					if (m_PawnTile != m_ClickedTile)
						StartToWalkToTile(m_PawnTile, m_ClickedTile);
				}
	        }
		}
		else
		{
			// camera has capture input for rotating, so clear the focused tile
			if (m_FocusedTile != null)
				m_FocusedTile.unHighlight();
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
		if (m_IsGlued && (m_PawnTile != null))
			return m_PawnTile.getDownVector();
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
		// for the threshold take 80% of half a cube size. In case you can to detect a tile 
		// which is just a touching wall of the current pawn tile, the distance will ne normally half a cube size.
		const float DISTANCE_THRESHOLD = GameplayCube.HALF_CUBE_SIZE * 0.8f;

		// compute the position of the target tile in the pawn local coordinates
		Vector3 localTargetTilePosition = transform.worldToLocalMatrix.MultiplyPoint(targetTile.Position);

		// now check if the target position if roughly in front, back, right or left
		if (localTargetTilePosition.z > DISTANCE_THRESHOLD)
			return BorderDirection.FRONT;
		else if (localTargetTilePosition.z < -DISTANCE_THRESHOLD)
			return BorderDirection.BACK;
		else if (localTargetTilePosition.x > DISTANCE_THRESHOLD)
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
		if (m_PawnTile != null) 
		{
			Vector3 down = GetMyVerticality();
			return Physics.SphereCast( new Ray( transform.position, down ), m_Width * 0.5f, m_Height * 0.5f, m_TilesLayerMask );
		}
		
		return false; // if there isn't a tile beneath him, he isn't grounded
	}

	/// <summary>
	/// Is the target tile above the Pawn?
	/// </summary>
	private bool IsTileBelow(Tile target)
	{
		switch (m_PawnTile.orientation)
        {
            default:
				return m_PawnTile.transform.position.y > target.transform.position.y;
            case TileOrientation.Down:
				return m_PawnTile.transform.position.y < target.transform.position.y;
			case TileOrientation.Left:
				return m_PawnTile.transform.position.x > target.transform.position.x;
			case TileOrientation.Right:
				return m_PawnTile.transform.position.x < target.transform.position.x;
            case TileOrientation.Front:
				return m_PawnTile.transform.position.z < target.transform.position.z;
            case TileOrientation.Back:
				return m_PawnTile.transform.position.z > target.transform.position.z;
        }
	}

    /// <summary>
    /// Gets the player ground position, i.e. the position of the "feet" of the Pawn, pawn has 8 height
    /// </summary>
    private Vector3 GetGroundPosition()
	{
		float halfHeight = m_Height * 0.5f;

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
