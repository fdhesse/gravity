using UnityEditor;
using UnityEngine;

// IngredientDrawer
[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(StringIntDictionary))]
[CustomPropertyDrawer(typeof(StringParticleSystemDictionary))]
[CustomPropertyDrawer(typeof(StringGameObjectDictionary))]
[CustomPropertyDrawer(typeof(StringAudioSourceDictionary))]
[CustomPropertyDrawer(typeof(StringRandomSoundPitcherDictionary))]
public class MonoDictionaryDrawer : PropertyDrawer
{
	// Draw the property inside the given rect
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);

		GUIStyle warningStyle = new GUIStyle();
		warningStyle.richText = true;

		// memorise the indent level as we will play with it
		int indentLevel = EditorGUI.indentLevel;

		// get the two serialized list properties
		SerializedProperty keys = property.FindPropertyRelative("m_Keys");
		SerializedProperty values = property.FindPropertyRelative("m_Values");
		SerializedProperty isKeyDuplicated = property.FindPropertyRelative("m_IsKeyDuplicated");

		// compute the Rect for the label
		var labelRect = new Rect(position.x, position.y, position.width, 16f);
		property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label);

		// init the total height counter with the height of the label
		float newTotalHeight = labelRect.height;
		position.y += labelRect.height;

		if (property.isExpanded)
		{
			// draw the size of the dictionary next to the label
			var sizeRect = new Rect(position.x, position.y, position.width, 16f); 
			int nbKeyValue = Mathf.Max(keys.arraySize, values.arraySize);
			int newNbKeyValue = EditorGUI.DelayedIntField(sizeRect, "Size", nbKeyValue);
			if (nbKeyValue != newNbKeyValue)
				nbKeyValue = newNbKeyValue;

			EditorGUI.EndProperty();

			// resize the arrays if needed
			if (keys.arraySize != nbKeyValue)
				keys.arraySize = nbKeyValue;			
			if (values.arraySize != nbKeyValue)
				values.arraySize = nbKeyValue;			
			if (isKeyDuplicated.arraySize != nbKeyValue)
				isKeyDuplicated.arraySize = nbKeyValue;

			// increase the total height and the line position for the next line
			newTotalHeight += sizeRect.height;
			position.y += sizeRect.height;

			// affiche le warning message
			var messageRect = new Rect(position.x, position.y, position.width, 16f);
			EditorGUI.LabelField(messageRect, "<i>Duplicated keys in red will be ignored.</i>", warningStyle);
			newTotalHeight += messageRect.height;
			position.y += messageRect.height;

			// compute the width for key and value (taking 40% for key and 60% for value)
			float marginWidth = 12f;
			float keyWidth = position.width * 0.4f;
			float valueWidth = position.width - keyWidth - marginWidth;

			for (int i = 0; i < nbKeyValue; ++i)
			{
				// get the current key and value
				SerializedProperty key = keys.GetArrayElementAtIndex(i);
				SerializedProperty value = values.GetArrayElementAtIndex(i);
				SerializedProperty isDuplicated = isKeyDuplicated.GetArrayElementAtIndex(i);

				// compute the needed height for key and value
				float keyHeight = (key != null) ? EditorGUI.GetPropertyHeight(key) : 0;
				float valueHeight = (value != null) ? EditorGUI.GetPropertyHeight(value) : 0;
				float lineHeight = Mathf.Max(keyHeight, valueHeight);

				// if we overpass the size of the dictionnary, we add a mark to show the ignored duplicated keys at the end
				if (isDuplicated.boolValue)
				{
					var warningRect = new Rect(position.x, position.y, position.width, lineHeight);
					EditorGUI.DrawRect(warningRect, Color.red);
				}

				// Calculate the rects for key and value
				var keyRect = new Rect(position.x, position.y, keyWidth, lineHeight);
				var valueRect = new Rect(position.x + keyWidth + marginWidth, position.y, valueWidth, lineHeight);		

				// Draw fields - passs GUIContent.none to each so they are drawn without labels
				EditorGUI.indentLevel = indentLevel;
				EditorGUI.PropertyField(keyRect, key, GUIContent.none, true);
				EditorGUI.indentLevel = 0;
				EditorGUI.PropertyField(valueRect, value, GUIContent.none, true);

				// increase the total height and the line position for the next line
				newTotalHeight += lineHeight;
				position.y += lineHeight;
			}

			// reset the indent level
			EditorGUI.indentLevel = indentLevel;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float newTotalHeight = 16f;
		if (property.isExpanded)
		{
			newTotalHeight += 32f;

			SerializedProperty keys = property.FindPropertyRelative("m_Keys");
			SerializedProperty values = property.FindPropertyRelative("m_Values");
			int nbKeyValue = Mathf.Max(keys.arraySize, values.arraySize);

			for (int i = 0; i < nbKeyValue; ++i)
			{
				// get the current key and value
				SerializedProperty key = keys.GetArrayElementAtIndex(i);
				SerializedProperty value = values.GetArrayElementAtIndex(i);

				// compute the needed height for key and value
				float keyHeight = (key != null) ? EditorGUI.GetPropertyHeight(key) : 0;
				float valueHeight = (value != null) ? EditorGUI.GetPropertyHeight(value) : 0;
				float lineHeight = Mathf.Max(keyHeight, valueHeight);

				// increase the total height and the line position for the next line
				newTotalHeight += lineHeight;
			}
		}
		return newTotalHeight;
	}
}
