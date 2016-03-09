// By Adriaan de Jongh, http://adriaandejongh.nl
// More info & other Unity scripts: https://github.com/AdriaandeJongh/UnityTools

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Inp : MonoBehaviour 
{
	
	//some settings you may want to change:
	private const float timeToRegisterAsQuickTap = 0.4f;
	private const float distanceToRegisterAsQuickTap = 20f;
	private const int mouseHistorySize = 30; //in frames, so divide by fps for # of seconds
	private const int touchHistorySize = 30; //in frames, so divide by fps for # of seconds

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
	private static Inp instance;
	
	private Vector2[] mouseHistory; 
	private int mouseHistoryIndex = 0;
	
	private Dictionary<int, Vector2[]> touchHistory;
	private Dictionary<int, int> touchHistoryIndex;
	private Dictionary<int, int> touchHistoryCount;
	private Dictionary<int, bool> touchOverUIHistory;

	private Dictionary<int, PositionAtTime> inputStart;
	struct PositionAtTime
	{
		public Vector2 position;
		public float time;

		public PositionAtTime(Vector2 position, float time)
		{
			this.position = position;
			this.time = time;
		}
	}

	void Awake()
	{
		mouseHistory = new Vector2[mouseHistorySize];
		touchHistory = new Dictionary<int, Vector2[]>();
		touchHistoryIndex = new Dictionary<int, int>();
		touchHistoryCount = new Dictionary<int, int>();
		touchOverUIHistory = new Dictionary<int, bool>();
		inputStart = new Dictionary<int, PositionAtTime>();
	}

	void Update()
	{
		if(Application.isEditor || (!Application.isMobilePlatform && !Application.isConsolePlatform))
			UpdateMouse();
		else if(Application.isMobilePlatform)
			UpdateTouch();
	}
	
	void LateUpdate()
	{
		if(Application.isEditor || (!Application.isMobilePlatform && !Application.isConsolePlatform))
			RemoveEndedMouse();
		else if(Application.isMobilePlatform)
			RemoveEndedTouch();
	}
	
	/// <summary> Updates mouse history based on Unity's Input class. </summary>
	void UpdateMouse()
	{
		if(Input.GetMouseButtonDown(0))
		{
			if(!inputStart.ContainsKey(0))
			{
				inputStart.Add(0, new PositionAtTime(Input.mousePosition, Time.time));
			}
		}

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
				touchHistoryCount[touch.fingerId] = Mathf.Clamp(touchHistoryCount[touch.fingerId] + 1, 0, touchHistorySize);

				if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
					touchOverUIHistory[touch.fingerId] = true;
				else
					touchOverUIHistory[touch.fingerId] = false;
			}
		}
	}

	/// <summary> Removes registered mouse click things! </summary>
	void RemoveEndedMouse()
	{
		if(Input.GetMouseButtonUp(0))
		{
			inputStart.Remove(0);
		}
	}

	/// <summary> Removes the registered touches! </summary>
	void RemoveEndedTouch()
	{
		foreach(Touch touch in Input.touches)
		{
			if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				touchHistoryIndex.Remove(touch.fingerId);
				touchHistory.Remove(touch.fingerId);
				touchHistoryCount.Remove(touch.fingerId);
				touchOverUIHistory.Remove(touch.fingerId);
				inputStart.Remove(touch.fingerId);
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
	public Vector2 MouseDeltaPositionSample(int sampleLength)
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
			touchHistoryCount.Add(touch.fingerId, 0);
			touchOverUIHistory.Add(touch.fingerId, false);
			inputStart.Add(touch.fingerId, new PositionAtTime());
		}
		
		touchHistoryIndex[touch.fingerId] = 0;
		touchHistory[touch.fingerId][touchHistoryIndex[touch.fingerId]] = touch.position;
		touchHistoryCount[touch.fingerId] = 1;

		if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
			touchOverUIHistory[touch.fingerId] = true;
		else
			touchOverUIHistory[touch.fingerId] = false;

		inputStart[touch.fingerId] = new PositionAtTime(touch.position, Time.time);
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
					if(touch.phase == TouchPhase.Began || 
					   touch.phase == TouchPhase.Moved || 
					   touch.phase == TouchPhase.Stationary)
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
	
	///<returns>Returns a previous position of a touch using Unity's unique fingerId.</returns>
	public Vector2 TouchPreviousPosition(int fingerId, int howManyBack = 1)
	{
		if(!touchHistory.ContainsKey(fingerId))
		{
			foreach(Touch touch in Input.touches)
			{
				if(touch.fingerId == fingerId)
				{
					if(touch.phase == TouchPhase.Began || 
					   touch.phase == TouchPhase.Moved || 
					   touch.phase == TouchPhase.Stationary)
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
		
		if(howManyBack >= touchHistorySize)
		{
			Debug.LogError ("You can't go back further than or as far away as the touch history!");
			howManyBack = touchHistorySize - 1;
		}
		
		if(howManyBack > touchHistoryCount[fingerId] - 1)
		{
//			Debug.LogWarning("Requested previous touch position doesn't exist yet, " +
//				"so the earliest touch available was returned.");
			howManyBack = touchHistoryCount[fingerId] - 1;
		}
		
		int prevIndex = touchHistoryIndex[fingerId] - howManyBack;
		if(prevIndex < 0) prevIndex += touchHistorySize;
		
		return touchHistory[fingerId][prevIndex];
	}
	
	///<returns>Returns the delta position of a touch using Unity's unique fingerId.</returns>
	public Vector2 TouchDeltaPosition(int fingerId)
	{
		return TouchPosition(fingerId) - TouchPreviousPosition(fingerId);
	}
	
	///<returns>Returns the average delta position over X samples of a touch, using Unity's unique fingerId. NOTE: CURRENTLY FRAME RATE DEPENDENT!!!!</returns>
	/// 
	/// NOTE: CURRENTLY FRAME RATE DEPENDENT!!!!!! 
	/// 
	public Vector2 TouchDeltaPositionSample(int fingerId, int sampleLength)
	{
		
		if(!touchHistory.ContainsKey(fingerId))
		{
			foreach(Touch touch in Input.touches)
			{
				if(touch.fingerId == fingerId)
				{
					if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
					{
						AddTouch(touch);
					}
					
					return Vector2.zero;
				}
			}
		}
		
		int samples = Mathf.Min(sampleLength, touchHistoryCount[fingerId] - 1); //history-1 because we'll need to go back in time at least 1 frame
		
		if(samples <= 1)
		{
			return Vector2.zero;
		}
		
		Vector2 avgDeltaPos = Vector2.zero;
		int thisIndex = touchHistoryIndex[fingerId];
		int prevIndex = thisIndex - 1;
		if(prevIndex < 0) prevIndex += touchHistorySize;
		
		for(int i = 0; i < samples; i++)
		{
			thisIndex = touchHistoryIndex[fingerId] - i;
			if(thisIndex < 0) thisIndex += touchHistorySize;
			
			prevIndex = (thisIndex - 1);
			if(prevIndex < 0) prevIndex += touchHistorySize;
			
			avgDeltaPos += touchHistory[fingerId][thisIndex] - touchHistory[fingerId][prevIndex];
		}
		
		avgDeltaPos /= samples;
		
		return avgDeltaPos;
	}
	
	///<returns>Returns screenPosition translated to worldspace.</returns>
	public Vector3 To2DWorld(Vector2 screenPosition, float z = 0f)
	{
		if(!Camera.main.orthographic || Camera.main.transform.eulerAngles.x != 0 || Camera.main.transform.eulerAngles.y != 0)
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
			if(Application.isEditor || (!Application.isMobilePlatform && !Application.isConsolePlatform))
			{
				return Input.GetMouseButtonDown(0);
			}
			else if(Application.isMobilePlatform)
			{
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
			}
			else
			{
				Debug.LogError("Inp.ut doesn't support this platform!");
				return false;
			}
		}
	}
	
	///<returns>Returns true if the release of a click or (first) touch is new this frame.</returns>
	public bool firstInputUp
	{
		get 
		{
			if(Application.isEditor || (!Application.isMobilePlatform && !Application.isConsolePlatform))
			{
				return Input.GetMouseButtonUp(0);
			}
			else if(Application.isMobilePlatform)
			{
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
			}
			else
			{
				Debug.LogError("Inp.ut doesn't support this platform!");
				return false;
			}
		}
	}

	///<returns>Returns true if the release of a click or (first) touch is was quick enough or within a distance of the start time and position of the input.</returns>
	public bool firstInputQuickTap
	{
		get 
		{
			if(!firstInputUp)
				return false;
			
			if(Application.isEditor || (!Application.isMobilePlatform && !Application.isConsolePlatform))
			{
				if(Time.time - inputStart[0].time < timeToRegisterAsQuickTap && 
					Vector2.Distance(inputStart[0].position, Inp.ut.firstInputPosition) < distanceToRegisterAsQuickTap)
				{
					return true;
				}

				return false;
			}
			else if(Application.isMobilePlatform)
			{
				if(Input.touchCount > 0)
				{
					Touch touch = Input.GetTouch(0);

					if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
					{
						if(Time.time - inputStart[touch.fingerId].time < timeToRegisterAsQuickTap && 
							Vector2.Distance(inputStart[touch.fingerId].position, Inp.ut.firstInputPosition) < distanceToRegisterAsQuickTap)
						{
							return true;
						}
					}
				}

				return false;
			}
			else
			{
				Debug.LogError("Inp.ut doesn't support this platform!");
				return false;
			}
		}
	}
	
	///<returns>Returns screenPosition of either the mouse or the first touch.</returns>
	public Vector2 firstInputPosition
	{
		get 
		{
			if(Application.isEditor || (!Application.isMobilePlatform && !Application.isConsolePlatform))
			{
				return mousePosition;
			}
			else if(Application.isMobilePlatform)
			{
				if(Input.touchCount > 0)
					return TouchPosition(Input.GetTouch(0).fingerId);
				else
					return Vector2.zero;
			}
			else
			{
				Debug.LogError("Inp.ut doesn't support this platform!");
				return Vector2.zero;
			}
		}
	}
	
	///<returns>Returns screenPosition of either the mouse or the first touch.</returns>
	public Vector2 firstInputDeltaPosition
	{
		get 
		{
			if(Application.isEditor || (!Application.isMobilePlatform && !Application.isConsolePlatform))
			{
				return mouseDeltaPosition;
			}
			else if(Application.isMobilePlatform)
			{
				if(Input.touchCount > 0)
					return TouchDeltaPosition(Input.GetTouch(0).fingerId);
				else
					return Vector2.zero;
			}
			else
			{
				Debug.LogError("Inp.ut doesn't support this platform...");
				return Vector2.zero;
			}
		}
	}
	
	///<returns>Returns an array of screen position(s) of the mouse or all touches.</returns>
	public Vector2[] allInputPositions
	{
		get 
		{
			if(Application.isEditor || (!Application.isMobilePlatform && !Application.isConsolePlatform))
			{
				if(Input.GetMouseButton(0))
				return new Vector2[] { mousePosition };
				else
					return new Vector2[0];
			}
			else if(Application.isMobilePlatform)
			{
				Vector2[] touchPositions = new Vector2[Input.touchCount];
				
				for(int i = 0; i < Input.touchCount; i++)
				{
					touchPositions[i] = TouchPosition(Input.GetTouch(i).fingerId);
				}
				
				return touchPositions;
			}
			else
			{
				Debug.LogError("Inp.ut doesn't support this platform!");
				return new Vector2[0];
			}
		}
	}
	
	///<returns>Returns whether the mouse or touches are touching or hovering over Unity 5's UI.</returns>
	public bool isOverUI
	{
		get
		{
			if(EventSystem.current == null)
				return false;

			if(Input.touchCount > 0) 
			{
				foreach(Touch touch in Input.touches) 
				{
					if(touchOverUIHistory[touch.fingerId]) 
					{
						return true;
					}
				}
			}
			else
			{
				return EventSystem.current.IsPointerOverGameObject();
			}
			
			return false;
		}
	}
	
	///<returns>Returns whether a specific touch is touching or hovering over Unity 5's UI.</returns>
	public bool IsOverUI(int fingerId)
	{
		if(EventSystem.current == null)
			return false;
		
		if(Input.touchCount > 0) 
		{
			if(touchOverUIHistory[fingerId]) 
			{
				return true;
			}
		}
		else
		{
			return EventSystem.current.IsPointerOverGameObject(fingerId);
		}
		
		return false;
	}
	
}