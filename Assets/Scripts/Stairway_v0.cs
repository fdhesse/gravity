#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Stairway_v0 : Platform {

	public Platform targetUp = null;
	public Platform targetDown = null;

	public enum StairwayDirection
	{
		Up,Down,Left,Right,Front,Back
	}

	public StairwayDirection stairwayDirection;


#if UNITY_EDITOR
	private Vector3 arrowDirection;
	private Vector3 arrowPosition;

	new void Update()
	{
		if ( stairwayDirection == StairwayDirection.Front )
			arrowDirection = transform.forward;
		else if ( stairwayDirection == StairwayDirection.Back )
			arrowDirection = -transform.forward;
		else if ( stairwayDirection == StairwayDirection.Right )
			arrowDirection = -transform.right;
		else if ( stairwayDirection == StairwayDirection.Left )
			arrowDirection = transform.right;
		else if ( stairwayDirection == StairwayDirection.Up )
			arrowDirection = transform.up;
		else if ( stairwayDirection == StairwayDirection.Down )
			arrowDirection = -transform.up;

		arrowDirection *= 10f;
		arrowPosition = transform.position -arrowDirection * 0.5f;
		
		if ( orientation == PlatformOrientation.Up )
			arrowPosition.y += 5;
		if ( orientation == PlatformOrientation.Down )
			arrowPosition.y -= 5;
		if ( orientation == PlatformOrientation.Left )
			arrowPosition.x += 5;
		if ( orientation == PlatformOrientation.Right )
			arrowPosition.x -= 5;
		if ( orientation == PlatformOrientation.Front )
			arrowPosition.z -= 5;
		if ( orientation == PlatformOrientation.Back )
			arrowPosition.z += 5;

		base.Update ();
	}

	void OnDrawGizmosSelected()
	{
		Quaternion arrowRotation = Quaternion.LookRotation ( arrowDirection );

		Handles.color = Color.cyan;
		Handles.ArrowCap( 0, arrowPosition, arrowRotation, 8.8f );

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (transform.position, Vector3.one * 10f);
	}
#endif
	
	protected override void scanNearbyPlatforms()
	{
		targetUp = null;
		targetDown = null;
		connectionSet = new HashSet<Platform>();
		Collider[] hits = Physics.OverlapSphere(transform.position, 7.5f);
		
		foreach (Collider hit in hits)
		{
			if (hit.GetComponent<Collider>().transform != transform)
			{
				Platform p = hit.gameObject.GetComponent<Platform>();

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
					connections = new List<Platform>(connectionSet);
					for (int i = 0; i != connections.Count; i++)
					{
						_connections[i] = connections[i].transform;
					}
				}
			}
		}
	}
}
