using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VertexObject : MonoBehaviour {
	public bool Selected = false;

	void OnMouseOver () {
		if(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().WindowOpen == false && GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().inCommand == false)
		{
			if(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().byteLength == 64)
			{
				GameObject.Find("VERTEXDATA").GetComponent<Text>().text = "VERTEX " + this.name + ":\nX = " + transform.position.x + "\nY = " + transform.position.y + "\nZ = " + transform.position.z + "\n" + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexBone[int.Parse(gameObject.name)].x.ToString() + " " + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexBone[int.Parse(gameObject.name)].y.ToString() + " " + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexBone[int.Parse(gameObject.name)].z.ToString() + "\n\nWeights:" + "\nX = " + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexWeight[int.Parse(gameObject.name)].x.ToString() + "\nY = " + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexWeight[int.Parse(gameObject.name)].y.ToString() + "\nZ = " + GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().vertexWeight[int.Parse(gameObject.name)].z.ToString();
			}
			else if(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().byteLength == 28)
			{
				GameObject.Find("VERTEXDATA").GetComponent<Text>().text = "VERTEX " + this.name + ":\nX = " + transform.position.x + "\nY = " + transform.position.y + "\nZ = " + transform.position.z;
			}
		}
		if(Input.GetMouseButton(0))
		{
			GameObject.Find("Selection Sphere").transform.position = this.gameObject.transform.position;
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
