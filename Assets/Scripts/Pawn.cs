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
[RequireComponent(typeof(BoxCollider))]
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
	public float turnDelay = 0.5f; // 1.0f		// délai avant la chute
	public float fallInterval = 4.0f;
	public float jumpAnimationLength = 0.3f;
	private float height;
	private float width;
	private bool newTarget = true;
	private bool lookAt_DestroyCoincidents;
	private Vector3 desiredRotation;
	private Vector3 desiredPosition;
	private bool isWalkingInStairs;
	private bool isWalking;

	private BoxCollider boxCollider;
	
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
	private Animator animator;
	private int animState;
	private int idleState;
	private float idleWait;
	
	// #SPAWN#
	private Vector3 spawnPosition;// position of the spawn GameObject
	private Quaternion spawnRotation;// rotation of the spawn GameObject
	
	// #TILES#
	private List<Tile> path = new List<Tile> (); // List of tiles in the current path
	private Tile playerTile; // Tile beneath the Pawn
	private Tile targetTile; // Tile the player targeted
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
		world = gameObject.AddComponent<World>() as World;
		world.Init( this );
	}

    void Start()
	{
		desiredRotation = Vector3.zero;
		desiredPosition = Vector3.zero;

		isWalking = false;
		isWalkingInStairs = false;
		
		animator = transform.FindChild("OldGuy").GetComponent<Animator>();

		boxCollider = GetComponent<BoxCollider>();
		height = boxCollider.size.y * boxCollider.transform.localScale.y;
		width = boxCollider.size.x * boxCollider.transform.localScale.x;

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
			orientationSpheres[i].GetComponent<Renderer>().material = Assets.getSphereMat();
			orientationSpheres[i].GetComponent<Renderer>().material.color = hud.dotColor;
			orientationSpheres[i].transform.localScale = Vector3.one * hud.dotSize;
			orientationSpheres[i].layer = 10;
			orientationSpheres[i].GetComponent<Collider>().enabled = false;
			orientationSpheres[i].name = "dot " + i;
			orientationSpheres[i].transform.parent = o.transform;
		}
	}
	
	public void respawn()
	{
		path = null;
		playerTile = null;
		targetTile = null;

		animState = 2;
		isFalling = true;
		isJumping = false;
		isWalking = false;
		
		transform.position = spawnPosition;
		transform.rotation = spawnRotation;
		
		SetWorldGravity( TileOrientation.Up );
		ResetDynamic();
		
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		boxCollider.enabled = true;

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

	private void SnapToTile( Tile tile )
	{
		moveMe (tile.transform.position);
		//path = AStarHelper.Calculate(tile, p); //give me a path towards the nearest tile
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (isFalling)
		{
			animState = 3;
			isFalling = false;
			GetComponent<Rigidbody>().mass = 1;
			
			if( playerTile != null )
			{
				Tile[] tiles = GameObject.FindObjectsOfType<Tile>();
				List<Tile> tilesList = new List<Tile>();
				
				for (int i = 0; i != tiles.Length; i++)
				{
					if (tiles[i].orientation == playerTile.orientation)
						tilesList.Add(tiles[i]);
				}
				
/*				if ( tilesList.Count > 0 )
				{
					Tile nearest = Tile.Closest(tilesList, transform.position); //nearest tile: the directly accessible tile from the tile bellow the Pawn, thats closest to the target tile
					SnapToTile( nearest );
				}
*/			}

			putDestinationMarks( playerTile );
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
			if (playerTile != null && playerTile.type.Equals(TileType.Exit))//Has the player reached an exit Tile?
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
		if (isGrounded() || isJumping) // is the player touching a tile "beneath" him?
		{
			if( playerTile.type.Equals(TileType.Exit) ) //if this tile is an exit tile, make the game end
				world.GameOver();
				
            moveAlongPath(); //otherwise, move along the path to the player selected tile
        }
		else if (isWalkingInStairs)
			moveAlongPath();
    }
	
    /// <summary>
    /// Moves along the path.
    /// If there is a path that has been previously decided the Pawn should take, finish the path.
    /// Otherwise, if there isn't a path, but there is a targetTile, a valid tile that the player has clicked. 
    /// This targetTile can be in a place not directly accessible to the Pawn, for example if there is a gap or if the tile is lower and the Pawn is supposed to fall.
    /// </summary>
    private void moveAlongPath()
	{
		//ResetDynamic ();
		
		if (path != null && path.Count > 0) //is there a path?
		{
			// play animation: walk
			animState = 1;
			isWalking = true;

			if (newTarget)
				StartCoroutine( LookAt ( path[0].transform.position ) );
			
            //if there is, move the pawn towards the next point in that path
            Vector3 vec = path[0].transform.position - getGroundPosition();
			
            if (moveMe(vec))
			{
				//SnapToTile( path[0] );
				//transform.position = new Vector3( path[0].transform.position.x, transform.position.y, path[0].transform.position.z );
				position = path[0].transform.position;

				newTarget = true;
				path.RemoveAt(0); //if we have reached this path point, delete it from the list so we can go to the next one next time
			}
			
			if (path.Count == 0)
			{
				animState = 0;
				isWalking = false;
			}
			
        }
        else if (targetTile != null) // Case where there is no path but a target tile, ie: target tile is not aligned to tile
		{
			if ( targetTile == playerTile )
				targetTile = null;
			else
			{
			//if ( targetTileIsAbove(targetTile) || targetTile.type.Equals(TileType.Invalid) ) // is targetTile above the pawn or of type invalid?
			//{
			//	GetComponent<AudioSource>().PlayOneShot(Assets.invalidSound); // play a failed action sound
				//targetTile = null; //forget target tile
			//}
			//else //the tile is in a valid place
	        //{
	        	// tile is not accessible but in valid space, so:
	        	// pawn will go towards the tile, then
	        	//  (- he will land on a neighbourg tile) -- no more possible yet
	        	//  - he will land on the tile
	        	//  (- he will fall into the void) -- this case won't happen anymore

				// nearest tile: the directly accessible tile from the tile bellow the Pawn, thats closest to the target tile
				Tile nearest = Tile.Closest(playerTile.AllAccessibleTiles(), targetTile.transform.position);
				
				if (nearest.Equals(playerTile) ) //is the nearest the one bellow the Pawn?
				{
					// landing tile: the directly accessible tile from the target tile, thats closest to the nearest tile
					Tile landing = Tile.Closest(targetTile.AllAccessibleTiles(), nearest.transform.position);
					
	                // check if landing tile is not EXACTLY under the nearest tile
					if ( Vector3.Scale ( ( landing.transform.position - nearest.transform.position ), Vector3.Scale ( World.getGravityVector( GetWorldGravity() ), World.getGravityVector( GetWorldGravity() ) ) - new Vector3( 1, 1, 1 ) ).magnitude == 0 )
						landing = Tile.Closest( landing.AllAccessibleTiles(), targetTile.transform.position );
					
					// calculate the vector from the Pawns position to the landing tile position at the same height
	                Vector3 vec = getGroundHeightVector(landing.transform.position) - getGroundPosition();

					// There is definitely a tile to fall, jump and go
					if ( vec != Vector3.zero )
					{
						if ( !isJumping )
						{
							//GetComponent<Rigidbody>().AddForce( -Physics.gravity * vec.sqrMagnitude * 0.45f );
							//GetComponent<Rigidbody>().mass = 30;
							StartCoroutine( JumpToTile());
							isJumping = true;
							
							StartCoroutine( LookAt ( landing.transform.position ) );
						}

						if (moveMe(vec)) // move the pawn towards that vector
						{
							targetTile = null; // target reached, forget it
							isJumping = false;
							path = AStarHelper.Calculate(landing, nearest); // give me a path towards the nearest tile
							putDestinationMarks( landing );
						}
					}
					else
					{
						targetTile = null;
					}
	            }
	            else
				{
					path = AStarHelper.Calculate(playerTile, nearest); //give me a path towards the nearest tile
	            }
			}
	    }
    }

	private IEnumerator JumpToTile()
	{
		float elapsedTime = 0;

		Vector3 jumpPos = Vector3.zero;

		while ( elapsedTime < jumpAnimationLength )
		{
			float t = elapsedTime / jumpAnimationLength;

			jumpPos = transform.position - (Physics.gravity.normalized * Mathf.Cos( t ) * 0.75f );
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
		if ( vec.magnitude > 1)
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
		Vector3 absG = Vector3.Scale ( Vector3.Normalize (Physics.gravity), Vector3.Normalize (Physics.gravity));

		// get the Y relative
		// point is scaled to inverted square of G
		Vector3 target = Vector3.Scale ( - absG, point);
//		Debug.Log ("Y relative: " + target);

		// get the X, Z relatives
		// remove the Y relative from target
		target = point + target;
//		Debug.Log ("X, Z relatives: " + target);

		// get the final point
		// remove pawn's & tile's heights from target
		target = target - Vector3.Normalize (Physics.gravity) * height * 0.5f - Vector3.Scale ( - absG, playerTile.transform.position);

		Vector3 forward = target - transform.position;

		Quaternion _look = Quaternion.identity;

		if ( forward != Vector3.zero )
			_look = Quaternion.LookRotation( forward, -Vector3.Normalize(Physics.gravity) );
		
		lookAt_DestroyCoincidents = true;
		yield return 0;
		lookAt_DestroyCoincidents = false;
		
		while (true)
		{
			if (lookAt_DestroyCoincidents)
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
    /// Updates the value of tile to the tile beneath the Pawn.
    /// Checks the space underneath the Pawn
    /// Assigns the spheres/dots to the tiles of other orientations where the Pawn would land after gravity changes
    /// </summary>
    private void checkUnderneath()
	{
		if ( clickableTiles == null )
			clickableTiles = new List<Tile>();

        //checks underneath the pawn
		Tile tile = null;

		RaycastHit hit = new RaycastHit();

		Debug.DrawRay (transform.position, Physics.gravity.normalized * width * 0.4f, Color.red, .1f, false);

		// casting a ray down, we need a sphereCast because the capsule has thickness, and we only need tiles layer
		if (Physics.SphereCast(transform.position, width * 0.4f, Physics.gravity.normalized, out hit, height * 0.5f, (1 << LayerMask.NameToLayer( "Tiles" ) )))
		{
			tile = hit.collider.gameObject.GetComponent<Tile>();

			if (tile != null) // if it is a tile
				playerTile = tile;
            else
				playerTile = null;
			
			if ( hit.collider.gameObject.tag == "MovingPlatform" )
			{
				// dirty snapping to a moving tile
				// actually, a mask is used to mix up both player & tile positions

				Vector3 _vec = Vector3.zero;

				// the gravity
				Vector3 _g = Vector3.Scale ( Vector3.Normalize ( Physics.gravity ), Vector3.Normalize ( Physics.gravity ) );

				if ( path.Count > 0 )
				{
					// the player asked the pawn to move

					_vec = path[0].transform.position - getGroundPosition();
					_vec = _vec - Vector3.Scale (  _g, _vec);

					if (_vec.x != 0)
						_vec.x = 1;
					if (_vec.y != 0)
						_vec.y = 1;
					if (_vec.z != 0)
						_vec.z = 1;
				}

				// _ppos: tile position 
				Vector3 _ppos = tile.transform.position - Vector3.Scale ( _g, tile.transform.position);

				// first mask: actual gravity
				Vector3 _mask = _g;

				// second mask: playerPawn's movement
				_mask = _mask - _vec;

				// get absolute
				_mask = Vector3.Scale( _mask, _mask );

				// mask the pawn's position
				_vec = Vector3.Scale( _mask, transform.position );
//				Debug.Log( "_vec: " + _vec + ", masque " + _mask );

				// revert the mask
				_mask = - (_mask - Vector3.one );
				
				// mask the tile's position
				_ppos = Vector3.Scale( _mask, _ppos );

				// compute the new position
				position =  _vec + _ppos;
//				Debug.Log( "transform: " + transform.position );
			}
			else if ( hit.collider.gameObject.tag == "Stairway" )
			{
				isWalkingInStairs = true;
				//moveAlongPath();
				//moveMe();
				//Debug.Log("Ok !!");
				//rigidbody.MovePosition( transform.position + transform.forward * 0.1f );
			}
        }
        else
		{
			playerTile = null;
        }

		putDestinationMarks (tile);

    }

	private void putDestinationMarks( Tile t )
	{
		foreach (TileOrientation orientation in Enum.GetValues(typeof(TileOrientation)))
		{
			if (!isFalling)
			{
				t = null;
				
				RaycastHit hitc = new RaycastHit ();
				//			if (Physics.SphereCast(_pos, height / 2.0f + 0.2f, Physics.gravity, out hitc, 10000, ~(1 << 10)))//casting a ray down, we need a sphereCast because the capsule has thickness, and we need to ignore the Pawn collider
				//			if (Physics.SphereCast(_pos, height / 2.0f + 0.2f, Physics.gravity, out hitc, 10000, (1 << 14)))//casting a ray down, we need a sphereCast because the capsule has thickness, and we need to ignore the Pawn collider
				//			if (Physics.SphereCast(_pos,  1.5f, getGravityVector( GetWorldGravity() ), out hitc, 10000, (1 << 14)))//casting a ray down, we need a sphereCast because the capsule has thickness, and we need to ignore the Pawn collider
				if (Physics.SphereCast (transform.position, 0.5f + 0.2f, World.getGravityVector (orientation).normalized, out hitc, 10000, (1 << LayerMask.NameToLayer( "Tiles" )))) {//casting a ray down, we need a sphereCast because the capsule has thickness, and we need to ignore the Pawn collider
					t = hitc.collider.gameObject.GetComponent<Tile> ();
					
					if (t != null && t != playerTile) {
						t.isClickable = true;
						
						if (!clickableTiles.Contains (t))
							clickableTiles.Add (t);
						
						if (hud.dotIsInside)
							getOrientationSphere (orientation).transform.position = t.transform.position;
						else
							getOrientationSphere (orientation).transform.position = t.transform.position - (World.getGravityVector (GetWorldGravity ()) * hud.dotSize / 2);
						
						if (t.GetComponent<Stairway> ()) {
							// don't put dots on stairways
							getOrientationSphere (orientation).transform.position = Vector3.one * float.MaxValue; //sphere is moved to infinity muhahahaha, tremble before my power
						}
					} else {
						t.isClickable = false;
						// Valid target
						getOrientationSphere (orientation).transform.position = Vector3.one * float.MaxValue; //sphere is moved to infinity muhahahaha, tremble before my power
					}
				}
			} else {
				// Falling, no more dots
				getOrientationSphere(orientation).transform.position = Vector3.one * float.MaxValue; //BEGONE
			}
		}
	}

    /// <summary>
    /// Manages the interaction with the mouse.
    /// </summary>
    private void manageMouse()
	{
		if (isFalling || isJumping || (path != null) && path.Count > 0)
			return;

		Tile tile = TileSelection.getTile();

		if ( tile != null && playerTile != null && tile != playerTile && playerTile.GetComponent<Stairway>() == null )
		{
			//if ( tile.GetComponent<Stairway>() != null )
			//	return;

			List<Tile> accessibleTiles = AStarHelper.Calculate(playerTile, tile);
			
			if ( accessibleTiles != null && accessibleTiles.Count > 0 )
			{
				for ( int i = 0, l = accessibleTiles.Count; i < l; i++ )
				{
					clickableTiles.Add( accessibleTiles[i] );
				}

				if ( TileSelection.isClickableType( tile.type ) )
				{
					tile.isClickable = true;
					tile.highlight();
				}
			}

			// Check if the tile is accessible "by fall"
			if ( tile.orientation == playerTile.orientation )
			{
				bool isAccessibleByFall = true;
				
				if ( playerTile.orientation == TileOrientation.Down && tile.transform.position.y < playerTile.transform.position.y)
					isAccessibleByFall = false;
				else if ( playerTile.orientation == TileOrientation.Up && tile.transform.position.y > playerTile.transform.position.y )
					isAccessibleByFall = false;
				else if ( playerTile.orientation == TileOrientation.Left && tile.transform.position.x > playerTile.transform.position.x )
					isAccessibleByFall = false;
				else if ( playerTile.orientation == TileOrientation.Right && tile.transform.position.x < playerTile.transform.position.x )
					isAccessibleByFall = false;
				else if ( playerTile.orientation == TileOrientation.Front && tile.transform.position.z < playerTile.transform.position.z )
					isAccessibleByFall = false;
				else if ( playerTile.orientation == TileOrientation.Back && tile.transform.position.z > playerTile.transform.position.z )
					isAccessibleByFall = false;

				if ( isAccessibleByFall && playerTile.orientation != TileOrientation.Down && playerTile.orientation != TileOrientation.Up )
				{
					float distance = Mathf.Abs( playerTile.transform.position.y - tile.transform.position.y );
					if ( distance > 10.1f )
						isAccessibleByFall = false;
				}
				
				if ( isAccessibleByFall && playerTile.orientation != TileOrientation.Left && playerTile.orientation != TileOrientation.Right )
				{
					float distance = Mathf.Abs( playerTile.transform.position.x - tile.transform.position.x );
					if ( distance > 10.1f )
						isAccessibleByFall = false;
				}
				
				if ( isAccessibleByFall && playerTile.orientation != TileOrientation.Front && playerTile.orientation != TileOrientation.Back )
				{
					float distance = Mathf.Abs( playerTile.transform.position.z - tile.transform.position.z );
					if ( distance > 10.1f )
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
		}

		TileSelection.highlightTargetTile();

		if(!isCameraMode)
		{
	        if (Input.GetMouseButton(0))
			{
				if(Time.time - lastClick < .1)
					clickCountdown += Time.deltaTime;
				else
					clickCountdown = 0;

	            lastClick = Time.time;

				if(clickCountdown > .25)
				{
					isCameraMode = true;
					StartCoroutine(SetCameraCursor());
				}
	        }

			if (Input.GetMouseButtonUp(0) && !isCameraMode && GetComponent<Rigidbody>().useGravity)
			{
				clickCountdown = 0;

				if (tile != null && tile.isClickable && !world.FallingCubes())
				{
					if ( !isWalking && ( playerTile == null || tile.orientation != playerTile.orientation ) ) //for punishing gravity take the tile == null here
					{
						hud.gravityChangeCount++;
						playerTile = null;
						
						desiredPosition = new Vector3( 0, height / 2 * fallInterval, 0 );

						StartCoroutine( DelayedPawnFall ( tile ));
					}
					else
					{
						targetTile = tile;
						
						if ( targetTile.transform != playerTile.transform && targetTile.orientation == playerTile.orientation )
						{
							path = AStarHelper.Calculate(playerTile, tile);
							//StartCoroutine( LookAt ( path[0].transform.position ) );
						}
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

		if ( clickableTiles != null )
		{
			// clear the "clickable tiles"
			for ( int i = 0, l = clickableTiles.Count; i < l; i++ )
				clickableTiles[i].isClickable = false;
			
			clickableTiles.Clear ();
		}
	}
	
	private IEnumerator DelayedPawnFall( Tile t )
	{
//		collider.gameObject.layer = 12;
		nextConstraint = GetComponent<Rigidbody>().constraints;

		ResetDynamic();

		float timer = 0.0f;

		animState = 2;
		isFalling = true;

		// Make the pawn float in the airs a little
		while(true)
		{
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().constraints = transformConstraints;
			
			if(adjustPawnPosition( ref timer ))
			{
				timer = 0.0f;
				break;
			}
			
			yield return 0;
		}
		
		SetPawnOrientation( t.orientation );

		// Rotate the pawn in order to face the correct direction
		while(true)
		{
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().constraints = transformConstraints;
			
			if( adjustPawnRotation( ref timer ) )
			{
				timer = 0.0f;
				break;
			}

			yield return 0;
		}
		
		GetComponent<Rigidbody>().constraints = nextConstraint;
		GetComponent<Rigidbody>().useGravity = true;

		SetWorldGravity( t.orientation );
		world.ChangeGravity ( t.orientation );
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
		
		//CameraControl cam = Camera.main.GetComponent<CameraControl> ();
		//cam.roll = Mathf.Lerp( cam.roll, cameraRotation.z, timer / turnDelay );
		//cam.pan = Mathf.Lerp( cam.pan, -cameraRotation.x, timer / turnDelay );
		//cam.tilt = Mathf.Lerp( cam.tilt, cameraRotation.y, timer / turnDelay );

		if ( _rot == transform.rotation )
		{
			//cam.roll = cameraRotation.z;
			//cam.pan = -cameraRotation.x;
			//cam.tilt = cameraRotation.y;
			transform.rotation = _to;

			return true;
		}
		else
			return false;
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

	private void SetWorldGravity(TileOrientation orientation)
	{
		switch (orientation)
		{
		default:
			break;
		case TileOrientation.Front:
			Physics.gravity = new Vector3(0, 0, World.G);
			break;
		case TileOrientation.Back:
			Physics.gravity = new Vector3(0, 0, -World.G);
			break;
		case TileOrientation.Right:
			Physics.gravity = new Vector3(World.G, 0, 0);
			break;
		case TileOrientation.Left:
			Physics.gravity = new Vector3(-World.G, 0, 0);
			break;
		case TileOrientation.Up:
			Physics.gravity = new Vector3(0, -World.G, 0);
			break;
		case TileOrientation.Down:
			Physics.gravity = new Vector3(0, World.G, 0);
			break;
		}
	}

	private TileOrientation GetWorldGravity()
	{
		if (Physics.gravity.x > 0)
			return TileOrientation.Right;
		if (Physics.gravity.x < 0)
			return TileOrientation.Left;
		if (Physics.gravity.y > 0)
			return TileOrientation.Down;
		if (Physics.gravity.y < 0)
			return TileOrientation.Up;
		if (Physics.gravity.z > 0)
			return TileOrientation.Front;
		if (Physics.gravity.z < 0)
			return TileOrientation.Back;
		else
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
		if (playerTile != null) //is there even a tile beneath the Pawn
		{
			Vector3 origin = transform.position;
			/*
			if ( GetWorldGravity() == TileOrientation.Up )
				origin.y += 1;
			else if ( GetWorldGravity() == TileOrientation.Down )
				origin.y -= 1;
			else if ( GetWorldGravity() == TileOrientation.Back )
				origin.z += 1;
			else if ( GetWorldGravity() == TileOrientation.Front )
				origin.z -= 1;
			else if ( GetWorldGravity() == TileOrientation.Left )
				origin.x += 1;
			else if (  GetWorldGravity() == TileOrientation.Right )
				origin.x -= 1;
				*/

			Ray ray = new Ray( origin, Physics.gravity.normalized );
			
			return Physics.SphereCast( ray, width * 0.5f, height * 0.5f, (1 << LayerMask.NameToLayer( "Tiles" )) );
			//return Physics.SphereCast( ray, height * 0.5f + 0.2f, 3.5f, (1 << LayerMask.NameToLayer( "Tiles" )) );

			/*
			float proximityThreshold = 0.2f; // the value bellow which can be said that the tile is touching the Pawn, it's like an error margin
			
			if (tile.orientation.Equals(TileOrientation.Up) || tile.orientation.Equals(TileOrientation.Down)) //check for up and down
				return Math.Abs(tile.transform.position.y - getGroundPosition().y) < proximityThreshold; //check distance
			
			if (tile.orientation.Equals(TileOrientation.Left) || tile.orientation.Equals(TileOrientation.Right)) //check for left and right
				return Math.Abs(tile.transform.position.x - getGroundPosition().x) < proximityThreshold; //check distance
			
			if (tile.orientation.Equals(TileOrientation.Front) || tile.orientation.Equals(TileOrientation.Back)) //check for front and back
				return Math.Abs(tile.transform.position.z - getGroundPosition().z) < proximityThreshold; //check distance
				*/
		}
		
		return false; // if there isn't a tile beneath him, he isn't grounded
	}

	/*
	/// <summary>
	/// Is the target tile above the Pawn?
	/// </summary>
	private bool targetTileIsAbove(Tile target)
	{
		switch (playerTile.orientation)
        {
            default:
				return playerTile.transform.position.y < target.transform.position.y;
            case TileOrientation.Down:
				return playerTile.transform.position.y > target.transform.position.y;
			case TileOrientation.Left:
				return playerTile.transform.position.x < target.transform.position.x;
			case TileOrientation.Right:
				return playerTile.transform.position.x > target.transform.position.x;
            case TileOrientation.Front:
				return playerTile.transform.position.z > target.transform.position.z;
            case TileOrientation.Back:
				return playerTile.transform.position.z < target.transform.position.z;
        }
	}
	*/

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
            case TileOrientation.Down:
				return new Vector3(0, 180, 180);
			case TileOrientation.Left:
				return new Vector3(0, 180, 90);
			case TileOrientation.Right:
                return new Vector3(0, 0, 90);
            case TileOrientation.Front:
                return new Vector3(0, 270, 90);
            case TileOrientation.Back:
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
		{
		default:
			return new Vector3 (transform.position.x, transform.position.y - n, transform.position.z);
		case TileOrientation.Up:
			return new Vector3 (transform.position.x, transform.position.y - n, transform.position.z);
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
			//*/
		}
    }



    /// <summary>
    /// Gets the ground height vector for the target position.
    /// </summary>
    /// <param name="position">Position of something</param>
    /// <returns>The position of something at the same height as the Pawn</returns>
    private Vector3 getGroundHeightVector(Vector3 position)
	{
		if (playerTile.orientation.Equals(TileOrientation.Up) || playerTile.orientation.Equals(TileOrientation.Down))
            return new Vector3(position.x, getGroundPosition().y, position.z);
		else if (playerTile.orientation.Equals(TileOrientation.Left) || playerTile.orientation.Equals(TileOrientation.Right))
            return new Vector3(getGroundPosition().x, position.y, position.z);
        else
            return new Vector3(position.x, position.y, getGroundPosition().z);
	}

    /// <summary>
    /// Gets the game object of the sphere for a given TileOrientation
    /// </summary>
    private GameObject getOrientationSphere(TileOrientation orientation)
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
