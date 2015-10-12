﻿using UnityEngine;
using System.Collections;
using System;

public class CarSkidmarks : MonoBehaviour 
{
	#region Main Attributes
	public int maxMarks = 256;			// Maximum number of marks total handled by one instance of the script.
	public float markWidth = 0.275f;		// The width of the skidmarks. Should match the width of the wheel that it is used for. In meters.
	public float groundOffset = 0.02f;	// The distance the skidmarks is places above the surface it is placed upon. In meters.
	public float minDistance = 0.1f;		// The minimum distance between two marks places next to each other. 
	int numMarks = 0;
	#endregion
	
	#region Serializables Attributes
	[Serializable]
	class MarkSection : System.Object
	{
		public Vector3 pos = Vector3.zero;
		public Vector3 normal = Vector3.zero;
		public Vector4 tangent = Vector4.zero;
		public Vector3 posl = Vector3.zero;
		public Vector3 posr = Vector3.zero;
		public float intensity = 0.0f;
		public int lastIndex = 0;
	};
	#endregion
	
	#region Other Attributes
	private MarkSection[] skidmarks;
	private bool updated = false;
	private Mesh mesh;
	private int segmentCount;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Vector4[] tangents;
	private Color[] colors;
	private Vector2[] uvs;
	private int[] triangles;
	#endregion
	
	#region Auxiliar Attributes
	private MeshFilter meshFilter;
	private MarkSection curr;
	private MarkSection last;
	private Vector3 dir;
	private Vector3 xDir;
	#endregion
	
	#region Main Methods
	private void Awake()
	{
		// Pre-Initialize values		
		skidmarks = new MarkSection[maxMarks];
		
		for (int i = 0; i < maxMarks; i++)
		{
			skidmarks[i] = new MarkSection();
		}
		
		meshFilter = GetComponent<MeshFilter>();
		
		if (meshFilter.mesh == null)
		{
			meshFilter.mesh = new Mesh();
		}
	}
	
	private void Start () 
	{ 
		mesh = meshFilter.mesh;
	}
	
	private void LateUpdate()
	{
		if (!updated)
		{
			return;
		}
		
		updated = false;
		
		mesh.Clear();
		segmentCount = 0;
		
		for (int j = 0; j < numMarks && j < maxMarks; j++)
		{
			if (skidmarks[j].lastIndex != -1 && skidmarks[j].lastIndex > numMarks - maxMarks)
			{
				segmentCount++;
			}
		}
		
		vertices = new Vector3[segmentCount * 4];
		normals = new Vector3[segmentCount * 4];
		tangents = new Vector4[segmentCount * 4];
		colors = new Color[segmentCount * 4];
		uvs = new Vector2[segmentCount * 4];
		triangles = new int[segmentCount * 6];
		segmentCount = 0;
		
		for (int i = 0; i < numMarks && i < maxMarks; i++)
		{
			if (skidmarks[i].lastIndex != -1 && skidmarks[i].lastIndex > numMarks - maxMarks)
			{
				curr = skidmarks[i];
				last = skidmarks[curr.lastIndex % maxMarks];
				vertices[segmentCount * 4 + 0] = last.posl;
				vertices[segmentCount * 4 + 1] = last.posr;
				vertices[segmentCount * 4 + 2] = curr.posl;
				vertices[segmentCount * 4 + 3] = curr.posr;
				
				normals[segmentCount * 4 + 0] = last.normal;
				normals[segmentCount * 4 + 1] = last.normal;
				normals[segmentCount * 4 + 2] = curr.normal;
				normals[segmentCount * 4 + 3] = curr.normal;
				
				tangents[segmentCount * 4 + 0] = last.tangent;
				tangents[segmentCount * 4 + 1] = last.tangent;
				tangents[segmentCount * 4 + 2] = curr.tangent;
				tangents[segmentCount * 4 + 3] = curr.tangent;
				
				colors[segmentCount * 4 + 0] = new Color(0, 0, 0, last.intensity);
				colors[segmentCount * 4 + 1] = new Color(0, 0, 0, last.intensity);
				colors[segmentCount * 4 + 2] = new Color(0, 0, 0, curr.intensity);
				colors[segmentCount * 4 + 3] = new Color(0, 0, 0, curr.intensity);
				
				uvs[segmentCount * 4 + 0] = new Vector2(0, 0);
				uvs[segmentCount * 4 + 1] = new Vector2(1, 0);
				uvs[segmentCount * 4 + 2] = new Vector2(0, 1);
				uvs[segmentCount * 4 + 3] = new Vector2(1, 1);
				
				triangles[segmentCount * 6 + 0] = segmentCount * 4 + 0;
				triangles[segmentCount * 6 + 2] = segmentCount * 4 + 1;
				triangles[segmentCount * 6 + 1] = segmentCount * 4 + 2;
				
				triangles[segmentCount * 6 + 3] = segmentCount * 4 + 2;
				triangles[segmentCount * 6 + 5] = segmentCount * 4 + 1;
				triangles[segmentCount * 6 + 4] = segmentCount * 4 + 3;
				segmentCount++;
			}
		}
		
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.triangles = triangles;
		mesh.colors = colors;
		mesh.uv = uvs;
	}
	#endregion
	
	#region Skidmarks Methods
	public int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, int lastIndex)
	{
		if (intensity > 1)
		{
			intensity = 1.0f;
		}
		
		if (intensity < 0)
		{
			return -1;
		}
		
		curr = skidmarks[numMarks % maxMarks];
		curr.pos = pos + normal * groundOffset;
		curr.normal = normal;
		curr.intensity = intensity;
		curr.lastIndex = lastIndex;
		
		if (lastIndex != -1)
		{
			last = skidmarks[lastIndex % maxMarks];
			dir = (curr.pos - last.pos);
			xDir = Vector3.Cross(dir, normal).normalized;
			
			curr.posl = curr.pos + xDir * markWidth * 0.5f;
			curr.posr = curr.pos - xDir * markWidth * 0.5f;
			curr.tangent = new Vector4(xDir.x, xDir.y, xDir.z, 1);
			
			if (last.lastIndex == -1)
			{
				last.tangent = curr.tangent;
				last.posl = curr.pos + xDir * markWidth * 0.5f;
				last.posr = curr.pos - xDir * markWidth * 0.5f;
			}
		}
		
		numMarks++;
		updated = true;
		return numMarks - 1;
	}
	#endregion
}