#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Tile))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
#if UNITY_EDITOR
[InitializeOnLoad]
[ExecuteInEditMode]
#endif
public class Stairway : MonoBehaviour {

	//public Tile targetUp = null;
	//public Tile targetDown = null;
	
	// 8 directions (I stands for inverted):
	// Left, Right, Front, Back
	// ILeft, IRgiht, IFront, IBack
	public enum StairwayDirection
	{
		Left,ILeft,Right,IRight,Front,IFront,Back,IBack
	}
	
	public enum StairwayAxis
	{
		X,Y,Z
	}

	public bool inverted;
	
	[ExposeProperty]
	public bool Invert
	{
		get { return inverted; }
		set	{ if(value != inverted) { inverted = value; SetAxis( stairwayAxis ); } }
	}

	[HideInInspector] [SerializeField] private StairwayDirection stairwayDirection;
	[HideInInspector] [SerializeField] private StairwayAxis stairwayAxis;

	//[ExposeProperty]
	public StairwayDirection Direction
	{
		get { return stairwayDirection; }
		set	{ if(value != stairwayDirection) { SetDirection( value ); } }
	}

	[ExposeProperty]
	public StairwayAxis Axis
	{
		get { return stairwayAxis; }
		set	{ if(value != stairwayAxis) { SetAxis( value ); } }
	}

	public Tile LookForSiblingTile( Tile previousTile )
	{
		if ( previousTile == null )
			return null;

		// Propage l'orientation de la plateforme source
		Tile stairTile = GetComponent<Tile>();
		stairTile.orientation = previousTile.orientation;

		Collider[] hits = Physics.OverlapSphere(transform.position, 8.5f);

		// Sphere de debugage
		//GameObject go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		//go.GetComponent<SphereCollider>().radius = 8.5f;
		//go.transform.position = this.transform.position;

		foreach( Collider hit in hits )
		{
			//Debug.Log ("Il y a " + hits.Length + " collisions" );

			if ( hit.transform != previousTile.transform && hit.transform != transform )
			{
				//Debug.Log ("Qui ne sont pas la précédente plateforme" );
				if ( stairwayAxis == StairwayAxis.X && !Mathf.Approximately( hit.transform.position.z, transform.position.z) )
					continue;
				else if ( stairwayAxis == StairwayAxis.Y && !Mathf.Approximately( hit.transform.position.y, transform.position.y) )
					continue;
				else if ( stairwayAxis == StairwayAxis.Z && !Mathf.Approximately( hit.transform.position.x, transform.position.x) )
					continue;

				Stairway stair = hit.GetComponent<Stairway>();
				if ( stair != null )
					return stair.LookForSiblingTile( stairTile );
				
				Tile p = hit.gameObject.GetComponent<Tile>();
				if (p != null && p.orientation.Equals(stairTile.orientation) )
					return p;
			}
		}
		//Debug.Log ("Mais rien de bon n'a été trouvé");

		return null;
	}
	
	void Start()
	{
		//type = TileType.Valid;

		gameObject.layer = LayerMask.NameToLayer ("Tiles");
		gameObject.tag = "Stairway";
		
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Plane);
		GetComponent<MeshFilter> ().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
		DestroyImmediate (go);

