using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreeData
{
	public bool Show;

	public TreeData(bool show)
	{
		Show = show;
	}
}
public class Tree : MonoBehaviour
{
	[Range(1, 7)]
	public int depth = 3;
	QuadTree<TreeData> quadTree;

	// render flat
	[SerializeField] MeshFilter flatMeshFilter;

	// render grass
	[SerializeField] MeshFilter grassMeshFilter;
	[SerializeField] Mesh grassMesh;
	[SerializeField] GameObject grassPrefab;

	private bool requestUpdate;

	[SerializeField] Text countText;
	int count = 0;
	Mesh mesh1, mesh2;
	private void Awake()
	{

		mesh1 = new Mesh();
		mesh2 = new Mesh();
		mesh2.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		quadTree = new QuadTree<TreeData>(null, 15f, Vector2.zero, 0);
		quadTree.SubdivdeWidthDepth(depth, quadTree);

	}

	[ContextMenu("GCCollect")]
	public void GCCollect()
	{
		GC.Collect();
	}

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float direction = -ray.origin.y / ray.direction.y;
			var pos = ray.GetPoint(direction);

			QuadTree<TreeData> leaf = quadTree.GetLeaf(new Vector2(pos.x, pos.z));
			if (leaf != null)
			{
				if (leaf.value == null)
				{
					count++;
					countText.text = count.ToString();
					leaf.value = new TreeData(true);
					AddTree(leaf);
					AddGrass(leaf);
					requestUpdate = true;
				}
			}
		}
		//if (Input.GetMouseButton(1))
		//{
		//	var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//	QuadTree<TreeData> leaf = quadTree.GetLeaf(pos);
		//	if(leaf != null && leaf.value != null)
		//	{

		//	}
		//	if (quadTree.Remove(pos))
		//	{
		//		requestUpdate = true;
		//	}
		//}
		if (requestUpdate)
		{
			UpdateFlat();
			UpdateGrass();
			requestUpdate = false;
		}
	}
	List<Vector3> vertexs = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();
	List<int> indices = new List<int>();
	int index = 0;

	List<Vector3> grassVertices = new List<Vector3>();
	List<Vector3> grassNormals = new List<Vector3>();
	List<int> grassIndices = new List<int>();
	int grassIndex = 0;

	void UpdateFlat()
	{
		mesh1.Clear();
		mesh1.SetVertices(vertexs);
		mesh1.SetTriangles(indices, 0);
		mesh1.SetNormals(normals);

		flatMeshFilter.mesh = mesh1;
	}

	private void AddTree(QuadTree<TreeData> tree)
	{
		Vector3 center = new Vector3(tree.center.x, 0, tree.center.y);
		var topLeft = center + new Vector3(-0.5f, 0, 0.5f) * tree.size;
		var topRight = center + new Vector3(0.5f, 0, 0.5f) * tree.size;
		var bottomLeft = center + new Vector3(-0.5f, 0, -0.5f) * tree.size;
		var bottomRight = center + new Vector3(0.5f, 0, -0.5f) * tree.size;

		vertexs.Add(bottomLeft);
		vertexs.Add(topLeft);
		vertexs.Add(topRight);
		vertexs.Add(bottomRight);

		for (int i = 0; i < 4; i++)
		{
			normals.Add(Vector3.up);
		}

		indices.Add(index * 4);
		indices.Add(index * 4 + 1);
		indices.Add(index * 4 + 3);

		indices.Add(index * 4 + 1);
		indices.Add(index * 4 + 2);
		indices.Add(index * 4 + 3);

		index++;
	}

	void UpdateGrass()
	{
		mesh2.Clear();
		mesh2.SetVertices(grassVertices);
		mesh2.SetTriangles(grassIndices, 0);
		mesh2.SetNormals(grassNormals);

		mesh2.RecalculateNormals();
		grassMeshFilter.mesh = mesh2;
	}


	private void AddGrass(QuadTree<TreeData> tree)
	{
		Vector3[] vertices = grassMesh.vertices;
		foreach (var v in vertices)
		{
			Vector3 newV = new Vector3(v.x * 0.1f, 0.5f * v.y, v.z * 0.1f);
			grassVertices.Add(newV + new Vector3(tree.center.x, 0, tree.center.y));

		}
		grassNormals.AddRange(grassMesh.normals);

		var indices = grassMesh.triangles;
		foreach (var k in indices)
		{
			grassIndices.Add(grassIndex + k);
		}
		grassIndex += vertices.Length;
	}

	private void OnDrawGizmos()
	{
		quadTree?.DrawGismos();
	}
}
