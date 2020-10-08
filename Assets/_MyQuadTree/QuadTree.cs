using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<T> where T : class
{
	public static Vector2[] directions = { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) };
	public Vector2 center;
	public float size;
	public T value;

	public QuadTree<T>[] nodes; // topleft, topright, bottomleft, bottom right;
	public int depth = 0;

	public QuadTree(T value, float size, Vector2 pos, int depth)
	{
		this.value = value;
		this.size = size;
		this.center = pos;
		this.depth = depth;
	}


	public bool IsFull
	{
		get
		{
			foreach (var leaf in GetLeafs())
			{
				if (leaf.value == null)
				{
					return false;
				}
			}

			return true;
		}
	}

	public bool Contains(Vector2 pos)
	{
		float half = 0.5f * size;
		if (Mathf.Abs(pos.x - center.x) > half)
		{
			return false;
		}
		if (Mathf.Abs(pos.y - center.y) > half)
		{
			return false;
		}

		return true;
	}

	public QuadTree<T> GetLeaf(Vector2 pos)
	{
		if (!Contains(pos))
			return null;

		if (IsLeaf)
		{
			return this;
		}
		else
		{
			foreach (var node in nodes)
			{
				QuadTree<T> quadTree = node.GetLeaf(pos);
				if (quadTree != null)
				{
					return quadTree;
				}
			}

		}
		return null;
	}

	public bool Remove(Vector2 pos)
	{
		if (!Contains(pos))
			return false;

		if (IsLeaf)
		{
			this.value = null;
			return true;
		}
		else
		{
			foreach (var node in nodes)
			{
				if (node.Remove(pos))
				{
					return true;
				}
			}

		}
		return false;
	}

	public bool Insert(T value, Vector2 pos)
	{
		if (IsLeaf)
		{
			if (Contains(pos) && this.value == null)
			{
				this.value = value;
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			foreach (var node in nodes)
			{
				if (node.Insert(value, pos))
				{
					return true;
				}
			}
		}

		return false;
	}

	public bool IsLeaf => nodes == null;

	public IEnumerable<QuadTree<T>> GetLeafs()
	{
		if (this.IsLeaf)
			yield return this;
		else
		{
			foreach (var node in nodes)
			{
				foreach (var n in node.GetLeafs())
				{
					yield return n;
				}
			}
		}
	}

	public IEnumerable<QuadTree<T>> GetFullTrees()
	{
		if (this.IsFull)
		{
			yield return this;
		}
		else
		{
			if (!IsLeaf)
			{
				foreach (var node in nodes)
				{
					foreach (var n in node.GetFullTrees())
					{
						yield return n;
					}
				}
			}
		}
	}

	public QuadTree<T>[] Subdivide()
	{
		nodes = new QuadTree<T>[4];
		for (int i = 0; i < nodes.Length; i++)
		{
			var pos = size * 0.5f * directions[i] + this.center;
			nodes[i] = new QuadTree<T>(null, size / 2, pos, this.depth + 1);
		}

		return nodes;
	}

	public void SubdivdeWidthDepth(int _depth, QuadTree<T> root)
	{
		if (_depth <= 0)
			return;

		var subs = root.Subdivide();
		_depth--;
		if (_depth > 0)
		{
			foreach (var sub in subs)
			{
				SubdivdeWidthDepth(_depth, sub);
			}
		}
	}

	public void DrawGismos()
	{
		foreach (var tree in GetLeafs())
		{
			Gizmos.DrawWireCube(new Vector3( tree.center.x,0, tree.center.y), new Vector3(1, 1, 1) * tree.size);
		}
	}


}
