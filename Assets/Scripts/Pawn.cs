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
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MeshRenderer))]
public class Pawn : MonoBehaviour
{
    public List<Platform> path = new List<Platform>(); // List of platforms in the current path
    public Platform platform;// Platform beneath the Pawn
    private Platform targetPlatform;// Platform the player targeted
    public PlatformOrientation gravity;// gravitational orientation applied to the Pawn
    public float speed = 20;// Speed of the pawn

    private bool isGameOver = false; //Game state
    private Vector3 spawnPosition;// position of the spawn GameObject
    public HUD hud; //script responsible for the HUD

    private GameObject[] orientationSpheres = new GameObject[6];
    /// <summary>
    ///  START
    ///  THIS IS CALLED ONCE IN THE BEGINNING
    /// </summary>
    void Start()
    {
        initSpawn();
        initHUD();
        initOrientationSpheres();
        initGravity();
    }

    /// <summary>
    /// Fetches the position of the spawn GameObject.
    /// Incase there is no spawn it will use the Pawn's initial position as spawnPoint
    /// </summary>
    private void initSpawn()
    {
        GameObject spawn = GameObject.FindGameObjectWithTag("Spawn");
        spawnPosition = (spawn == null) ? transform.position : spawn.transform.position;
        respawn();
    }

    public void respawn()
    {
        transform.position = spawnPosition;
        gravity = PlatformOrientation.Up;
        isGameOver = false;
    }

    private void initHUD()
    {
        hud = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUD>();
    }

