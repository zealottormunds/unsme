using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Windows.Forms;

public class BoneEditor : MonoBehaviour {
	bool customBones = false;
	string path = "Assets/Resources/bones.txt";
	string[] boneFile;

	public int Vertex_;
	public Dropdown[] BoneInput;
	public Text VerticeIndicator;

	public InputField Weight0Input;
	public InputField Weight1Input;
	public InputField Weight2Input;

	void Start () {
		List<string> BoneListCmn = new List<string>();

		if(GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().CustomBones.Count > 0)
		{
			customBones = true;
		}

		if(customBones == false)
		{
			boneFile = File.ReadAllLines(path);
			BoneListCmn = new List<string>(boneFile);
		}
		else
		{
			BoneListCmn = GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().CustomBones;
		}

		for(int x = 0; x < 3; x++)
		{
			for(int y = 0; y < BoneListCmn.Count; y++)
			{
				Dropdown.OptionData opt = new Dropdown.OptionData((y + 1).ToString() + " - " + BoneListCmn[y]);
				BoneInput[x].options.Add(opt);
			}
			BoneInput[x].value = 0;
			BoneInput[x].RefreshShownValue();
		}
	}

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
		foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
		{
			GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 0, BoneInput[0].value + 1);
		}
	}

	public void SaveBone1()
	{
		foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
		{
			GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 1, BoneInput[1].value + 1);
		}
	}

	public void SaveBone2()
	{
		foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
		{
			GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 2, BoneInput[2].value + 1);
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

	public void CloseWindow()
	{
		BoneInput[0].value = 0;
		BoneInput[1].value = 0;
		BoneInput[2].value = 0;
		Weight0Input.text = "";
		Weight1Input.text = "";
		Weight2Input.text = "";
	}
}
