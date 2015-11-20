// Only works together with Inp.ut
// By Adriaan de Jongh, http://adriaandejongh.nl
// More info & other Unity scripts: https://github.com/AdriaandeJongh/UnityTools


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraControls : MonoBehaviour 
{
	public static CameraControls instance;

	private float minimumOrthographicSize = 4.0f;
	private float maximumOrthographicSize = 7.0f;
	private float wantedOrthographicSize = 4.0f;

	private Camera cam;
	private Vector3 inertia = Vector3.zero;
	private Bounds cameraBounds = new Bounds(Vector3.zero, Vector3.one);

	private bool waitForInteraction = false;

	void Awake()
	{
		instance = this;
	}

	void Start ()
	{
		cam = Camera.main;
		wantedOrthographicSize = cam.orthographicSize;

		Application.targetFrameRate = 60;

		foreach (Renderer r in FindObjectsOfType(typeof(Renderer)))
		{
			if(r.gameObject.layer != 5) //ignore the characters in the UI!
				cameraBounds.Encapsulate(r.bounds);
		}
	}

	void Update () 
	{
		if(Inp.ut.isOverUI)
			waitForInteraction = true;

		if(waitForInteraction && !Inp.ut.anyInput)
			waitForInteraction = false;

		if(!Inp.ut.isOverUI && !waitForInteraction)
		{
			if(Application.isMobilePlatform) 
			{
				if(Input.touchCount == 1)
				{
					Touch touch = Input.GetTouch(0);

					if(touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
					{
						//Only sample when we've let go, so that there is no delay!

						Vector2 currentPosition = Inp.ut.TouchPosition(touch.fingerId);
						Vector2 deltaSample = Inp.ut.TouchDeltaPositionSample(touch.fingerId, 3);

						if(deltaSample != Vector2.zero)
						{
							//if we're not standing still...
							Vector2 sampledPrevPos = currentPosition - deltaSample;
							inertia = Inp.ut.To2DWorld(sampledPrevPos) - Inp.ut.To2DWorld(Inp.ut.TouchPosition(touch.fingerId));
						}
					}
					else
					{
						inertia = Inp.ut.To2DWorld(Inp.ut.TouchPreviousPosition(touch.fingerId))
							- Inp.ut.To2DWorld(Inp.ut.TouchPosition(touch.fingerId));
					}
				} 
				else if(Input.touchCount >= 2)
				{
					int id0 = Input.GetTouch(0).fingerId;
					int id1 = Input.GetTouch(1).fingerId;

					// Find the magnitude (distance) of the vector between the touches in the current & previous frame.
					float prevTouchDeltaMag = (Inp.ut.TouchPreviousPosition(id0) - Inp.ut.TouchPreviousPosition(id1)).magnitude;
					float touchDeltaMag = (Inp.ut.TouchPosition(id0) - Inp.ut.TouchPosition(id1)).magnitude;
					
					// Find the difference in the distances between each frame.
					float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

					// ... change the orthographic size based on the change in distance between the touches.
					if(wantedOrthographicSize > maximumOrthographicSize)
					{
						wantedOrthographicSize += deltaMagnitudeDiff * 0.004f;
					}
					else if(wantedOrthographicSize < minimumOrthographicSize)
					{
						wantedOrthographicSize += deltaMagnitudeDiff * 0.0015f;
					}
					else
					{
						wantedOrthographicSize += deltaMagnitudeDiff * 0.008f;
					}

					//set camera!
					cam.orthographicSize = wantedOrthographicSize;

					//aaand set inertia so that the camera moves according to the average of both touches
					//(in the rare case that you let go of two touches in one frame there might be a halt if the 
					// last frame reports no delta position, because no sampling happens here... oh well :)
					inertia = Inp.ut.To2DWorld((Inp.ut.TouchPreviousPosition(id0) + Inp.ut.TouchPreviousPosition(id1)) / 2f)
						- Inp.ut.To2DWorld((Inp.ut.TouchPosition(id0) + Inp.ut.TouchPosition(id1)) / 2f);

				}

				if(Input.touchCount < 2)
				{
					if(wantedOrthographicSize > maximumOrthographicSize + 0.01f)
					{
						wantedOrthographicSize -= (wantedOrthographicSize - maximumOrthographicSize) * 0.1f;
						cam.orthographicSize = wantedOrthographicSize;
					}
					else if(wantedOrthographicSize < minimumOrthographicSize - 0.01f)
					{
						wantedOrthographicSize += (minimumOrthographicSize - wantedOrthographicSize) * 0.1f;
						cam.orthographicSize = wantedOrthographicSize;
					}
				}

			}
			else
			{
				cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel"), minimumOrthographicSize, maximumOrthographicSize);


				if(Input.GetMouseButton(0))
					inertia = Inp.ut.To2DWorld(Inp.ut.mousePreviousPosition) - Inp.ut.To2DWorld(Inp.ut.mousePosition);
				else if(Input.GetMouseButtonUp(0))
					inertia = Inp.ut.To2DWorld(Inp.ut.mousePosition - Inp.ut.MouseDeltaPositionSample(3)) - Inp.ut.To2DWorld(Inp.ut.mousePosition);

			}
		}

		if(!Inp.ut.anyInput || Inp.ut.isOverUI || waitForInteraction)
			inertia *= 0.9f;

		cam.transform.position = cameraBounds.ClosestPoint(cam.transform.position + inertia);
	}

	public void WaitForInteraction()
	{
		inertia = Vector3.zero;
		waitForInteraction = true;
	}
}
