using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementWithAxis : MonoBehaviour {
	public int AxisNum = 1;
	bool Moving = false;
	Vector3 Mov = new Vector3();

	void OnMouseOver () {
		if(Input.GetMouseButtonDown(0))
		{
			Moving = true;
		}
	}

	void Update()
	{
		if(Input.GetMouseButtonUp(0))
		{
			Moving = false;
			GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().mf.mesh.SetVertices(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().meshVertices);
		}
		if(Moving && (Input.GetAxis("Mouse X") < -0.1f || Input.GetAxis("Mouse X") > 0.1f))
		{
			List<GameObject> Vertices = GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex;

			switch(AxisNum)
			{
				case 1:
					Mov.x += Input.GetAxis("Mouse X")*3;
				break;
				case 2:
					Mov.y += Input.GetAxis("Mouse X")*3;
				break;
				case 3:
					Mov.z += Input.GetAxis("Mouse X")*3;
				break;
			}

			for(int x = 0; x < Vertices.Count; x++)
			{
				Vertices[x].transform.position += Mov;
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().meshVertices[x] = Vertices[x].transform.position;
			}
		}
	}
}
