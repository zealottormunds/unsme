using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VertexObject : MonoBehaviour {
	public bool Selected = false;

	void OnMouseOver () {
		if(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().WindowOpen == false)
		{
			if(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().byteLenght == 64)
			{
				GameObject.Find("VERTEXDATA").GetComponent<Text>().text = "VERTEX " + this.name + ":\nX = " + transform.position.x + "\nY = " + transform.position.y + "\nZ = " + transform.position.z + "\n" + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexBone[int.Parse(gameObject.name)].x.ToString() + " " + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexBone[int.Parse(gameObject.name)].y.ToString() + " " + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexBone[int.Parse(gameObject.name)].z.ToString();
			}
			else if(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().byteLenght == 28)
			{
				GameObject.Find("VERTEXDATA").GetComponent<Text>().text = "VERTEX " + this.name + ":\nX = " + transform.position.x + "\nY = " + transform.position.y + "\nZ = " + transform.position.z;
			}

			if(Input.GetMouseButton(0) && Selected == false && !Input.GetKey(KeyCode.LeftAlt))
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().SphereTest.transform.position = transform.position;
				SelectObject();
			}
			if(Input.GetMouseButton(0) && Selected == true && Input.GetKey(KeyCode.LeftAlt))
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().SphereTest.transform.position = transform.position;
				UnselectObject();
			}
		}
	}

	public void SelectObject()
	{
		Selected = true;
		GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex.Add(this.gameObject);
		this.GetComponent<Renderer>().material.color = Color.red;
	}

	public void UnselectObject()
	{
		Selected = false;
		GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex.Remove(this.gameObject);
		this.GetComponent<Renderer>().material.color = Color.white;
	}

	void OnMouseExit()
	{
		GameObject.Find("VERTEXDATA").GetComponent<Text>().text = "";
	}
}