		BoxCollider boxCollider = GetComponent<BoxCollider> ();
		boxCollider.size = new Vector3 (10, 0.001f, 10);
	}

	// <summary>
	// Set stairway's axis
	// </summary>
	private void SetAxis( StairwayAxis axis )
	{
		Vector3 scale = Vector3.one;

		if ( axis == StairwayAxis.X )
		{
			scale.x = Mathf.Sqrt (2);

			if ( !inverted )
				transform.rotation = Quaternion.Euler( new Vector3( 0, 0, -45f ) );
			else
				transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 135f ) );
		}
		else if ( axis == StairwayAxis.Y )
		{
			scale.z = Mathf.Sqrt (2);

			if ( !inverted )
				transform.rotation = Quaternion.Euler( new Vector3( 0, -45f, 90f ) );
			else
				transform.rotation = Quaternion.Euler( new Vector3( 0, -135f, 90f ) );
		}
		else if ( axis == StairwayAxis.Z )
		{
			scale.z = Mathf.Sqrt (2);

			if ( !inverted )
				transform.rotation = Quaternion.Euler( new Vector3( -45f, 0, 0 ) );
			else
				transform.rotation = Quaternion.Euler( new Vector3( 135f, 0, 0 ) );
		}

		stairwayAxis = axis;
		transform.localScale = scale;
	}

	// <summary>
	// Set stairway's direction
	// </summary>
	private void SetDirection( StairwayDirection direction )
	{
		stairwayDirection = direction;

		Vector3 scale = Vector3.one;
		
		if ( direction == StairwayDirection.Front )
		{
			transform.rotation = Quaternion.Euler( new Vector3( -45f, 0, 0 ) );
			scale.z = Mathf.Sqrt (2);
		}
		else if ( direction == StairwayDirection.IFront )
		{
			transform.rotation = Quaternion.Euler( new Vector3( -135f, 0, 0 ) );
			scale.z = Mathf.Sqrt (2);
		}
		else if ( direction == StairwayDirection.Back )
		{
			transform.rotation = Quaternion.Euler( new Vector3( 45f, 0, 0 ) );
			scale.z = Mathf.Sqrt (2);
		}
		else if ( direction == StairwayDirection.IBack )
		{
			transform.rotation = Quaternion.Euler( new Vector3( 135f, 0, 0 ) );
			scale.z = Mathf.Sqrt (2);
		}
		else if ( direction == StairwayDirection.Right )
		{
			transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 45f ) );
			scale.x = Mathf.Sqrt (2);
		}
		else if ( direction == StairwayDirection.IRight )
		{
			transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 135f ) );
			scale.x = Mathf.Sqrt (2);
		}
		else if ( direction == StairwayDirection.Left )
		{
			transform.rotation = Quaternion.Euler( new Vector3( 0, 0, -45f ) );
			scale.x = Mathf.Sqrt (2);
		}
		else if ( direction == StairwayDirection.ILeft )
		{
			transform.rotation = Quaternion.Euler( new Vector3( 0, 0, -135f ) );
			scale.x = Mathf.Sqrt (2);
		}
		transform.localScale = scale;


		/*
		else if ( direction == StairwayDirection.Up )
		{
			transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 225f ) );
			scale.x = Mathf.Sqrt (2);
			transform.localScale = scale;
		}
		else if ( direction == StairwayDirection.Down )
		{
			transform.rotation = Quaternion.Euler( new Vector3( 0, 0, -45f ) );
			scale.x = Mathf.Sqrt (2);
			transform.localScale = scale;
		}
		 */
	}

