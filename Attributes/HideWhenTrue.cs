using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class HideWhenTrue : PropertyAttribute 
{
	public readonly string hideBoolean;

	public HideWhenTrue (string booleanName)
	{
		this.hideBoolean = booleanName;
	}
}
#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(HideWhenTrue))]
public class HideWhenTrueDrawer : PropertyDrawer 
{

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label) 
	{
		HideWhenTrue hiddenAttribute = attribute as HideWhenTrue;
		SerializedProperty boolProperty = property.serializedObject.FindProperty(hiddenAttribute.hideBoolean);

		if(!boolProperty.boolValue)
			EditorGUI.PropertyField(position, property, label, true);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		HideWhenTrue hiddenAttribute = attribute as HideWhenTrue;
		SerializedProperty boolProperty = property.serializedObject.FindProperty(hiddenAttribute.hideBoolean);

		if(boolProperty.boolValue)
			return 0f;

		return EditorGUI.GetPropertyHeight(property);
	}
}
#endif