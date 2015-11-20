// Put this script inside the Editor folder.
// By Adriaan de Jongh, http://adriaandejongh.nl
// More info & other Unity scripts: https://github.com/AdriaandeJongh/UnityTools

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(HideWhenFalseAttribute))]
public class HideWhenFalseDrawer : PropertyDrawer 
{

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label) 
	{
		HideWhenFalseAttribute hiddenAttribute = attribute as HideWhenFalseAttribute;
		SerializedProperty boolProperty = property.serializedObject.FindProperty(hiddenAttribute.hideBoolean);

		if(boolProperty.boolValue)
			EditorGUI.PropertyField(position, property, label, true);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		HideWhenFalseAttribute hiddenAttribute = attribute as HideWhenFalseAttribute;
		SerializedProperty boolProperty = property.serializedObject.FindProperty(hiddenAttribute.hideBoolean);
		
		if(!boolProperty.boolValue)
			return 0f;

		return EditorGUI.GetPropertyHeight(property);
	}
}