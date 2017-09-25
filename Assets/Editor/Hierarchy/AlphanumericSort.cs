using UnityEditor;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
public class AlphaNumericSort : BaseHierarchySort
#pragma warning restore CS0618 // Type or member is obsolete
{
	public override int Compare( GameObject lhs, GameObject rhs )
	{
		if (lhs == rhs)
			return 0;
		if (lhs == null)
			return -1;
		if (rhs == null)
			return 1;

		return EditorUtility.NaturalCompare (lhs.name, rhs.name);
	}
}