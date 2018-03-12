using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class CanvasHelper : MonoBehaviour
{
	private static bool screenChangeVarsInitialized = false;
	private static ScreenOrientation lastOrientation = ScreenOrientation.Unknown;
	private static Vector2 lastResolution = Vector2.zero;
	
	public static UnityEvent onOrientationChange = new UnityEvent();
	public static UnityEvent onResolutionChange = new UnityEvent();
	
	private RectTransform safeAreaTransform;
	
	void Awake()
	{
		safeAreaTransform = transform.Find("SafeArea") as RectTransform;
		ApplySafeArea();
	}
	
	void OnRectTransformDimensionsChange()
	{
		ApplySafeArea();
		
		if(!screenChangeVarsInitialized)
		{
			lastOrientation = Screen.orientation;
			lastResolution.x = Screen.width;
			lastResolution.y = Screen.height;
			screenChangeVarsInitialized = true;
		}
		
		if(Screen.orientation != lastOrientation)
		{
			//will only ever happen on mobile
			
			onOrientationChange.Invoke();
			
			lastOrientation = Screen.orientation;
			lastResolution.x = Screen.width;
			lastResolution.y = Screen.height;
			
			return;
		}
		
		if(Screen.width != lastResolution.x || Screen.height != lastResolution.y)
		{
			onResolutionChange.Invoke();
			
			lastResolution.x = Screen.width;
			lastResolution.y = Screen.height;
		}
	}
	
	void ApplySafeArea()
	{
		if(safeAreaTransform == null)
			return;
		
		var anchorMin = Screen.safeArea.position;
		var anchorMax = Screen.safeArea.position + Screen.safeArea.size;
		anchorMin.x /= Screen.width;
		anchorMin.y /= Screen.height;
		anchorMax.x /= Screen.width;
		anchorMax.y /= Screen.height;
		
		safeAreaTransform.anchorMin = anchorMin;
		safeAreaTransform.anchorMax = anchorMax;
	}
}
