using NUnit.Framework;
using UnityEngine;

public class CinematicAnimationSingleton : MonoBehaviour
{
    public static CinematicAnimationSingleton Instance;

    public GameObject ClimbDownPrefab;

    GameObject cinematicPawn;

    public void Awake()
    {
        Assert.IsNotNull( ClimbDownPrefab );
    }

    public void Start()
    {
        Instance = this;
    }

    public void SpawnClimbDownPrefab( Vector3 axialDisplacement )
    {
        //Time.timeScale = 0.2f;
        cinematicPawn = Instantiate( ClimbDownPrefab, Pawn.Instance.transform.position, Quaternion.Euler( axialDisplacement ) );
    }

    public void DestroyCinematicPawn()
    {
        Destroy( cinematicPawn );
        //Time.timeScale = 1f;
    }
}