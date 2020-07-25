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
    public InputField Bone3Input;
    public Text VerticeIndicator;

	public InputField Weight0Input;
	public InputField Weight1Input;
	public InputField Weight2Input;
    public InputField Weight3Input;

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
            int b = int.Parse(Bone0Input.text);
            foreach (GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 0, b);
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
            int b = int.Parse(Bone1Input.text);
            foreach (GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 1, b);
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
            int b = int.Parse(Bone2Input.text);
            foreach (GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 2, b);
			}
		}
		else
		{
			MessageBox.Show("Cannot parse bone.");
		}
	}

    public void SaveBone3()
    {
        if (Bone3Input.text != "")
        {
            int b = int.Parse(Bone3Input.text);
            foreach (GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
            {
                GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeBoneID(int.Parse(vertexSelected.name), 3, b);
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
            float w = float.Parse(Weight0Input.text.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
            foreach (GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeWeight(int.Parse(vertexSelected.name), 0, w);
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
            float w = float.Parse(Weight1Input.text.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
			foreach(GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeWeight(int.Parse(vertexSelected.name), 1, w);
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
            float w = float.Parse(Weight2Input.text.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
            foreach (GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeWeight(int.Parse(vertexSelected.name), 2, w);
			}
		}
		else
		{
			MessageBox.Show("Cannot parse weight.");
		}
	}

    public void SaveWeight3()
    {
        if (Weight3Input.text != "")
        {
            float w = float.Parse(Weight3Input.text.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
            foreach (GameObject vertexSelected in GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().selectedVertex)
            {
                GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().ChangeWeight(int.Parse(vertexSelected.name), 3, w);
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
        Bone3Input.text = "";
        Weight0Input.text = "";
		Weight1Input.text = "";
		Weight2Input.text = "";
        Weight3Input.text = "";
        GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().WindowOpen = false;
		this.gameObject.SetActive(false);
	}
}
