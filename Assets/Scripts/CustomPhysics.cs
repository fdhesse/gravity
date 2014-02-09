using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class CustomPhysics : MonoBehaviour
{

    public GameObject player;
    public float force = 10f;
    private Pawn pawn;
    void Start()
    {
        pawn = player.GetComponent<Pawn>();
    }

    // Update is called once per frame
    void Update()
    {

        rigidbody.useGravity = false;
            rigidbody.AddForce(pawn.getGravityVector(pawn.gravity) * force);

    }
}
