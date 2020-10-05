using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GpuInstanced : MonoBehaviour
{
	public GameObject prefabCube;

	public int row = 31;
	public int col = 31;

	private void Start()
	{
		for (int x = 0; x < row; x++)
		{
			for (int y = 0; y < col; y++)
			{
				Quaternion quaternion = Quaternion.Euler(Random.value * 360, Random.value * 360, Random.value * 360);
				var cube = Instantiate(prefabCube);
				cube.transform.position = new Vector3(x, y, 0);
				cube.transform.rotation = quaternion;
			}

		}
	}


}
