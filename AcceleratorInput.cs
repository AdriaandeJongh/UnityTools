using UnityEngine;
using System.Collections;

public class AcceleratorInput : MonoBehaviour 
{
	private static AcceleratorInput instance;
	private Vector3[] accelHistory; 
	private int historySize = 50;   //1 second with 50 fps
	private int historyIndex = 0;
	
	void Start()
	{
		if(instance != null)
		{
			Debug.LogError("Two AcceleratorInput classes in the scene! (Click me to see the " +
				"first AcceleratorInput script)", instance.gameObject);
			Debug.LogError("(Click me to see the second AcceleratorInput script(", this.gameObject);
			Debug.Break();
		}

		instance = this;
		accelHistory = new Vector3[historySize];
	}

	void FixedUpdate()
	{
		accelHistory[historyIndex] = Input.acceleration;
		historyIndex = (historyIndex + 1) % historySize;
	}
	
	public static Vector3 Sample(int sampleLength)
	{
		if(instance == null)
		{
			Debug.LogError("You need to attach the AcceleratorInput class to an object " +
				"in the scene before you can get the acceleration.");
			Debug.Break();
			return Vector3.zero;
		}

		Vector3 addedAccels = Vector3.zero;
		int thisIndex = instance.historyIndex;
		
		for(int i = 0; i < sampleLength; i++)
		{
			thisIndex = (instance.historyIndex - i) % instance.historySize;
			addedAccels += instance.accelHistory[thisIndex];
		}
		
		addedAccels /= sampleLength;
		
		return addedAccels;
	}
}
