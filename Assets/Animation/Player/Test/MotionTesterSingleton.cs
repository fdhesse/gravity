using NUnit.Framework;
using UnityEngine;

public class MotionTesterSingleton : MonoBehaviour
{
    public static MotionTesterSingleton Instance;

    public GameObject ClimbDownPrefab;

    GameObject pawnTester;

    public void Awake()
    {
        Assert.IsNotNull( ClimbDownPrefab );
    }

    public void Start()
    {
        Instance = this;
    }

    public void SpawnClimbDownPrefab()
    {
        Time.timeScale = 0.2f;
        pawnTester = Instantiate( ClimbDownPrefab, Pawn.Instance.transform.position, Quaternion.Euler( 0, -90, 0 ) );
    }

    public void DestroyLastSpawnedPawnTester()
    {
        Destroy( pawnTester );
        Time.timeScale = 1f;
    }
}