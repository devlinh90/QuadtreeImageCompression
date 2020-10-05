using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public int depth = 3;
	QuadTree<TreeData> quadTree;

	// render flat
	[SerializeField] MeshFilter flatMeshFilter;

	// render grass
	[SerializeField] MeshFilter grassMeshFilter;
	[SerializeField] Mesh grassMesh;

	private bool requestUpdate;

	private void Awake()
	{
		quadTree = new QuadTree<TreeData>(null, 10f, Vector2.zero, 0);
		quadTree.SubdivdeWidthDepth(depth, quadTree);

	}

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			QuadTree<TreeData> leaf = quadTree.GetLeaf(pos);
			if (leaf != null)
			{
				if (leaf.value == null)
				{
					leaf.value = new TreeData(true);
					AddTree(leaf);
					AddGrass(leaf);
					requestUpdate = true;
				}
			}
		}
		if (Input.GetMouseButton(1))
		{
			var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			QuadTree<TreeData> leaf = quadTree.GetLeaf(pos);
			if(leaf != null && leaf.value != null)
			{

			}
			if (quadTree.Remove(pos))
			{
				requestUpdate = true;
			}
		}
		if (requestUpdate)
		{
			UpdateFlat();
			//UpdateGrass();
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
		////vertexs.Clear();
		////normals.Clear();
		////indices.Clear();

		//foreach (var tree in quadTree.GetFullTrees())
		//{
		//	if (tree.IsFull)
		//	{
		//		AddTree(tree);
		//	}
		//}


		Mesh mesh = new Mesh();

		mesh.SetVertices(vertexs);
		mesh.SetTriangles(indices, 0);
		mesh.SetNormals(normals);

		flatMeshFilter.mesh = mesh;
	}

	private void AddTree(QuadTree<TreeData> tree)
	{
		var topLeft = tree.center + new Vector2(-0.5f, 0.5f) * tree.size;
		var topRight = tree.center + new Vector2(0.5f, 0.5f) * tree.size;
		var bottomLeft = tree.center + new Vector2(-0.5f, -0.5f) * tree.size;
		var bottomRight = tree.center + new Vector2(0.5f, -0.5f) * tree.size;

		vertexs.Add(bottomLeft);
		vertexs.Add(topLeft);
		vertexs.Add(topRight);
		vertexs.Add(bottomRight);

		for (int i = 0; i < 4; i++)
		{
			normals.Add(-Vector3.forward);
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

		Mesh mesh = new Mesh();
		mesh.vertices = grassVertices.ToArray();
		mesh.triangles = grassIndices.ToArray();
		mesh.normals = grassNormals.ToArray();

		mesh.SetVertices(grassVertices);
		mesh.SetTriangles (grassIndices,0);
		mesh.SetNormals(grassNormals);

		grassMeshFilter.mesh = mesh;
	}

	private void AddGrass(QuadTree<TreeData> tree)
	{
		Vector3[] vertices = grassMesh.vertices;
		foreach (var v in vertices)
		{
			grassVertices.Add(v + new Vector3(tree.center.x, tree.center.y, 0));

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
