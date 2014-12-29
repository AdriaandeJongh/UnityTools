// By Adriaan de Jongh, http://adriaandejongh.nl
// More Unity scripts: https://github.com/AdriaandeJongh/UnityTools

using UnityEngine;
using System.Collections;

public class AccelerationInput : MonoBehaviour 
{
	private static AccelerationInput instance;

	private Vector3[] history; 
	private int historySize = 50;   //1 second with 50 fps
	private int historyIndex = 0;

	public delegate void OnTriggerFunction();
	public OnTriggerFunction OnTrigger; //functions added to this thing will be called when the trigger hits!

	private float triggerDistance = 0.4f; //how much force is required to activate the trigger (0.0-1.0)
	private int triggerLargeSampleSize = 50; //in frames, so 50 frames is 1 second with 50 fps
	private int triggerSmallSampleSize = 3; //in frames, so 3 frames is 0.06 seconds with 50 fps
	
	void Start()
	{
		if(instance != null)
		{
			Debug.LogError("Two AccelerationInput classes in the scene! (Click me to see the " +
			               "first AccelerationInput script)", instance.gameObject);
			Debug.LogError("(Click me to see the second AccelerationInput script(", this.gameObject);
			Debug.Break();
		}

		instance = this;
		history = new Vector3[historySize];
	}

	void FixedUpdate()
	{
		history[historyIndex] = Input.acceleration;
		historyIndex = (historyIndex + 1) % historySize;

		if(OnTrigger != null)
		{
			if(Vector3.Distance(Sample(triggerSmallSampleSize), Sample(triggerLargeSampleSize)) > triggerDistance)
			{
				OnTrigger();
			}
		}
	}

	///<returns>Returns the acceleration force over X amount of frames.</returns>
	public static Vector3 Sample(int sampleFrameAmount)
	{
		if(instance == null)
		{
			Debug.LogError("You need to attach the AccelerationInput class to an object " +
			               "in the scene before you can get the acceleration.");
			Debug.Break();
			return Vector3.zero;
		}

		if(sampleFrameAmount > instance.historySize)
		{
			Debug.LogError("You can't sample bigger than the container! Increase the history size.");
			Debug.Break();
			return Vector3.zero;
		}

		Vector3 addedAccels = Vector3.zero;
		int thisIndex = instance.historyIndex;
		
		for(int i = 0; i < sampleFrameAmount; i++)
		{
			thisIndex = instance.historyIndex - i;

			if(thisIndex < 0) 
			{
				thisIndex += instance.historySize;
			}

			addedAccels += instance.history[thisIndex];
		}
		
		addedAccels /= sampleFrameAmount;
		
		return addedAccels;
	}

	///<summary>Set the function to call when the acceleration force is greater than triggerForce.</summary>
	public static void SetTrigger(OnTriggerFunction FunctionToCall, float triggerForce)
	{
		instance.OnTrigger += FunctionToCall;
		instance.triggerDistance = triggerForce;
	}
}
