using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]
public class ColliderToMesh : MonoBehaviour 
{

	public bool rebuildMesh = true;
	public Material material;
	private PolygonCollider2D coll;
	public Mesh mesh; //required to be public, or this script will cause memory leaks!

	void OnEnable()
	{
		coll = GetComponent<PolygonCollider2D>();
		
		mesh = new Mesh();
		mesh.name = gameObject.name + "'s mesh";

		UpdatePath();
		
		MeshFilter filter = GetComponent<MeshFilter>();
		
		if(filter == null) 
			filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		
		filter.sharedMesh = mesh;
		
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
			mesh.name = gameObject.name + "'s mesh";

			UpdatePath();

			renderer.material = material;
		}
	}

	void OnDisable()
	{
		DestroyImmediate(mesh);
	}

	void UpdatePath()
	{
		Vector2[] path = coll.GetPath(0);
		Triangulator tr = new Triangulator(path);
		
		int[] indices = tr.Triangulate();
		Vector3[] vertices = new Vector3[path.Length];
		Vector2[] uvs = new Vector2[path.Length];
		
		for (int i=0; i<vertices.Length; i++) {
			vertices[i] = new Vector3(path[i].x, path[i].y, 0);
			uvs[i] = new Vector2(path[i].x, path[i].y);
		}
		
		mesh.vertices = vertices;
		mesh.triangles = indices;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
	
	//
	// Triangulator that does MAGIC
	// from http://wiki.unity3d.com/index.php?title=Triangulator
	//
	public class Triangulator
	{
		private List<Vector2> m_points = new List<Vector2>();
		
		public Triangulator(Vector2[] points)
		{
			m_points = new List<Vector2>(points);
		}
		
		public int[] Triangulate()
		{
			List<int> indices = new List<int>();
			
			int n = m_points.Count;
			if (n < 3)
				return indices.ToArray();
			
			int[] V = new int[n];
			if (Area() > 0)
			{
				for (int v = 0; v < n; v++)
					V[v] = v;
			}
			else
			{
				for (int v = 0; v < n; v++)
					V[v] = (n - 1) - v;
			}
			
			int nv = n;
			int count = 2 * nv;
			for (int m = 0, v = nv - 1; nv > 2; )
			{
				if ((count--) <= 0)
					return indices.ToArray();
				
				int u = v;
				if (nv <= u)
					u = 0;
				v = u + 1;
				if (nv <= v)
					v = 0;
				int w = v + 1;
				if (nv <= w)
					w = 0;
				
				if (Snip(u, v, w, nv, V))
				{
					int a, b, c, s, t;
					a = V[u];
					b = V[v];
					c = V[w];
					indices.Add(a);
					indices.Add(b);
					indices.Add(c);
					m++;
					for (s = v, t = v + 1; t < nv; s++, t++)
						V[s] = V[t];
					nv--;
					count = 2 * nv;
				}
			}
			
			indices.Reverse();
			return indices.ToArray();
		}
		
		private float Area()
		{
			int n = m_points.Count;
			float A = 0.0f;
			for (int p = n - 1, q = 0; q < n; p = q++)
			{
				Vector2 pval = m_points[p];
				Vector2 qval = m_points[q];
				A += pval.x * qval.y - qval.x * pval.y;
			}
			return (A * 0.5f);
		}
		
		private bool Snip(int u, int v, int w, int n, int[] V)
		{
			int p;
			Vector2 A = m_points[V[u]];
			Vector2 B = m_points[V[v]];
			Vector2 C = m_points[V[w]];
			if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
				return false;
			for (p = 0; p < n; p++)
			{
				if ((p == u) || (p == v) || (p == w))
					continue;
				Vector2 P = m_points[V[p]];
				if (InsideTriangle(A, B, C, P))
					return false;
			}
			return true;
		}
		
		private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
		{
			float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
			float cCROSSap, bCROSScp, aCROSSbp;
			
			ax = C.x - B.x; ay = C.y - B.y;
			bx = A.x - C.x; by = A.y - C.y;
			cx = B.x - A.x; cy = B.y - A.y;
			apx = P.x - A.x; apy = P.y - A.y;
			bpx = P.x - B.x; bpy = P.y - B.y;
			cpx = P.x - C.x; cpy = P.y - C.y;
			
			aCROSSbp = ax * bpy - ay * bpx;
			cCROSSap = cx * apy - cy * apx;
			bCROSScp = bx * cpy - by * cpx;
			
			return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
		}
	}
}