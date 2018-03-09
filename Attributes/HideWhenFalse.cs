using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class HideWhenFalse : PropertyAttribute 
{
	public readonly string hideBoolean;

	public HideWhenFalse (string booleanName)
	{
		this.hideBoolean = booleanName;
	}
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(HideWhenFalse))]
public class HideWhenFalseDrawer : PropertyDrawer 
{

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label) 
	{
		HideWhenFalse hiddenAttribute = attribute as HideWhenFalse;
		SerializedProperty boolProperty = property.serializedObject.FindProperty(hiddenAttribute.hideBoolean);

		if(boolProperty.boolValue)
			EditorGUI.PropertyField(position, property, label, true);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		HideWhenFalse hiddenAttribute = attribute as HideWhenFalse;
		SerializedProperty boolProperty = property.serializedObject.FindProperty(hiddenAttribute.hideBoolean);

		if(!boolProperty.boolValue)
			return -2f;

		return EditorGUI.GetPropertyHeight(property);
	}
}
#endif