#if UNITY_EDITOR
	private Vector3 arrowDirection;
	private Vector3 arrowPosition;
	
	[System.NonSerialized]
	private bool isInitialized;

	Stairway()
	{
		isInitialized = false;
	}

	void Update()
	{
		if ( !isInitialized )
		{
			Start ();
			isInitialized = true;
		}
		
		if ( stairwayAxis == StairwayAxis.X )
			arrowDirection = transform.right;
		else if ( stairwayAxis == StairwayAxis.Y )
				arrowDirection = transform.forward;
		else if ( stairwayAxis == StairwayAxis.Z )
			arrowDirection = transform.forward;

		/*
		if ( stairwayDirection == StairwayDirection.Front )
			arrowDirection = transform.forward;
		if ( stairwayDirection == StairwayDirection.IFront )
			arrowDirection = transform.forward;
		else if ( stairwayDirection == StairwayDirection.Back )
			arrowDirection = -transform.forward;
		else if ( stairwayDirection == StairwayDirection.IBack )
			arrowDirection = -transform.forward;
		else if ( stairwayDirection == StairwayDirection.Right )
			arrowDirection = transform.right;
		else if ( stairwayDirection == StairwayDirection.IRight )
			arrowDirection = transform.right;
		else if ( stairwayDirection == StairwayDirection.Left )
			arrowDirection = -transform.right;
		else if ( stairwayDirection == StairwayDirection.ILeft )
			arrowDirection = -transform.right;
		*/
		/*
		else if ( stairwayDirection == StairwayDirection.Up )
			arrowDirection = -transform.right;
		else if ( stairwayDirection == StairwayDirection.Down )
			arrowDirection = transform.right;
		*/

		arrowDirection *= 10f;
		arrowPosition = transform.position -arrowDirection * 0.5f;
	}

	void OnDrawGizmosSelected()
	{
		Quaternion arrowRotation = Quaternion.LookRotation ( arrowDirection );

		Handles.color = Color.cyan;
		Handles.ArrowCap( 0, arrowPosition, arrowRotation, 10f );

		arrowRotation = Quaternion.LookRotation ( -arrowDirection );
		Vector3 arrow2Position = arrowPosition + arrowDirection;
		Handles.ArrowCap( 0, arrow2Position, arrowRotation, 10f );

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (transform.position, Vector3.one * 10f);
	}
#endif

	/*
	protected override void scanNearbyTiles()
	{
		targetUp = null;
		targetDown = null;
		connectionSet = new HashSet<Tile>();
		Collider[] hits = Physics.OverlapSphere(transform.position, 7.5f);
		
		foreach (Collider hit in hits)
		{
			if (hit.collider.transform != transform)
			{
				Tile p = hit.gameObject.GetComponent<Tile>();

				if (p != null && p.orientation.Equals(orientation))
				{
					// Ignore les plateformes qui ne sont pas dans l'axe de l'escalier
					if ( stairwayDirection == StairwayDirection.Right )
					{
						if ( p.transform.parent.position.z != transform.parent.position.z )
							continue;

						if ( p.transform.parent.position.y < transform.parent.position.y )
							targetDown = p;
						else
							targetUp = p;
					}
					else if ( stairwayDirection == StairwayDirection.Left )
					{
						if ( p.transform.parent.position.z != transform.parent.position.z )
							continue;
						
						if ( p.transform.parent.position.y < transform.parent.position.y )
							targetUp = p;
						else
							targetDown = p;
					}
					if ( stairwayDirection == StairwayDirection.Up )
					{
						if ( p.transform.parent.position.z != transform.parent.position.z )
							continue;
						
						if ( p.transform.parent.position.x < transform.parent.position.x )
							targetDown = p;
						else
							targetUp = p;
					}
					else if ( stairwayDirection == StairwayDirection.Down )
					{
						if ( p.transform.parent.position.z != transform.parent.position.z )
							continue;
						
						if ( p.transform.parent.position.x < transform.parent.position.x )
							targetDown = p;
						else
							targetUp = p;
					}
					else if ( stairwayDirection == StairwayDirection.Front )
					{
						if ( p.transform.parent.position.x != transform.parent.position.x )
							continue;
						
						if ( p.transform.parent.position.y < transform.parent.position.y )
							targetDown = p;
						else
							targetUp = p;
					}
					else if ( stairwayDirection == StairwayDirection.Back )
					{
						if ( p.transform.parent.position.x != transform.parent.position.x )
							continue;
						
						if ( p.transform.parent.position.y < transform.parent.position.y )
							targetDown = p;
						else
							targetUp = p;
					}

					if (rescanPath)
						p.rescanPath = true;
					
					connectionSet.Add(p);
					
					_connections = new Transform[connectionSet.Count];
					connections = new List<Tile>(connectionSet);
					for (int i = 0; i != connections.Count; i++)
					{
						_connections[i] = connections[i].transform;
					}
				}
			}
		}
	}*/
}
