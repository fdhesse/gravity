using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class GravityPlatform : MonoBehaviour {
	
/*	[HideInInspector] [SerializeField] private PlatformType m_left;
	[HideInInspector] [SerializeField] private PlatformType m_right;
	[HideInInspector] [SerializeField] private PlatformType m_up;
	[HideInInspector] [SerializeField] private PlatformType m_down;
	[HideInInspector] [SerializeField] private PlatformType m_front;
	[HideInInspector] [SerializeField] private PlatformType m_back;
	
	[ExposeProperty]
	public PlatformType Left
	{
		get { return m_left; }
		set	{ if(value != m_left) { SetFace( "left", value ); m_left = value; } }
//		set	{ if(value != m_left) { __needsUpdate = true; m_left = value; } }
	}
	[ExposeProperty]
	public PlatformType Right
	{
		get { return m_right; }
		set	{ if(value != m_right) { SetFace( "right", value ); m_right = value; } }
//		set	{ if(value != m_left) { __needsUpdate = true; m_right = value; } }
	}
	[ExposeProperty]
	public PlatformType Up
	{
		get { return m_up; }
		set	{ if(value != m_up) { SetFace( "up", value ); m_up = value; } }
//		set	{ if(value != m_up) { __needsUpdate = true; m_up = value; } }
	}
	[ExposeProperty]
	public PlatformType Down
	{
		get { return m_down; }
		set	{ if(value != m_down) { SetFace( "down", value ); m_down = value; } }
//		set	{ if(value != m_down) { __needsUpdate = true; m_down = value; } }
	}
	[ExposeProperty]
	public PlatformType Front
	{
		get { return m_front; }
		set	{ if(value != m_front) { SetFace( "front", value ); m_front = value; } }
//		set	{ if(value != m_front) { __needsUpdate = true; m_front = value; } }
	}
	[ExposeProperty]
	public PlatformType Back
	{
		get { return m_back; }
		set	{ if(value != m_back) { SetFace( "back", value ); m_back = value; } }
	}
*/	
	public enum ConstraintAxis { X, Y, Z };

	public float from = 0;
	public float to = 10;

	public ConstraintAxis constrainedAxis;

	private bool freezed;

	private float position
	{
		get {
			if( constrainedAxis == ConstraintAxis.X )
				return transform.position.x;
			if( constrainedAxis == ConstraintAxis.Y )
				return transform.position.y;
			if ( constrainedAxis == ConstraintAxis.Z )
				return transform.position.z;

			return 0f;
			}
		set	{
			if( constrainedAxis == ConstraintAxis.X )
				transform.position = new Vector3( value, transform.position.y, transform.position.z );
			if( constrainedAxis == ConstraintAxis.Y )
				transform.position = new Vector3( transform.position.x, value, transform.position.z );
			if ( constrainedAxis == ConstraintAxis.Z )
				transform.position = new Vector3( transform.position.x, transform.position.y, value );
		}
	}

	// Use this for initialization
	void Start () {
		Reset ();
	}
	
	// Update is called once per frame
	void Update () {
		if ( !(position <= to && position >= from) && !freezed )
			Freeze();
		else if ( freezed && (position > to || position < from) )
			Unfreeze();
	}

	public void Reset()
	{
		position = from;
		
		Freeze ();
		Unfreeze ();

//		Debug.Log (rigidbody.constraints);
	}

	public void Unfreeze()
	{
		freezed = false;

		if( constrainedAxis == ConstraintAxis.X )
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation | ~RigidbodyConstraints.FreezePositionX;
		if( constrainedAxis == ConstraintAxis.Y )
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation | ~RigidbodyConstraints.FreezePositionY;
		if( constrainedAxis == ConstraintAxis.Z )
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation | ~RigidbodyConstraints.FreezePositionZ;
	}

	private void Freeze()
	{
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;

		rigidbody.constraints = RigidbodyConstraints.FreezeAll;

		if (position > to)
			position = to - 1;
		else if (position < from)
			position = from + 1;

		freezed = true;
	}
}
