using UnityEngine;
using System.Collections;
using Furiosity;

[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]
public class ColliderToMesh : MonoBehaviour 
{

	public bool rebuildMesh = true;
	public Material material;
	private PolygonCollider2D coll;
	public Mesh msh; //required to be public, or this script will cause memory leaks!

	void OnEnable()
	{
		coll = GetComponent<PolygonCollider2D>();
		
		msh = new Mesh();
		msh.name = gameObject.name + "'s mesh";
		msh.UpdatePath(coll.GetPath(0));
		
		MeshFilter filter = GetComponent<MeshFilter>();
		
		if(filter == null) 
			filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		
		filter.sharedMesh = msh;
		
		if(GetComponent<MeshRenderer>() == null)
			gameObject.AddComponent(typeof(MeshRenderer));
	}

	void Start()
	{
		//Setting it to false here only when the game starts running allows you to still turn it on later.
		if(Application.isPlaying) 
			rebuildMesh = false;
	}

	//ExecuteInEditMode only updates when something in the scene changes!
	void Update() 
	{
		if(rebuildMesh)
		{
			msh.name = gameObject.name + "'s mesh";
			msh.UpdatePath(coll.GetPath(0));

			renderer.material = material;
		}
	}

	void OnDisable()
	{
		DestroyImmediate(msh);
	}
}