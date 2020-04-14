using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Windows.Forms;

public class w_VertexBoneEditor : MonoBehaviour {
	public int Vertex_;
	public InputField Bone0Input;
	public InputField Bone1Input;
	public InputField Bone2Input;
	public Text VerticeIndicator;

	public InputField Weight0Input;
	public InputField Weight1Input;
	public InputField Weight2Input;

	public void EnableWindow()
	{
		if(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex.Count == 1)
		{
			Vertex_ = int.Parse(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex[0].name);	
		}
		VerticeIndicator.text = GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex.Count.ToString() + " vertices selected";
	}

	public void SaveBone0()
	{
		if(Bone0Input.text != "")
		{
			foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 0, int.Parse(Bone0Input.text));
			}
		}
		else
		{
			MessageBox.Show("Cannot parse bone.");
		}
	}

	public void SaveBone1()
	{
		if(Bone1Input.text != "")
		{
			foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 1, int.Parse(Bone1Input.text));
			}
		}
		else
		{
			MessageBox.Show("Cannot parse bone.");
		}
	}

	public void SaveBone2()
	{
		if(Bone2Input.text != "")
		{
			foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 2, int.Parse(Bone2Input.text));
			}
		}
		else
		{
			MessageBox.Show("Cannot parse bone.");
		}
	}

	public void SaveWeight0()
	{
		if(Weight0Input.text != "")
		{
			foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeWeight(int.Parse(vertexSelected.name), 0, float.Parse(Weight0Input.text));
			}
		}
		else
		{
			MessageBox.Show("Cannot parse weight.");
		}
	}

	public void SaveWeight1()
	{
		if(Weight1Input.text != "")
		{
			foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeWeight(int.Parse(vertexSelected.name), 1, float.Parse(Weight1Input.text));
			}
		}
		else
		{
			MessageBox.Show("Cannot parse weight.");
		}
	}

	public void SaveWeight2()
	{
		if(Weight2Input.text != "")
		{
			foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeWeight(int.Parse(vertexSelected.name), 2, float.Parse(Weight2Input.text));
			}
		}
		else
		{
			MessageBox.Show("Cannot parse weight.");
		}
	}

	public void TryCloseWindow()
	{
		if(Bone0Input.text != "" || Bone1Input.text != "" || Bone2Input.text != "")
		{
			Debug.Log("You have unsaved changes. Do you want to save them?");
		}
		else
		{
			CloseWindow();
		}
	}

	public void CloseWindow()
	{
		Bone0Input.text = "";
		Bone1Input.text = "";
		Bone2Input.text = "";
		Weight0Input.text = "";
		Weight1Input.text = "";
		Weight2Input.text = "";
		GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().WindowOpen = false;
		this.gameObject.SetActive(false);
	}
}
