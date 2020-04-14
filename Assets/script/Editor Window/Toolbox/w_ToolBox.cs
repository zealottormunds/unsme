using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class w_ToolBox : MonoBehaviour {
	private RenderFile r_file;
	public GameObject window_boneEditor;
	public GameObject window_uvEditor;

	public void Start()
	{
		r_file = GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>();
	}

	public void CloseWindow()
	{
		GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().WindowOpen = false;
		this.gameObject.SetActive(false);
	}

	public void OpenVertexBoneEditor()
	{
		r_file.WindowOpen = true;
		window_boneEditor.SetActive(true);
		window_boneEditor.GetComponent<w_VertexBoneEditor>().EnableWindow();
	}
}
