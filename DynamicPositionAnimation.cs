using UnityEngine;
using System.Collections;

public class DynamicPositionAnimation : MonoBehaviour 
{
	public float animatableValue = 0f;

	private Vector3 from;
	private Vector3 to;
	private bool animating;

	public void AnimateFromTo(Vector3 _from, Vector3 _to)
	{
		from = _from;
		to = _to;
		animating = true;
		GetComponent<Animator>().Play("Move");
	}

	void Update()
	{
		if(animating)
			transform.position = Vector3.Lerp(from, to, animatableValue);
	}
}
