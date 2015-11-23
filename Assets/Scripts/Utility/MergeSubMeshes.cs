using UnityEngine;
using System.Collections;

public class MergeSubMeshes : MonoBehaviour
{
	// Use this for initialization
	void Awake()
	{
		MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
		int subMeshCount = meshFilter.sharedMesh.subMeshCount;
		CombineInstance[] combine = new CombineInstance[subMeshCount];
		for (int i = 0 ; i < subMeshCount; ++i)
		{
			combine[i].mesh = meshFilter.sharedMesh;
			combine[i].subMeshIndex = i;
		}

		// get the current active state of the object and desactive it
		bool activeSelfState = meshFilter.gameObject.activeSelf;
		meshFilter.gameObject.SetActive(false);
		// combine the sub meshes
		meshFilter.mesh.CombineMeshes(combine, true, false);
		// reset the active state
		transform.gameObject.SetActive(activeSelfState);
	}
}
