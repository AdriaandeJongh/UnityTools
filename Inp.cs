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
	private const int historySize = 30; //in frames, so divide by fps for # of seconds

	///Inp.ut   - get it? hrhr.
	public static Inp ut
	{
		get
		{
			if(instance == null)
			{
				new GameObject("Inp.ut", typeof(Inp));
				
				//compensates for having missed the update of the frame Inp.ut was called but didn't exist yet
				instance.Update(); 
			}
			
			return instance;
		}
	}
	private static Inp instance;
	
	private Dictionary<int, Vector2[]> inputHistory;
	private Dictionary<int, int> inputHistoryIndex;
	private Dictionary<int, int> inputHistoryCount;
	private Dictionary<int, bool> inputOverUIHistory;
	private Dictionary<int, PositionAtTime> inputStart;
	private List<int> inputToBeRemoved;
	
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
	
	private bool _touchEnabled = true;
	public bool touchEnabled
	{
		get { return Input.touchSupported && _touchEnabled; }
		set { _touchEnabled = value; }
	}
	
	private bool _mouseEnabled = true;
	public bool mouseEnabled
	{
		get { return Input.mousePresent && _mouseEnabled; }
		set { _mouseEnabled = value; }
	}
	
	public const int mouseId = -1;

	void Awake()
	{
		//make this singleton persistent & don't allow any other gameobject to have this component
		if(instance != null && instance != this) { Destroy(this); return; } 
		instance = this; DontDestroyOnLoad(this.gameObject);
		
		inputHistory = new Dictionary<int, Vector2[]>();
		inputHistoryIndex = new Dictionary<int, int>();
		inputHistoryCount = new Dictionary<int, int>();
		inputOverUIHistory = new Dictionary<int, bool>();
		inputStart = new Dictionary<int, PositionAtTime>();
		inputToBeRemoved = new List<int>();

		#if UNITY_TVOS
		if(Application.platform == RuntimePlatform.tvOS)
		{
			UnityEngine.Apple.TV.Remote.touchesEnabled = true;
		}
		#endif
		
		if(!Input.touchSupported && !Input.mousePresent)
		{
			Debug.LogError("This platform is currently not supported by Inp.ut.");
		}
	}

	void Update()
	{
		if(touchEnabled)
		{
			RemoveEndedTouch();
			UpdateTouch();
		}
		
		if(mouseEnabled)
		{
			RemoveEndedMouse();
			UpdateMouse();
		}
	}
	
	/// <summary> Updates mouse history based on Unity's Input class. </summary>
	void UpdateMouse()
	{
		if(!inputHistory.ContainsKey(mouseId))
			AddInput(mouseId, Input.mousePosition);
		
		if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			inputHistoryCount[mouseId] = 1; //so that when we do any sampling, 
			// it doesn't try to go beyond where the touch started. 
			// this is especially important for touchscreens that set the mouse position, 
			// as the mouse is set to a position instantly...
			// also: it's set to 0 because it's +1'ed below
			
			if(!inputStart.ContainsKey(mouseId))
				inputStart.Add(mouseId, new PositionAtTime(Input.mousePosition, Time.time));
			else //this may be the case when we're on a touch screen on a computer...
				inputStart[mouseId] = new PositionAtTime(Input.mousePosition, Time.time);
		}
		else if((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && inputStart.ContainsKey(mouseId)) //in case we're on a touchscreen with a mouse
			inputToBeRemoved.Add(mouseId);

		inputHistoryIndex[mouseId] = (inputHistoryIndex[mouseId] + 1) % historySize;
		inputHistory[mouseId][inputHistoryIndex[mouseId]] = Input.mousePosition;
		inputHistoryCount[mouseId] = Mathf.Clamp(inputHistoryCount[mouseId] + 1, 0, historySize);
	}
	
	/// <summary> Updates touches history based on Unity's Input class. </summary>
	void UpdateTouch()
	{
		foreach(Touch touch in Input.touches)
		{
			if(touch.phase == TouchPhase.Began)
			{
				AddInput(touch.fingerId, touch.position);
				inputStart.Add(touch.fingerId, new PositionAtTime(touch.position, Time.time));
			}
			else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
			{
				inputHistoryIndex[touch.fingerId] = (inputHistoryIndex[touch.fingerId] + 1) % historySize;
				inputHistory[touch.fingerId][inputHistoryIndex[touch.fingerId]] = touch.position;
				inputHistoryCount[touch.fingerId] = Mathf.Clamp(inputHistoryCount[touch.fingerId] + 1, 0, historySize);
				
				//this is a fix to unity's system
				if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
					inputOverUIHistory[touch.fingerId] = true;
				else
					inputOverUIHistory[touch.fingerId] = false;
			}
			else
			{
				inputToBeRemoved.Add(touch.fingerId);
			}
		}
	}

	/// <summary> Removes registered mouse click things! </summary>
	void RemoveEndedMouse()
	{
		if(inputToBeRemoved.Count == 0)
			return;
		
		foreach (var index in inputToBeRemoved)
		{
			inputStart.Remove(index);
			
		}
		
		inputToBeRemoved.Clear();
		
	}

	/// <summary> Removes the registered touches! </summary>
	void RemoveEndedTouch()
	{
		if(inputToBeRemoved.Count == 0)
			return;
		
		foreach(var index in inputToBeRemoved)
		{
			inputHistoryIndex.Remove(index);
			inputHistory.Remove(index);
			inputHistoryCount.Remove(index);
			inputOverUIHistory.Remove(index);
			inputStart.Remove(index);
		}
		
		inputToBeRemoved.Clear();
	}
	
	void AddInput(int fingerId, Vector2 position)
	{
		if(inputHistory.ContainsKey(fingerId))
			return;
		
		inputHistoryIndex.Add(fingerId, 0);
		inputHistory.Add(fingerId, new Vector2[historySize]);
		inputHistory[fingerId][inputHistoryIndex[fingerId]] = position;
		inputHistoryCount.Add(fingerId, 1);
		inputOverUIHistory.Add(fingerId, EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId));
	}
	
	///<returns>Returns the current position of a touch using Unity's unique fingerId.</returns>
	public Vector2 Position(int fingerId)
	{
		if(!inputHistory.ContainsKey(fingerId))
		{
			if(mouseEnabled && fingerId == mouseId)
				return Input.mousePosition;
			
			Debug.LogError("An Inp.ut position was checked before Inp.ut's first update. " +
				"Don't call for position functions in other script's Start() and Awake() functions " +
				"as it doesn't make any sense for touches.");
		}
		
		return inputHistory[fingerId][inputHistoryIndex[fingerId]];
	}
	
	///<returns>Returns a previous position of a touch using Unity's unique fingerId.</returns>
	public Vector2 PreviousPosition(int fingerId, int howManyBack = 1)
	{
		if(!inputHistory.ContainsKey(fingerId))
		{
			if(mouseEnabled && fingerId == mouseId)
				return Input.mousePosition;
			
			Debug.LogError("An Inp.ut position was checked before Inp.ut's first update. " +
				"Don't call for position functions in other script's Start() and Awake() functions " +
				"as it doesn't make any sense for touches.");
		}
		
		if(howManyBack >= historySize)
		{
			Debug.LogError ("You can't go back further than or as far away as the touch history!");
			howManyBack = historySize - 1;
		}
		
		if(howManyBack > inputHistoryCount[fingerId] - 1)
		{
//			Debug.LogWarning("Requested previous touch position doesn't exist yet, " +
//				"so the earliest touch available was returned.");
			howManyBack = inputHistoryCount[fingerId] - 1;
		}
		
		int prevIndex = inputHistoryIndex[fingerId] - howManyBack;
		if(prevIndex < 0) prevIndex += historySize;
		
		return inputHistory[fingerId][prevIndex];
	}
	
	///<returns>Returns the delta position of a touch using Unity's unique fingerId.</returns>
	public Vector2 DeltaPosition(int fingerId)
	{
		return Position(fingerId) - PreviousPosition(fingerId);
	}
	
	///<returns>Returns the average delta position over X samples of a touch / the mouse, using Unity's unique fingerId. NOTE: CURRENTLY FRAME RATE DEPENDENT!!!!</returns>
	/// 
	/// NOTE: CURRENTLY FRAME RATE DEPENDENT!!!!!! 
	/// 
	public Vector2 DeltaPositionSample(int fingerId, int sampleLength)
	{
		
		if(!inputHistory.ContainsKey(fingerId))
			return Vector2.zero;
		
		int samples = Mathf.Min(sampleLength, inputHistoryCount[fingerId] - 1); //history-1 because we'll need to go back in time at least 1 frame
		
		if(samples <= 1)
			return Vector2.zero;
		
		Vector2 avgDeltaPos = Vector2.zero;
		int thisIndex = inputHistoryIndex[fingerId];
		int prevIndex = thisIndex - 1;
		if(prevIndex < 0) prevIndex += historySize;
		
		for(int i = 0; i < samples; i++)
		{
			thisIndex = inputHistoryIndex[fingerId] - i;
			if(thisIndex < 0) thisIndex += historySize;
			
			prevIndex = (thisIndex - 1);
			if(prevIndex < 0) prevIndex += historySize;
			
			avgDeltaPos += inputHistory[fingerId][thisIndex] - inputHistory[fingerId][prevIndex];
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
	public bool any
	{
		get 
		{
			return inputStart.Count > 0;
		}
	}
	
	///<returns>Returns true if the click or (first) touch is new this frame.</returns>
	public bool firstDown
	{
		get 
		{
			if(Application.platform == RuntimePlatform.tvOS && Input.GetButtonDown("tvOS-TouchAreaClick"))
			{
				inputStart[Input.GetTouch(0).fingerId] = new PositionAtTime(Input.GetTouch(0).position, Time.time);
				
				return true;
			}
			
			if(touchEnabled && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
			{
				return true;
			}
			
			if(mouseEnabled && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
			{
				return true;
			}

			return false;
		}
	}
	
	///<returns>Returns true if the release of a click or (first) touch is new this frame.</returns>
	public bool firstUp
	{
		get 
		{
			if(Application.platform == RuntimePlatform.tvOS && Input.GetButtonUp("tvOS-TouchAreaClick"))
			{
				return true;
			}
			
			if(touchEnabled && Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled))
			{
				return true;
			}
			
			if(mouseEnabled && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
			{
				return true;
			}

			return false;
		}
	}

	///<returns>Returns true if the release of a click or (first) touch is was quick enough or within a distance of the start time and position of the input.</returns>
	public bool firstQuickTap
	{
		get 
		{
			if(!firstUp)
				return false;
			
			if(touchEnabled && Input.touchCount > 0)
			{
				Touch touch = Input.GetTouch(0);
				
				if(Time.time - inputStart[touch.fingerId].time < timeToRegisterAsQuickTap && 
					Vector2.Distance(inputStart[touch.fingerId].position, touch.position) < distanceToRegisterAsQuickTap)
				{
					return true;
				}
			}
			
			if(mouseEnabled)
			{
				if(inputStart.Count < 1) //in case a second touch on a touchscreen with mouse has let go
					return false;
				
				if(Time.time - inputStart[mouseId].time < timeToRegisterAsQuickTap && 
					Vector2.Distance(inputStart[mouseId].position, Inp.ut.firstPosition) < distanceToRegisterAsQuickTap)
				{
					return true;
				}
			}

			return false;
		}
	}

	///<returns>Returns true if the release of a click or (first) touch can no longer be quick enough or within a distance of the start time and position of the input.</returns>
	public bool firstIsPastQuickTap
	{
		get 
		{
			if(inputStart.Count == 0)
			{
					//Debug.Log("Can't figure out this bug; inputStart is empty for some reason. Debugging:" +
					//	" any:" + any.ToString() + 
					//	", firstDown:" + firstDown.ToString() + 
					//	", firstUp:" + firstUp.ToString() + 
					//	", firstQuickTap:" + firstQuickTap.ToString() + 
					//	", firstPosition:" + firstPosition.ToString() + 
					//	", Time.time:" + Time.time.ToString() + 
					//	", inputHistoryCount[mouseId]:" + inputHistoryCount[mouseId].ToString());
				
				return true;
			}
			
			
			if(touchEnabled && Input.touchCount > 0)
			{
				Touch touch = Input.GetTouch(0);
				
				if(Time.time - inputStart[touch.fingerId].time > timeToRegisterAsQuickTap || 
					Vector2.Distance(inputStart[touch.fingerId].position, touch.position) > distanceToRegisterAsQuickTap)
				{
					return true;
				}
			}
			
			if(mouseEnabled)
			{
				if(Time.time - inputStart[mouseId].time > timeToRegisterAsQuickTap || 
					Vector2.Distance(inputStart[mouseId].position, Inp.ut.firstPosition) > distanceToRegisterAsQuickTap)
				{
					return true;
				}
			}

			return false;
		}
	}
	
	///<returns>Returns screenPosition of any input.</returns>
	public Vector2 firstPosition
	{
		get 
		{
			if(Application.platform == RuntimePlatform.tvOS)
			{
				//this is a design choice. yeah....
				return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
			}
			
			if(touchEnabled && Input.touchCount > 0)
			{
				return Position(Input.GetTouch(0).fingerId);
			}
			
			if(mouseEnabled)
			{
				return Position(mouseId);
			}

			return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
		}
	}
	
	///<returns>Returns previous screenPosition of any input.</returns>
	public Vector2 firstPreviousPosition
	{
		get 
		{
			if(touchEnabled && Input.touchCount > 0)
			{
				return PreviousPosition(Input.GetTouch(0).fingerId);
			}
			
			if(mouseEnabled)
			{
				return PreviousPosition(mouseId);
			}
			
			return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
		}
	}
	
	///<returns>Returns screenPosition of any input.</returns>
	public Vector2 firstDeltaPosition
	{
		get 
		{
			if(touchEnabled && Input.touchCount > 0)
			{
				return DeltaPosition(Input.GetTouch(0).fingerId);
			}
			
			if(mouseEnabled)
			{
				return DeltaPosition(mouseId);
			}

			return Vector2.zero;
		}
	}
	
	
	
	///<returns>Returns whether any input is hovering over Unity 5's UI.</returns>
	public bool isOverUI
	{
		get
		{
			if(EventSystem.current == null)
				return false;
			
			if(touchEnabled && Input.touchCount > 0)
			{
				foreach(Touch touch in Input.touches) 
				{
					if(inputOverUIHistory[touch.fingerId]) 
					{
						return true;
					}
				}
			}
			
			if(mouseEnabled)
			{
				return EventSystem.current.IsPointerOverGameObject();
			}
			
			return false;
		}
	}
	
	///<returns>Returns whether a specific is hovering over Unity 5's UI.</returns>
	public bool IsOverUI(int fingerId)
	{
		if(EventSystem.current == null)
			return false;
		
		if(touchEnabled && Input.touchCount > 0 && inputOverUIHistory[fingerId]) 
		{
			return true;
		}
		
		if(mouseEnabled)
		{
			return EventSystem.current.IsPointerOverGameObject();
		}
		
		return false;
	}
}