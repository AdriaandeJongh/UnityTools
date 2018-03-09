using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ReadOnly : PropertyAttribute { }

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(ReadOnly))]
public class ReadOnlyDrawer : PropertyDrawer 
{
	
	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label) 
	{
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label);
		GUI.enabled = true;
	}
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight( property, label, true );
	}
}
#endif