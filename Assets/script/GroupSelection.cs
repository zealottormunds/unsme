using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupSelection : MonoBehaviour {
	public List<string> GroupNames = new List<string>();
	public List<List<int>> Groups = new List<List<int>>();
    public List<int> TrianglesPerGroup = new List<int>();

    public void AddToGroupByVertexID(int GroupID, int VertexID)
    {
        if (Groups[GroupID].Contains(VertexID) == false)
        {
            Groups[GroupID].Add(VertexID);
        }
    }

	public void AddToGroupByID(int ID)
	{
		RenderFile MV = GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>();
		int GroupID = ID;

		for(int x = 0; x < MV.selectedVertex.Count; x++)
		{
			if(Groups[GroupID].Contains(MV.selectedVertex[x].transform.GetSiblingIndex()) == false)
			{
				Groups[GroupID].Add(MV.selectedVertex[x].transform.GetSiblingIndex());
			}
		}
	}

	public void RemoveToGroupByID(int ID)
	{
		RenderFile MV = GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>();
		int GroupID = ID;

		for(int x = 0; x < MV.selectedVertex.Count; x++)
		{
			if(Groups[GroupID].Contains(MV.selectedVertex[x].transform.GetSiblingIndex()))
			{
				Groups[GroupID].Remove(MV.selectedVertex[x].transform.GetSiblingIndex());
			}
		}
	}

	public void SelectGroupByID(int ID)
	{
		RenderFile MV = GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>();
		int GroupID = ID;

		for(int x = 0; x < Groups[GroupID].Count; x++)
		{
			VertexObject V = GameObject.Find("Model Data").transform.GetChild(Groups[GroupID][x]).GetComponent<VertexObject>();
			if(V.Selected == false)
			{
				V.SelectObject();
			}
		}
	}

	public void UnselectGroupByID(int ID)
	{
		RenderFile MV = GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>();
		int GroupID = ID;

		for(int x = 0; x < Groups[GroupID].Count; x++)
		{
			VertexObject V = GameObject.Find("Model Data").transform.GetChild(Groups[GroupID][x]).GetComponent<VertexObject>();
			if(V.Selected == true)
			{
				V.UnselectObject();
			}
		}
	}
}
