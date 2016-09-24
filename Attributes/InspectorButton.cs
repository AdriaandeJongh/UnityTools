using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Linq;
using System.Reflection;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class InspectorButton : PropertyAttribute
{
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof (MonoBehaviour), true)]
public class InspectorButtonDrawer : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var mono = target as MonoBehaviour;

		var methods = mono.GetType()
			.GetMembers(BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
				BindingFlags.NonPublic)
			.Where(o => System.Attribute.IsDefined(o, typeof (InspectorButton)));

		foreach (var memberInfo in methods)
		{
			if (GUILayout.Button(memberInfo.Name))
			{
				var method = memberInfo as MethodInfo;
				method.Invoke(mono, null);
			}
		}
	}
}
#endif