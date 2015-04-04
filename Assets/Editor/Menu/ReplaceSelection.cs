using UnityEditor;
using UnityEngine;

public class ReplaceSelection : EditorWindow
{
	GameObject myObject;

	[MenuItem ("Tools/ReplaceSelected %g")]
	public static void ReplaceObjects() {
		
		EditorWindow.GetWindow(typeof(ReplaceSelection));
	}        
	
	void OnGUI () {
		
		GUILayout.Label ("Use Object", EditorStyles.boldLabel);
		
		myObject = EditorGUILayout.ObjectField(myObject, typeof(GameObject), true) as GameObject;
		
		if (GUILayout.Button ("Replace Selected")) {            
			
			if (myObject != null) {
				
				foreach (Transform t in Selection.transforms) {
					
					GameObject o = null;
					o = PrefabUtility.GetPrefabParent(myObject) as GameObject;                    
					
					if (PrefabUtility.GetPrefabType(myObject).ToString() == "PrefabInstance") {
						
						o = (GameObject)PrefabUtility.InstantiatePrefab(o);
						PrefabUtility.SetPropertyModifications(o, PrefabUtility.GetPropertyModifications(myObject));
					}                    
					
					else if (PrefabUtility.GetPrefabType(myObject).ToString() == "Prefab") {
						
						o = (GameObject)PrefabUtility.InstantiatePrefab(myObject);
					}                    
					
					else {
						
						o = Instantiate(myObject) as GameObject;
					}                    
					
					Undo.RegisterCreatedObjectUndo(o, "created prefab");
					
					Transform newT = o.transform;
					
					if(t != null){                     
						
						newT.position = t.position;
						newT.rotation = t.rotation;
						newT.localScale = t.localScale;
						newT.parent = t.parent;  
					}
				}
				
				foreach (GameObject go in Selection.gameObjects) {
					
					Undo.DestroyObjectImmediate(go);
				}
			}
		}
	}
}