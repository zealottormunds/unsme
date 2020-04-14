using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ListItem_Properties : MonoBehaviour {
	public Text MeshName_UI;
	public Text VertexLength_UI;

	public string MeshName;
	public int MeshID;
	public int MeshGroups;
	public int IndexInXfbin;
	public int LengthOfNDP3;
	public int VertexLength;

	public void SelectThisIndex()
	{
		GameObject.Find("Welcome Screen").GetComponent<ModelLoad_Main>().SelectedIndex = MeshID;
	}
}
