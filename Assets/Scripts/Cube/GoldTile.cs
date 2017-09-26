using UnityEngine;
using System.Collections;

public class GoldTile : MonoBehaviour
{
	// all the tile will share the same material, either the active or inactive one
	public Material activeMaterial;
	public Material inactiveMaterial;
	public Material changeGravityMaterial;

	[Tooltip("The VFX to play once when the tile is activated and under the pawn")]
	public ParticleSystem activationVFX = null;

	[Tooltip("The VFX to play in loop when the tile is active")]
	public ParticleSystem activeVFX = null;

	private TileOrientation orientation;

	private bool isClickableToChangeGravity = false;
	public bool IsClickableToChangeGravity
	{
		get	{ return isClickableToChangeGravity; }
		set 
		{
			isClickableToChangeGravity = value;

			// get the world gravity if any
			TileOrientation worldGravityOrientation = TileOrientation.Up;
			if ((Pawn.Instance != null) && (World.Instance != null))
				worldGravityOrientation = World.Instance.CurrentGravityOrientation;

			// and change the material
			DefineMaterial(worldGravityOrientation);
		}
	}

	// Use this for initialization
	void Awake()
	{
		DefineOrientation();
		// The first material assignation doesn't really matter, because the reset will be called later
		// when the game start, so the correct orientation will be given at that time
		Reset(TileOrientation.Up);
	}
	
	public void Reset(TileOrientation startingOrientation)
	{
		DefineMaterial(startingOrientation);
	}

	public void ChangeGravity( TileOrientation gravityOrientation )
	{
		DefineMaterial(gravityOrientation);
	}

	private void DefineOrientation()
	{
		Vector3 tileDirection = transform.rotation * -Vector3.up;

		if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Up) ), 0 ) )
			orientation = TileOrientation.Up;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Down) ), 0 ) )
			orientation = TileOrientation.Down;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Right) ), 0 ) )
			orientation = TileOrientation.Right;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Left) ), 0 ) )
			orientation = TileOrientation.Left;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Front) ), 0 ) )
			orientation = TileOrientation.Front;
		else if ( Mathf.Approximately ( Vector3.Angle( tileDirection, World.getGravityVector(TileOrientation.Back) ), 0 ) )
			orientation = TileOrientation.Back;
	}

	/// <summary>
	/// Updates the orientation of the golden tile. This function should be called if the golden tile is rotated
	/// for example when the golden tile is attached to a moving platform performing a rotation, or a rotating
	/// platform, after a change of gravity.
	/// </summary>
	public void UpdateOrientation(TileOrientation gravityOrientation)
	{
		// redefine its orientation
		DefineOrientation();
		// and update its material
		DefineMaterial( gravityOrientation );
	}

	/// <summary>
	/// Defines the correct material to set. In priority, if the tile can be targeted to change the gravity
	/// we use that material, otherwise we use an active or inactive material depending if the tile is
	/// oriented like the gravity.
	/// However, in edit mode (when the designer build the level) we use special edition material to display
	/// the face orientation.
	/// </summary>
	/// <param name="gravityOrientation">The current orientation of the gravity.</param>
	private void DefineMaterial( TileOrientation gravityOrientation )
	{
		MeshRenderer mesh = GetComponent<MeshRenderer>();

		// a flag to tell if the active VFX should be played or stopped
		bool playActiveVFX = false;

		if (isClickableToChangeGravity)
		{
			mesh.material = changeGravityMaterial;
			mesh.enabled = (changeGravityMaterial != null);
			playActiveVFX = true;
		}
		else if ( gravityOrientation == orientation )
		{
			mesh.material = activeMaterial;
			mesh.enabled = (activeMaterial != null);
			playActiveVFX = true;
		}
		else
		{
			mesh.material = inactiveMaterial;
			mesh.enabled = (inactiveMaterial != null);
			playActiveVFX = false;
		}

		// also play/stop the active VFX if any
		if (activeVFX != null)
		{
			if (playActiveVFX)
				activeVFX.Play();
			else
				activeVFX.Stop();
		}

		// in editor mode we display the material to show the orientation of the tile
		#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			switch (this.orientation)
			{
			case TileOrientation.Up:
				mesh.material = Assets.getUpBlockMat();
				break;
			case TileOrientation.Down:
				mesh.material = Assets.getDownBlockMat();
				break;
			case TileOrientation.Left:
				mesh.material = Assets.getLeftBlockMat();
				break;
			case TileOrientation.Right:
				mesh.material = Assets.getRightBlockMat();
				break;
			case TileOrientation.Front:
				mesh.material = Assets.getFrontBlockMat();
				break;
			case TileOrientation.Back:
				mesh.material = Assets.getBackBlockMat();
				break;
			}
		}
		#endif
	}

	/// <summary>
	/// Call this function if you want to play the VFX for when the tile is activated
	/// </summary>
	public void playActivationVFX()
	{
		if (activationVFX != null)
			activationVFX.Play();
	}
}
