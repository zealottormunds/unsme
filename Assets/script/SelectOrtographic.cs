using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectOrtographic : MonoBehaviour {
	bool Add = true;

	void Update()
	{
		if(Input.GetKey(KeyCode.LeftAlt))
		{
			Add = false;
		}
		else
		{
			Add = true;
		}
	}

	void OnCollisionEnter(Collision col)
	{
		if(col.gameObject.tag == "Vertex")
		{
			if(Add && col.gameObject.GetComponent<VertexObject>().Selected == false) col.gameObject.GetComponent<VertexObject>().SelectObject();
			if(!Add && col.gameObject.GetComponent<VertexObject>().Selected == true) col.gameObject.GetComponent<VertexObject>().UnselectObject();
		}
	}
}
