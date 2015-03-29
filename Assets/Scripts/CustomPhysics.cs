using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class CustomPhysics : MonoBehaviour
{

    public GameObject player;
	public float force = 10f;
//	[EDIT]: commented 1 line
//    private Pawn pawn;
    void Start()
    {
//        pawn = player.GetComponent<Pawn>();
    }

    // Update is called once per frame
    void Update()
    {

		GetComponent<Rigidbody>().useGravity = true;
//	[EDIT]: commented 1 line
 //           rigidbody.AddForce(pawn.getGravityVector(pawn.gravity) * force);

    }
}
