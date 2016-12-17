using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ShowOnEnum : PropertyAttribute 
{
	public readonly string enumName;
	public readonly int wantedValue; 
	
	public ShowOnEnum(string enumName, int wantedValue)
	{
		this.enumName = enumName;
		this.wantedValue = wantedValue;
	}
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(ShowOnEnum))]
public class ShowOnEnumDrawer : PropertyDrawer 
{
	
	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label) 
	{
		ShowOnEnum att = attribute as ShowOnEnum;
		SerializedProperty enumProperty = property.serializedObject.FindProperty(att.enumName);
		
		if(enumProperty.enumValueIndex == att.wantedValue)
			EditorGUI.PropertyField(position, property, label, true);
	}
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		ShowOnEnum att = attribute as ShowOnEnum;
		SerializedProperty enumProperty = property.serializedObject.FindProperty(att.enumName);
		
		if(enumProperty.enumValueIndex != att.wantedValue)
			return -2f;
		
		return EditorGUI.GetPropertyHeight(property);
	}
}
#endif