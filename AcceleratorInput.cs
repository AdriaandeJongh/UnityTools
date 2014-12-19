using UnityEngine;
using System.Collections;

public class AcceleratorInput : MonoBehaviour 
{
	
	public static AcceleratorInput instance;
	private Vector3[] accelHistory; 
	private int historySize = 50;   //1 second with 50 fps
	private int historyIndex = 0;
	
	void Start()
	{
		instance = this;
		accelHistory = new Vector3[historySize];
	}

	void FixedUpdate()
	{
		accelHistory[historyIndex] = Input.acceleration;
		historyIndex = (historyIndex + 1) % historySize;
	}
	
	public Vector3 GetAcceleration(int sampleLength)
	{
		Vector3 addedAccels = Vector3.zero;
		int thisIndex = historyIndex;
		
		for(int i = 0; i < sampleLength; i++)
		{
			thisIndex = (historyIndex - i) % historySize;
			addedAccels += accelHistory[thisIndex];
		}
		
		addedAccels /= sampleLength;
		
		return addedAccels;
	}
}
