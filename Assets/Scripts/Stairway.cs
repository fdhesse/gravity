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
	
	public enum StairwayAxis
	{
		X,Y,Z
	}

	private bool inverted;
	
	[ExposeProperty]
	public bool Invert
	{
		get { return inverted; }
		set	{ if(value != inverted) { inverted = value; SetAxis( stairwayAxis ); } }
	}

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

		// Sphere de debugage
		//GameObject go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		//go.GetComponent<SphereCollider>().radius = 8.5f;
		//go.transform.position = this.transform.position;

		foreach( Collider hit in hits )
		{
			if ( hit.transform != previousTile.transform && hit.transform != transform )
			{
				//Debug.Log ("Qui ne sont pas la précédente plateforme" );
				if ( stairwayAxis == StairwayAxis.X && Mathf.RoundToInt( hit.transform.position.z ) != Mathf.RoundToInt( transform.position.z ) )
					continue;
				else if ( stairwayAxis == StairwayAxis.Y && Mathf.RoundToInt( hit.transform.position.y ) != Mathf.RoundToInt( transform.position.y ) )
					continue;
				else if ( stairwayAxis == StairwayAxis.Z && Mathf.RoundToInt( hit.transform.position.x ) != Mathf.RoundToInt( transform.position.x ) )
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
