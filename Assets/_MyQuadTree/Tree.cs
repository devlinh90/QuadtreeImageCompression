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

	private List<TreeData> treeDatas = new List<TreeData>();

	Vector3[] grassMesh_vertices;
	int[] grassMesh_triangles;
	Vector3[] grassMesh_normals;

	private void Awake()
	{

		mesh1 = new Mesh();
		mesh2 = new Mesh();
		mesh2.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		quadTree = new QuadTree<TreeData>(null, 15f, Vector2.zero, 0);
		quadTree.SubdivdeWidthDepth(depth, quadTree);

		grassMesh_vertices = grassMesh.vertices;
		grassMesh_triangles = grassMesh.triangles;
		grassMesh_normals = grassMesh.normals;

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
					TreeData treeData = new TreeData(true);
					leaf.value = treeData;
					AddSquare(leaf);
					AddGrass(leaf);
					treeDatas.Add(treeData);
					requestUpdate = true;

				}
			}
		}

		if (Input.GetMouseButton(1))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float direction = -ray.origin.y / ray.direction.y;
			var pos = ray.GetPoint(direction);

			QuadTree<TreeData> leaf = quadTree.GetLeaf(new Vector2(pos.x, pos.z));
			if (leaf != null && leaf.value != null)
			{
				int leafIndex = treeDatas.IndexOf(leaf.value);
				if (leafIndex != -1)
				{
					//square
					Debug.Log(leafIndex);
					vertexs.RemoveAt(leafIndex);
					normals.RemoveAt(leafIndex);

					indices.RemoveAt(leafIndex * 6 + 5);
					indices.RemoveAt(leafIndex * 6 + 4);
					indices.RemoveAt(leafIndex * 6 + 3);
					indices.RemoveAt(leafIndex * 6 + 2);
					indices.RemoveAt(leafIndex * 6 + 1);
					indices.RemoveAt(leafIndex * 6);

					squareIndex--;

					//trees
					grassVertices.RemoveRange(leafIndex * grassMesh_vertices.Length, grassMesh_vertices.Length);
					grassNormals.RemoveRange(leafIndex * grassMesh_vertices.Length, grassMesh_vertices.Length);
					int length = grassMesh_triangles.Length;
					grassIndices.RemoveRange(leafIndex * length, length);
					for (int i = leafIndex * length; i < grassIndices.Count; i++)
					{
						grassIndices[i] -= grassMesh_vertices.Length;
					}

					grassIndex -= grassMesh_vertices.Length;

					leaf.value = null;
					treeDatas.RemoveAt(leafIndex);

					requestUpdate = true;

				}
			}

		}

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
	int squareIndex = 0;

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

	private void AddSquare(QuadTree<TreeData> tree)
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

		indices.Add(squareIndex * 4);
		indices.Add(squareIndex * 4 + 1);
		indices.Add(squareIndex * 4 + 3);

		indices.Add(squareIndex * 4 + 1);
		indices.Add(squareIndex * 4 + 2);
		indices.Add(squareIndex * 4 + 3);

		squareIndex++;
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

		foreach (var v in grassMesh_vertices)
		{
			Vector3 newV = new Vector3(v.x * 0.1f, 0.5f * v.y, v.z * 0.1f);
			grassVertices.Add(newV + new Vector3(tree.center.x, 0, tree.center.y));

		}
		grassNormals.AddRange(grassMesh_normals);

		foreach (var k in grassMesh_triangles)
		{
			grassIndices.Add(grassIndex + k);
		}
		grassIndex += grassMesh_vertices.Length;
	}

	private void OnDrawGizmos()
	{
		quadTree?.DrawGismos();
	}
}
