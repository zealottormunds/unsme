using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereSelect : MonoBehaviour {

	void OnCollisionStay(Collision col)
	{
		if(col.gameObject.tag == "Vertex" && col.gameObject.GetComponent<VertexObject>().Selected == false && !Input.GetKey(KeyCode.LeftAlt))
		{
			col.gameObject.GetComponent<VertexObject>().SelectObject();
		}
		else if(col.gameObject.tag == "Vertex" && col.gameObject.GetComponent<VertexObject>().Selected == true && Input.GetKey(KeyCode.LeftAlt))
		{
			col.gameObject.GetComponent<VertexObject>().UnselectObject();
		}
	}
}
