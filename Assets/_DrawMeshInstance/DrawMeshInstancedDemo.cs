using UnityEngine;

public class DrawMeshInstancedDemo : MonoBehaviour
{
	public Mesh mesh;
	public Material material;

	public int row = 10;
	public int col = 100;
	Matrix4x4[] matrix4s;
	Vector4[] colors;
	MaterialPropertyBlock block;
	private void Start()
	{
		matrix4s = new Matrix4x4[row * col];
		colors = new Vector4[row * col];

		for (int x = 0; x < row; x++)
		{
			for (int y = 0; y < col; y++)
			{
				int index = y * row + x;
				matrix4s[index] = new Matrix4x4();
				matrix4s[index].SetTRS(new Vector3(x + this.transform.position.x , 0, y + this.transform.position.z), Quaternion.identity, new Vector3(0.5f, 4, 0.5f));
				Random.seed = x*y;
				colors[index] = new Vector4(Random.value, Random.value, Random.value, 1);
			}

		}

		block = new MaterialPropertyBlock();
		block.SetVectorArray("_Color", colors);
	}


	private void Update()
	{
		Test();
	}

	[ContextMenu("Draw")]
	void Test()
	{
		Graphics.DrawMeshInstanced(mesh, 0, material, matrix4s, row * col, block);
	}
}