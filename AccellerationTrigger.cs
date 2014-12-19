using UnityEngine;
using System.Collections;

public class AccellerationTrigger : MonoBehaviour
{

	//changable
	private float accelDistanceToTrigger = 0.4f; //how much force is required to activate the trigger (0.0-1.0)
	private int longHistoryFrames = 50; //in frames, so 50 frames is 1 second with 50 fps
	private int shortHistoryFrames = 3; //in frames, so 3 frames is 0.06 seconds with 50 fps
	
	//required
	private Vector3[] longAccelHistory; 
	private Vector3[] shortAccelHistory;
	private int longHistorySlot = 0;
	private int shortHistorySlot = 0;
	private Vector3 shortAvgAccel = Vector3.zero;
	private Vector3 longAvgAccel = Vector3.zero;
	
	void Start()
	{
		longAccelHistory = new Vector3[longHistoryFrames];
		shortAccelHistory = new Vector3[shortHistoryFrames];
	}

	void FixedUpdate()
	{
		longHistorySlot = (longHistorySlot + 1) % longHistoryFrames;
		longAccelHistory[longHistorySlot] = Input.acceleration;
		
		shortHistorySlot = (shortHistorySlot + 1) % shortHistoryFrames;
		shortAccelHistory[shortHistorySlot] = Input.acceleration;
		
		Vector3 addedAccels = Vector3.zero;
		
		foreach(Vector3 acc in longAccelHistory) addedAccels += acc;
		longAvgAccel = addedAccels / longHistoryFrames;
		
		addedAccels = Vector3.zero;
		
		foreach(Vector3 acc in shortAccelHistory) addedAccels += acc;
		shortAvgAccel = addedAccels / shortHistoryFrames;
		
		if(Vector3.Distance(shortAvgAccel, longAvgAccel) > accelDistanceToTrigger)
		{
			gameObject.SendMessage("OnAccelTrigger", SendMessageOptions.RequireReceiver);
		}
	}
}
