using System;
using NUnit.Framework;
using UnityEngine;

public class CinematicAnimationSingleton : MonoBehaviour
{
    public static CinematicAnimationSingleton Instance;

    public GameObject ClimbDownPrefab;
    public GameObject JumpDownTwoTilesPrefab;
    public GameObject JumpDownThreeTilesPrefab;
    public GameObject JumpDownFourTilesPrefab;
    public GameObject JumpDownFiveTilesPrefab;
    public GameObject JumpDownSixTilesPrefab;
    public GameObject JumpDownSevenTilesPrefab;

    GameObject cinematicPawn;

    public void Awake()
    {
        Assert.IsNotNull( ClimbDownPrefab );
        Assert.IsNotNull( JumpDownTwoTilesPrefab );
        Assert.IsNotNull( JumpDownThreeTilesPrefab );
        Assert.IsNotNull( JumpDownFourTilesPrefab );
        Assert.IsNotNull( JumpDownFiveTilesPrefab );
        Assert.IsNotNull( JumpDownSixTilesPrefab );
        Assert.IsNotNull( JumpDownSevenTilesPrefab );
    }

    public void Start()
    {
        Instance = this;
    }

    public void SpawnClimbDownPrefab( Vector3 axialDisplacement )
    {
        //Time.timeScale = 0.2f;
        cinematicPawn = Instantiate( ClimbDownPrefab, Pawn.Instance.transform.position,
            Quaternion.Euler( axialDisplacement ) );
    }

    public void DestroyCinematicPawn()
    {
        Destroy( cinematicPawn );
        //Time.timeScale = 1f;
    }

    public void SpawnJumpPrefab( Vector3 axialDisplacementAngles, float fallDistance )
    {
        GameObject prefab;
        switch ( Mathf.CeilToInt( fallDistance ) )
        {
            case 20:
                prefab = JumpDownTwoTilesPrefab;
                break;
            case 30:
                prefab = JumpDownThreeTilesPrefab;
                break;
            case 40:
                prefab = JumpDownFourTilesPrefab;
                break;
            case 50:
                prefab = JumpDownFiveTilesPrefab;
                break;
            case 60:
                prefab = JumpDownSixTilesPrefab;
                break;
            case 70:
                prefab = JumpDownSevenTilesPrefab;
                break;
            default:
                Debug.LogError( "Pawn needs to jump, can't find how high" );
                throw new Exception("Failed Jump");
        }

        cinematicPawn = Instantiate( prefab, Pawn.Instance.transform.position, Quaternion.Euler( axialDisplacementAngles ) );
    }
}