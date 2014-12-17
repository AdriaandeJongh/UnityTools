/* This thing was written by Adriaan de Jongh, http://adriaandejongh.nl */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inp : MonoBehaviour 
{
	private static Inp instance;

	///Inp.ut   - get it? hrhr.
	public static Inp ut
	{
		get
		{
			if(instance == null)
			{
				GameObject inpGO = new GameObject("Inp.ut", typeof(Inp));
				instance = inpGO.GetComponent<Inp>();
				DontDestroyOnLoad(inpGO);
			}
			
			return instance;
		}
	}

	private Vector2[] mouseHistory; 
	private int mouseHistoryIndex = 0;
	private const int mouseHistorySize = 50; //in frames, so divide by fps for # of seconds

	private Dictionary<int, Vector2[]> touchHistory = new Dictionary<int, Vector2[]>();
	private Dictionary<int, int> touchHistoryIndex = new Dictionary<int, int>();
	private const int touchHistorySize = 50; //in frames, so divide by fps for # of seconds

	void Awake()
	{
		mouseHistory = new Vector2[mouseHistorySize];
	}
	
	void Start()
	{

	}

	void Update()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
		UpdateMouse();
		#elif UNITY_IPHONE || UNITY_WP8 || UNITY_ANDROID
		UpdateTouch();
		#endif
	}

	/// <summary> Updates mouse history based on Unity's Input class. </summary>
	void UpdateMouse()
	{
		mouseHistoryIndex = (mouseHistoryIndex + 1) % mouseHistorySize;
		mouseHistory[mouseHistoryIndex] = Input.mousePosition;
	}

	/// <summary> Updates touches history based on Unity's Input class. </summary>
	void UpdateTouch()
	{
		foreach(Touch touch in Input.touches)
		{
			if(touch.phase == TouchPhase.Began)
			{
				AddTouch(touch);
			}
			else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
			{
				touchHistoryIndex[touch.fingerId] = (touchHistoryIndex[touch.fingerId] + 1) % touchHistorySize;
				touchHistory[touch.fingerId][touchHistoryIndex[touch.fingerId]] = touch.position;
			}
			else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				touchHistoryIndex.Remove(touch.fingerId);
				touchHistory.Remove(touch.fingerId);
			}
		}
	}

	///<returns>Returns the current mouse position.</returns>
	public Vector2 mousePosition
	{
		get { return mouseHistory[mouseHistoryIndex]; }
	}
	
	///<returns>Returns the previous mouse position.</returns>
	public Vector2 mousePreviousPosition
	{
		get
		{
			int prevIndex = mouseHistoryIndex - 1;
			if(prevIndex < 0) prevIndex += mouseHistorySize;
			return mouseHistory[prevIndex];
		}
	}
	
	///<returns>Returns the previous mouse position.</returns>
	public Vector2 mouseDeltaPosition
	{
		get { return mouseHistory[mouseHistoryIndex] - mousePreviousPosition; }
	}
	
	///<returns>Returns the average delta position over an amount of samples.</returns>
	public Vector2 MouseDeltaSample(int sampleLength)
	{
		Vector2 avgDeltaPos = Vector2.zero;
		int thisIndex = mouseHistoryIndex;
		int prevIndex = thisIndex - 1;
		if(prevIndex < 0) prevIndex += mouseHistorySize;
		
		for(int i = 0; i < sampleLength; i++)
		{
			thisIndex = mouseHistoryIndex - i;
			if(thisIndex < 0) thisIndex += mouseHistorySize;
			
			prevIndex = (thisIndex - 1);
			if(prevIndex < 0) prevIndex += mouseHistorySize;
			
			avgDeltaPos += mouseHistory[thisIndex] - mouseHistory[prevIndex];
		}
		
		avgDeltaPos /= sampleLength;
		
		return avgDeltaPos;
	}

	/// <summary> Adds a touch & touch index to history. </summary>
	private void AddTouch(Touch touch)
	{
		if(!touchHistory.ContainsKey(touch.fingerId))
		{
			touchHistoryIndex.Add(touch.fingerId, 0);
			touchHistory.Add(touch.fingerId, new Vector2[touchHistorySize]);
		}

		touchHistoryIndex[touch.fingerId] = 1;
		touchHistory[touch.fingerId][touchHistoryIndex[touch.fingerId]] = touch.position;
		
		//also set previous
		touchHistory[touch.fingerId][touchHistoryIndex[touch.fingerId] - 1] = touch.position - touch.deltaPosition;
	}

	///<returns>Returns the current position of a touch using Unity's unique fingerId.</returns>
	public Vector2 TouchPosition(int fingerId)
	{
		if(!touchHistory.ContainsKey(fingerId))
		{
			foreach(Touch touch in Input.touches)
			{
				if(touch.fingerId == fingerId)
				{
					if(touch.phase == TouchPhase.Began)
					{
						AddTouch(touch);
					}
					else
					{
						return touch.position;
					}
				}
			}
		}

		return touchHistory[fingerId][touchHistoryIndex[fingerId]];
	}

	///<returns>Returns the previous position of a touch using Unity's unique fingerId.</returns>
	public Vector2 TouchPreviousPosition(int fingerId)
	{
		if(!touchHistory.ContainsKey(fingerId))
		{
			foreach(Touch touch in Input.touches)
			{
				if(touch.fingerId == fingerId)
				{
					if(touch.phase == TouchPhase.Began)
					{
						AddTouch(touch);
					}
					else
					{
						return touch.position - touch.deltaPosition;
					}
				}
			}
		}

		int prevIndex = touchHistoryIndex[fingerId] - 1;
		if(prevIndex < 0) prevIndex += touchHistorySize;

		return touchHistory[fingerId][prevIndex];
	}

	///<returns>Returns the delta position of a touch using Unity's unique fingerId.</returns>
	public Vector2 TouchDeltaPosition(int fingerId)
	{
		return TouchPosition(fingerId) - TouchPreviousPosition(fingerId);
	}

	///<returns>Returns screenPosition translated to worldspace.</returns>
	public Vector3 To2DWorld(Vector2 screenPosition, float z = 0f)
	{
		if(!Camera.main.isOrthoGraphic || Camera.main.transform.eulerAngles.x != 0 || Camera.main.transform.eulerAngles.y != 0)
		{
			Debug.LogError("To use To2DWorld(), the camera needs to be orthographic & non-rotated on x- and y-axis!");
		}
		
		Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
		worldPosition.z = z;
		return worldPosition;
	}

	///<returns>Returns true if any input is given.</returns>
	public bool anyInput
	{
		get { return Input.GetMouseButton(0) || Input.GetMouseButtonUp(0) || Input.touchCount > 0; }
	}

	///<returns>Returns true if the click or (first) touch is new this frame.</returns>
	public bool firstInputDown
	{
		get 
		{ 
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
			return Input.GetMouseButtonDown(0);
			#elif UNITY_IPHONE || UNITY_WP8 || UNITY_ANDROID
			if(Input.touchCount > 0)
			{
				if(Input.GetTouch(0).phase == TouchPhase.Began)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
			#endif
		}
	}

	///<returns>Returns true if the click or (first) touch is new this frame.</returns>
	public bool firstInputUp
	{
		get 
		{ 
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
			return Input.GetMouseButtonUp(0);
			#elif UNITY_IPHONE || UNITY_WP8 || UNITY_ANDROID
			if(Input.touchCount > 0)
			{
				if(Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
			#endif
		}
	}

	///<returns>Returns screenPosition of either the mouse or the first touch.</returns>
	public Vector2 firstInputPosition
	{
		get 
		{
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
			return mousePosition;
			#elif UNITY_IPHONE || UNITY_WP8 || UNITY_ANDROID || UNITY_METRO
			if(Input.touchCount > 0)
				return TouchPosition(Input.GetTouch(0).fingerId);
			else
				return Vector2.zero;
			#endif
		}
	}

	///<returns>Returns an array of screen position(s) of the mouse or all touches.</returns>
	public Vector2[] allInputPositions
	{
		get 
		{
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
			if(Input.GetMouseButton(0))
				return new Vector2[] { mousePosition };
			else
				return new Vector2[0];
			#elif UNITY_IPHONE || UNITY_WP8 || UNITY_ANDROID
			Vector2[] touchPositions = new Vector2[Input.touchCount];
			
			for(int i = 0; i < Input.touchCount; i++)
			{
				touchPositions[i] = TouchPosition(Input.GetTouch(i).fingerId);
			}
			
			return touchPositions;
			#endif
		}
	}
}






