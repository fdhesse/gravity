#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Tile))]
//[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
#if UNITY_EDITOR
[InitializeOnLoad]
[ExecuteInEditMode]
#endif
public class Stairway : MonoBehaviour {
	
	public enum StairwayAxis
	{
		None,X,Y,Z
	}

	// the stairway axis is perpendicular to the stairway path way, because
	// a stairway can be used in the plane of the its path way. For example
	// if the stairways axis is Z, then the path that include the stairways
	// can run along the X axis, or along the Y axis, so in the XY plane
	[HideInInspector] [SerializeField] private StairwayAxis stairwayAxis;

	[ExposeProperty]
	public StairwayAxis Axis
	{
		get { return stairwayAxis; }
		set	{ stairwayAxis = value; }
	}

	public Tile LookForSiblingTile( Tile previousTile )
	{
		if ( previousTile == null )
			return null;

		// Propage l'orientation de la plateforme source
		Tile stairTile = GetComponent<Tile>();
		stairTile.orientation = previousTile.orientation;

		Collider[] hits = Physics.OverlapSphere(transform.position, 8.5f);

		foreach( Collider hit in hits )
		{
			// Si il ne s'agit ni de la tile précédente ni de la tile actuelle
			if ( hit.transform != previousTile.transform && hit.transform != transform )
			{
				// exclude all the block tile that are not in the plane of the stairway path
				if ( stairwayAxis == StairwayAxis.X )
				{
					if ( Mathf.RoundToInt( hit.transform.position.x ) != Mathf.RoundToInt( transform.position.x ) )
						continue;
				}
				else if ( stairwayAxis == StairwayAxis.Y )
				{
					if ( Mathf.RoundToInt( hit.transform.position.y ) != Mathf.RoundToInt( transform.position.y ) )
						continue;
				}
				else if ( stairwayAxis == StairwayAxis.Z )
				{
					if ( Mathf.RoundToInt( hit.transform.position.z ) != Mathf.RoundToInt( transform.position.z ) )
						continue;
				}

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
		stairwayAxis = StairwayAxis.None;

		// X
		if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 0, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (135f, 0, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (225f, 0, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (315f, 0, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 180f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (135f, 180f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (225f, 180f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (315f, 180f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		// Y
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (0, 45f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (0, 135f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (0, 225f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (0, 315f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (180f, 45f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (180f, 135f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (180f, 225f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (180f, 315f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		// Z
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 90f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (135f, 90f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (225f, 90f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (315f, 90f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 270f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (135f, 270f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (225f, 270f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (315f, 270f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}

		//type = TileType.Valid;

		gameObject.layer = LayerMask.NameToLayer ("Tiles");
		gameObject.tag = "Stairway";
		
		//GameObject go = GameObject.CreatePrimitive (PrimitiveType.Plane);
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Quad);
		GetComponent<MeshFilter> ().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
		DestroyImmediate (go);

		MeshCollider meshCollider = GetComponent<MeshCollider> ();

		if ( meshCollider == null )
		{
			DestroyImmediate (GetComponent<BoxCollider> ());
			meshCollider = gameObject.AddComponent<MeshCollider>();
		}

		meshCollider.sharedMesh = GetComponent<MeshFilter> ().sharedMesh;
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

		arrowDirection = transform.right;
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
}
