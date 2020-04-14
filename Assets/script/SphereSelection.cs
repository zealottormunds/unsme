using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SphereSelection : MonoBehaviour {
	bool Add = true;

	void Update()
	{
		if(Input.GetMouseButtonUp(0))
		{
			GameObject.Find("Selection Sphere").transform.position = new Vector3(65535, 65535, 65535);
		}

		if(Input.GetKey(KeyCode.LeftAlt))
		{
			Add = false;
		}
		else
		{
			Add = true;
		}
	}

	void OnCollisionStay(Collision col)
	{
		if(col.gameObject.tag == "Vertex")
		{
			if(Add && col.gameObject.GetComponent<VertexObject>().Selected == false) col.gameObject.GetComponent<VertexObject>().SelectObject();
			if(!Add && col.gameObject.GetComponent<VertexObject>().Selected == true) col.gameObject.GetComponent<VertexObject>().UnselectObject();
		}
	}

	public void SetSphereSize()
	{
		float Size = float.Parse(GameObject.Find("SizeOfSphere").GetComponent<Text>().text);
		this.gameObject.transform.localScale = new Vector3(Size, Size, Size);
	}
}
