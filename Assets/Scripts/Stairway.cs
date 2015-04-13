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

	private bool invertedUp;

	[HideInInspector] [SerializeField] private StairwayAxis stairwayAxis;

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

		foreach( Collider hit in hits )
		{
			// Si il ne s'agit ni de la tile précédente ni de la tile actuelle
			if ( hit.transform != previousTile.transform && hit.transform != transform )
			{
				//Debug.Log ("Qui ne sont pas la précédente plateforme" );
				if ( stairwayAxis == StairwayAxis.X )
				{
					if ( !invertedUp && Mathf.RoundToInt( hit.transform.position.z ) != Mathf.RoundToInt( transform.position.z ) )
						continue;
					else if ( invertedUp && Mathf.RoundToInt( hit.transform.position.y ) != Mathf.RoundToInt( transform.position.y ) )
						continue;
				}

				else if ( stairwayAxis == StairwayAxis.Y )
				{
					if ( !invertedUp && Mathf.RoundToInt( hit.transform.position.z ) != Mathf.RoundToInt( transform.position.z ) )
						continue;
				}
				else if ( stairwayAxis == StairwayAxis.Z )
				{
					if ( !invertedUp && Mathf.RoundToInt( hit.transform.position.x ) != Mathf.RoundToInt( transform.position.x ) )
						continue;
					else if ( invertedUp && Mathf.RoundToInt( hit.transform.position.y ) != Mathf.RoundToInt( transform.position.y ) )
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
		invertedUp = false;
		
		// X
		if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (0, 45f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
			invertedUp = true;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (0, -45f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
			invertedUp = true;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 90f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (-45f, 90f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 270f, 270f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		// X upside-down
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (-45f, 270f, 90f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (0, 135f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
			invertedUp = true;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (0, 225f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.X;
			invertedUp = true;
		}
		// Y
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 90f, 180f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (-45f, 90f, 180f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 270f, 180f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (-45f, 270f, 180f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Y;
		}
		// Z
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 0, 180f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (-45f, 0, 180f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (-45f, 180f, 0) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (45f, 180f, 180f) ))) < 0.1f)
		{
			stairwayAxis = StairwayAxis.Z;
		}
		else if (Mathf.Abs ( Quaternion.Angle(transform.rotation, Quaternion.Euler( new Vector3 (-45f, 180f, 180f) ))) < 0.1f)
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

	// <summary>
	// Set stairway's axis
	// </summary>
	private void SetAxis( StairwayAxis axis )
	{
		Vector3 scale = Vector3.one;

		if ( axis == StairwayAxis.X )
		{
			scale.x = Mathf.Sqrt (2);
			transform.rotation = Quaternion.Euler( new Vector3( 45f, -90f, -90f ) );
		}
		else if ( axis == StairwayAxis.Y )
		{
			scale.y = Mathf.Sqrt (2);
			transform.rotation = Quaternion.Euler( new Vector3( 0, -45f, 90f ) );
		}
		else if ( axis == StairwayAxis.Z )
		{
			scale.y = Mathf.Sqrt (2);
			transform.rotation = Quaternion.Euler( new Vector3( 135f, 0, 0 ) );
		}

		stairwayAxis = axis;
		transform.localScale = scale * 10;
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
			arrowDirection = transform.up;
		else if ( stairwayAxis == StairwayAxis.Z )
			arrowDirection = transform.up;

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
