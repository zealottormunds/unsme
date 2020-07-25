using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VertexObject : MonoBehaviour {
	public bool Selected = false;
    public Image img;
    private GameObject Viewer;

    void Start()
    {
        Viewer = GameObject.Find("MODEL VIEWER");
       
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
}
