using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class MinMax : PropertyAttribute 
{
	public readonly float max;
	public readonly float min;
	
	public MinMax (float min, float max) 
	{
		this.min = min;
		this.max = max;
	}
}
#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(MinMax))]
public class MinMaxDrawer : PropertyDrawer 
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) 
	{
		
		if (property.propertyType != SerializedPropertyType.Vector2)
		{
			EditorGUI.LabelField (position, label, "Vector2 only!");
			return;
		}
		
		Vector2 range = property.vector2Value;
		float min = range.x;
		float max = range.y;
		MinMax attr = attribute as MinMax;
		
		label.text += ": " + min.ToString("0.00") + " to " + max.ToString("0.00");
		
		EditorGUI.BeginChangeCheck();
		
		EditorGUI.MinMaxSlider (position, label, ref min, ref max, attr.min, attr.max);
		
		if(EditorGUI.EndChangeCheck()) 
		{
			range.x = min;
			range.y = max;
			property.vector2Value = range;
		}
		
	}
}
#endif