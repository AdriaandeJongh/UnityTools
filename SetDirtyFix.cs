using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class SetDirtyFix 
{
	public static void Fix(Object o)
	{
		#if UNITY_EDITOR
		EditorUtility.SetDirty(o);
		#endif
	}
 
	public static void Fix(GameObject go) 
	{
		#if UNITY_EDITOR
		EditorUtility.SetDirty(go);
		EditorSceneManager.MarkSceneDirty(go.scene); //This used to happen automatically from SetDirty
		#endif
	}
 
	public static void Fix(Component comp)
	{
		#if UNITY_EDITOR
		EditorUtility.SetDirty(comp);
		EditorSceneManager.MarkSceneDirty(comp.gameObject.scene); //This used to happen automatically from SetDirty
		#endif
	}
}