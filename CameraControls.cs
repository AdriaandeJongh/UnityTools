using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraControls : MonoBehaviour 
{
	public static float minimumOrthographicSize = 3.5f;
	public static float maximumOrthographicSize = 7.0f;
	private float wantedOrthographicSize = 4.0f;

	private Camera cam;
	private Vector3 inertia = Vector3.zero;
	private Bounds cameraBounds = new Bounds(Vector3.zero, Vector3.one);

	private bool waitForInteraction = false;
	private bool controlsEnabled = true;

	void Start ()
	{
		cam = GetComponent<Camera>();
		
		//determine the camera bounds
		foreach (Renderer r in FindObjectsOfType(typeof(Renderer)))
		{
			if(r.gameObject.layer != 5) //ignore the characters in the UI!
				cameraBounds.Encapsulate(r.bounds);
		}

		//zoom into the level if the level is small
		if(cameraBounds.extents.y < maximumOrthographicSize)
		{
			cam.orthographicSize = Mathf.Clamp(cameraBounds.extents.y, minimumOrthographicSize, maximumOrthographicSize);
		}

		wantedOrthographicSize = cam.orthographicSize;

		//contract the bounds so that we have less white space
		cameraBounds.Expand(-3f);
		
		//but make sure we don't make it toooo small...
		cameraBounds.Encapsulate(Vector3.up);
		cameraBounds.Encapsulate(Vector3.down);
		cameraBounds.Encapsulate(Vector3.left);
		cameraBounds.Encapsulate(Vector3.right);
	}

	void Update () 
	{
		if(Inp.ut.isOverUI)
			waitForInteraction = true;

		if(waitForInteraction && !Inp.ut.any)
			waitForInteraction = false;

		if(!waitForInteraction && controlsEnabled)
		{
			if(Inp.ut.mouseEnabled)
			{
				if(Input.mouseScrollDelta.y != 0f) //we are scrolling (and reposittioning at the same time)!
				{
					//calc pos before zooming
					Vector3 posBeforeZoom = Inp.ut.To2DWorld(Inp.ut.Position(Inp.mouseId));
					
					//save orth before
					float camOrth = cam.orthographicSize;
					
					//calc orth wanted
					wantedOrthographicSize = Mathf.Clamp(cam.orthographicSize - Input.mouseScrollDelta.y * 0.2f, minimumOrthographicSize, maximumOrthographicSize);
					
					//set orth wanted
					cam.orthographicSize = wantedOrthographicSize;
					
					//calc pos after
					Vector3 posAfterZoom = Inp.ut.To2DWorld(Inp.ut.Position(Inp.mouseId));
					
					//reset orth
					cam.orthographicSize = camOrth;
					
					if(posBeforeZoom != posAfterZoom)
					{
						//set the position directly here, because we don't really want inertia...
						Vector3 insideBoundsAfterZoom = cameraBounds.ClosestPoint(cam.transform.position + (posBeforeZoom - posAfterZoom));
						
						cam.transform.position = insideBoundsAfterZoom + Vector3.back * 10f;
					}
				}
				
				
				if(!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0))
					inertia = Inp.ut.To2DWorld(Inp.ut.PreviousPosition(Inp.mouseId)) - Inp.ut.To2DWorld(Inp.ut.Position(Inp.mouseId));
				else if(Input.GetMouseButtonUp(0))
				{
					Vector2 deltaSample = Inp.ut.DeltaPositionSample(Inp.mouseId, 3);
					
					if(deltaSample != Vector2.zero)
					{
						inertia = Inp.ut.To2DWorld(Inp.ut.Position(Inp.mouseId) - deltaSample) - Inp.ut.To2DWorld(Inp.ut.Position(Inp.mouseId));
					}
				}
				
			}
			
			if(Inp.ut.touchEnabled) 
			{
				if(Input.touchCount == 1)
				{
					Touch touch = Input.GetTouch(0);

					if(touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
					{
						Vector2 currentPosition = Inp.ut.Position(touch.fingerId);
						Vector2 deltaSample = Inp.ut.DeltaPositionSample(touch.fingerId, 3);
						
						if(deltaSample != Vector2.zero)
						{
							//if we're not standing still...
							Vector2 sampledPrevPos = currentPosition - deltaSample;
							inertia = Inp.ut.To2DWorld(sampledPrevPos) - Inp.ut.To2DWorld(Inp.ut.Position(touch.fingerId));
						}
					}
					else
					{
						inertia = Inp.ut.To2DWorld(Inp.ut.PreviousPosition(touch.fingerId))
							- Inp.ut.To2DWorld(Inp.ut.Position(touch.fingerId));
					}

					//handle all the special cases for tvOS
					if(Application.platform == RuntimePlatform.tvOS)
					{
						inertia *= -0.3f;
					}
				} 
				else if(Input.touchCount >= 2)
				{
					int id0 = Input.GetTouch(0).fingerId;
					int id1 = Input.GetTouch(1).fingerId;

					// Find the magnitude (distance) of the vector between the touches in the current & previous frame.
					float prevTouchDeltaMag = (Inp.ut.PreviousPosition(id0) - Inp.ut.PreviousPosition(id1)).magnitude;
					float touchDeltaMag = (Inp.ut.Position(id0) - Inp.ut.Position(id1)).magnitude;
					
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

					//aaand set inertia so that the camera moves according to the average of both touches
					//(in the rare case that you let go of two touches in one frame there might be a halt if the 
					// last frame reports no delta position, because no sampling happens here... oh well :)
					inertia = Inp.ut.To2DWorld((Inp.ut.PreviousPosition(id0) + Inp.ut.PreviousPosition(id1)) / 2f)
						- Inp.ut.To2DWorld((Inp.ut.Position(id0) + Inp.ut.Position(id1)) / 2f);

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
			
			
			cam.orthographicSize = wantedOrthographicSize;
			
		}

		if(!Inp.ut.any || Inp.ut.isOverUI || waitForInteraction)
			inertia *= 0.9f;

		Vector3 insideBounds = cameraBounds.ClosestPoint(cam.transform.position + inertia);

		cam.transform.position = insideBounds + Vector3.back * 10f;
	}

	public void WaitForInteraction()
	{
		inertia = Vector3.zero;
		waitForInteraction = true;
	}

	public void SetControllable (bool controllable)
	{
		controlsEnabled = controllable;
	}

}