    private void initGravity()
    {
        checkUnderneath();
        gravity = platform.orientation;
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


    /// <summary>
    ///  UPDATE
    ///  THIS IS CALLED ONCE PER FRAME
    /// </summary>
    void Update()
    {
        if (!(isGameOver || hud.isPaused)) // is the game active?, i.e. is the game not paused and not finished?
        {
            //if it is do stuff
            manageMouse();
            movePawn();
            checkUnderneath();
        }
    }

    /// <summary>
    ///  ONGUI
    ///  THIS IS CALLED ONCE PER FRAME
    ///  
    /// Checks if the game as ended, if it has, it activates the HUD's endscreen
    /// </summary>
    void OnGUI()
    {
        if (isGameOver) //is the game over? 
        {
            if (platform != null && platform.type.Equals(PlatformType.Exit))//Has the player reached an exit Platform?
            {
                hud.isEndScreen = true; //activate the endscreen
            }
            else //the player must have crashed into the DeathZone
            {
                respawn();
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
            isGameOver = platform.type.Equals(PlatformType.Exit);//if this platform is an exit platform, make the game end
            moveAlongPath();//otherwise, move along the path to the player selected platform
        }
        else
        {
            fall();// if there is no platform, make the pawn fall
        }
    }
    /// <summary>
    /// Makes the pawn fall in the current gravitational orientation
    /// </summary>
    private void fall()
    {

        adjustPawnRotation();

        Vector3 vec = getGravityVector(gravity); //vector pointing "down"

        if (platform != null) //if there is a platform beneath the Pawn
        {
            Vector3 ground = getGroundPosition();
            vec = platform.transform.position - ground; //change the vector pointing down to a vector pointing to the center of the platform. i.e. the Pawn gravitates towards the platform beneath him
        }

        //move the Pawn in the direction of the vector
        moveMe(vec);


    }

    /// <summary>
    /// Align the pawn with the platform "beneath" him, or to the axis relating the current gravitational orientation.
    /// Also determines the "animation" of this movement.
    /// </summary>
    private void adjustPawnRotation()
    {
        Vector3 from = getGroundPosition();
        Vector3 to = platform != null ? platform.transform.position : from;//Vector3.Cross(getGravityVector(),from) for increased smootheness

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(getPlayerRotation()), 1 / Math.Abs(Vector3.Magnitude(to - from)));

        //transform.rotation = Quaternion.Euler(getPlayerRotation());
    }

    /// <summary>
    /// Checks if the pawn is grounded.
    /// Answers the question " is the player touching a platform "beneath" him?" where beneath relates to the current gravitational orientation.
    /// </summary>
    private bool isGrounded()
    {
        if (platform != null) //is there even a platform beneath the Pawn
        {
            float proximityThreshold = 0.5f; // the value bellow which can be said that the platform is touching the Pawn, it's like an error margin
            if (gravity.Equals(PlatformOrientation.Up) || gravity.Equals(PlatformOrientation.Down)) //check for up and down
            {
                return Math.Abs(platform.transform.position.y - getGroundPosition().y) < proximityThreshold;//check distance
            }

            if (gravity.Equals(PlatformOrientation.Right) || gravity.Equals(PlatformOrientation.Left)) //check for left and right
            {
                return Math.Abs(platform.transform.position.x - getGroundPosition().x) < proximityThreshold;//check distance
            }

            if (gravity.Equals(PlatformOrientation.Front) || gravity.Equals(PlatformOrientation.Back)) //check for front and back
            {
                return Math.Abs(platform.transform.position.z - getGroundPosition().z) < proximityThreshold;//check distance
            }
        }
        return false; //if there isn't a platform beneath him, he isn't grounded
    }

    /// <summary>
    /// Moves along the path.
    /// If there is a path that has been previously decided the Pawn should take, finish the path.
    /// Otherwise, if there isn't a path, but there is a targetPlatform, a valid platform that the player has clicked. 
    /// This targetPlatform can be in a place not directly accessible to the Pawn, for example if there is a gap or if the platform is lower and the Pawn is supposed to fall.
    /// </summary>
    private void moveAlongPath()
    {
        if (path != null && path.Count > 0) //is there a path?
        {
            //if there is, move the pawn towards the next point in that path
            Vector3 vec = path[0].transform.position - getGroundPosition();
            if (moveMe(vec))
            {
                path.RemoveAt(0); //if we have reached this path point, delete it from the list so we can go to the next one next time
            }

        }
        else
            if (targetPlatform != null)//ok, there is no path, but is there a target platform? (this happends when the chosen platform is not directly accessible)
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
                        {
                            targetPlatform = null; //if we are already there, forget targetPlatform
                        }
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
            GetComponent<CharacterController>().Move(Vector3.Normalize(vec) * Time.deltaTime * speed);
            return false;
        }
        else
        {
            GetComponent<CharacterController>().Move(vec * Time.deltaTime * speed);
            return true;
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
        if (Physics.SphereCast(transform.position, 1.5f, getGravityVector(gravity), out hit, 10000, ~(1 << 10)))//casting a ray down, we need a sphereCast because the capsule has thickness, and we need to ignore the Pawn collider
        {
            p = hit.collider.gameObject.GetComponent<Platform>();
            if (p != null) //if it is a platform
            {
                platform = p;
            }
            else
            {
                platform = null;
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
            if (Physics.SphereCast(transform.position, 1.5f, getGravityVector(orientation), out hitc, 10000, ~(1 << 10)))//casting a ray down, we need a sphereCast because the capsule has thickness, and we need to ignore the Pawn collider
            {
                p = hitc.collider.gameObject.GetComponent<Platform>();
                if (p != null && p != platform)
                {
                    if (hud.dotIsInside)
                    {
                        getOrientationSphere(orientation).transform.position = p.transform.position;
                    }
                    else
                    {
                        getOrientationSphere(orientation).transform.position = p.transform.position - (getGravityVector(orientation) * hud.dotSize / 2);
                    }
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
    private float lastClick;
    private void manageMouse()
    {
        PlatformSelection.highlightTargetPlatform();
        Platform p = PlatformSelection.getPlatform();

        if (Input.GetMouseButtonDown(0))
        {
            lastClick = Time.time;
        }
        if (Input.GetMouseButtonUp(0) && ((lastClick + hud.ClickDelay)> Time.time))
        {
            if (p != null)
            {
                p.flashMe();
                if (platform == null || p.orientation != platform.orientation) //for punishing gravity take the platform == null here
                {
                }
                else
                {
                    path = AStarHelper.Calculate(platform, p);
                    targetPlatform = p;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && ((lastClick + hud.ClickDelay) > Time.time))
        {
            if (p != null)
            {

                if (platform == null || p.orientation != platform.orientation) //for punishing gravity take the platform == null here
                {
                    transform.position -= getGravityVector(gravity) * 4;
                    gravity = p.orientation;
                    hud.gravityChangeCount++;
                }
            }


        }
    }

    /// ----- CHECKERS ----- ///

    /// <summary>
    /// Is the target platform above the Pawn?
    /// </summary>
    private bool targetPlatformIsAbove(Platform target)
    {
        switch (gravity)
        {
            default:
                return platform.transform.position.y < target.transform.position.y;
            case PlatformOrientation.Down:
                return platform.transform.position.y > target.transform.position.y;
            case PlatformOrientation.Right:
                return platform.transform.position.x < target.transform.position.x;
            case PlatformOrientation.Left:
                return platform.transform.position.x > target.transform.position.x;
            case PlatformOrientation.Front:
                return platform.transform.position.z > target.transform.position.z;
            case PlatformOrientation.Back:
                return platform.transform.position.z < target.transform.position.z;
        }
    }
    /// <summary>
    /// Is the Pawn out of bounds?
    /// </summary>
    public void outOfBounds()
    {
        isGameOver = true;
    }

    /// ----- GETTERS ----- ///

    /// <summary>
    /// Gets the player rotation according to the current gravitational orientation.
    /// </summary>
    private Vector3 getPlayerRotation()
    {
        switch (gravity)
        {
            default:
                return Vector3.zero;
            case PlatformOrientation.Down:
                return new Vector3(0, 180, 180);
            case PlatformOrientation.Right:
                return new Vector3(0, 180, 90);
            case PlatformOrientation.Left:
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
        switch (gravity)
        {
            default:
                return new Vector3(transform.position.x, transform.position.y - 4f, transform.position.z);
            case PlatformOrientation.Down:
                return new Vector3(transform.position.x, transform.position.y + 4f, transform.position.z);
            case PlatformOrientation.Right:
                return new Vector3(transform.position.x - 4, transform.position.y, transform.position.z);
            case PlatformOrientation.Left:
                return new Vector3(transform.position.x + 4, transform.position.y, transform.position.z);
            case PlatformOrientation.Front:
                return new Vector3(transform.position.x, transform.position.y, transform.position.z + 4);
            case PlatformOrientation.Back:
                return new Vector3(transform.position.x, transform.position.y, transform.position.z - 4);
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
        if (gravity.Equals(PlatformOrientation.Up) || gravity.Equals(PlatformOrientation.Down))
        {
            return new Vector3(position.x, getGroundPosition().y, position.z);
        }
        else if (gravity.Equals(PlatformOrientation.Left) || gravity.Equals(PlatformOrientation.Right))
        {
            return new Vector3(getGroundPosition().x, position.y, position.z);
        }
        else
        {
            return new Vector3(position.x, position.y, getGroundPosition().z);
        }
    }

    /// <summary>
    /// Gets the game object of the sphere for a given PlatformOrientation
    /// </summary>
    private GameObject getOrientationSphere(PlatformOrientation orientation)
    {
        return orientationSpheres[(int)orientation];
    }
}
