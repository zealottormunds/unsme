using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using System.Windows.Forms;
using System.IO;

public class ModelLoad_Main : MonoBehaviour {
	private byte[] fileBytes;
	private List<int> indexOfMeshes = new List<int>();
	private int SizeHeader = 48;

	public GameObject ListItem;
	public int SelectedIndex = 0;

	public bool stageMode = false;

	public void OpenUnsmfFolder(int stage)
	{
		try
		{
			System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			folderBrowserDialog1.ShowDialog();

			int VertexLenght = int.Parse(File.ReadAllText(folderBrowserDialog1.SelectedPath + "\\modelVertexLenght.unsmf"));
			byte[] vertexBytes = File.ReadAllBytes(folderBrowserDialog1.SelectedPath + "\\modelVertices.unsmf");
			byte[] triangleBytes = new byte[0];
			byte[] textureCoordBytes = new byte[0];

			if(File.Exists(folderBrowserDialog1.SelectedPath + "\\modelTriangles.unsmf") == true)
			{
				triangleBytes = File.ReadAllBytes(folderBrowserDialog1.SelectedPath + "\\modelTriangles.unsmf");
			}

			if(File.Exists(folderBrowserDialog1.SelectedPath + "\\modelTriangles.unsmf") == true)
			{
				textureCoordBytes = File.ReadAllBytes(folderBrowserDialog1.SelectedPath + "\\modelTextureCoords.unsmf");
			}

			if(stage == 1)
			{
				stageMode = true;
			}

			if(folderBrowserDialog1.SelectedPath != "" && Directory.Exists(folderBrowserDialog1.SelectedPath))
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().OpenModelFromUnsmf(VertexLenght, triangleBytes, textureCoordBytes, vertexBytes, stageMode);
				Destroy(GameObject.Find("Welcome Screen"));
			}
		}
		catch
		{
			stageMode = false;
		}
	}
	
	public void LoadXfbinWindow()
	{
		System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		openFileDialog1.DefaultExt = "xfbin";
		openFileDialog1.AddExtension = true;
		openFileDialog1.ShowDialog();

		string pathToXfbin = openFileDialog1.FileName;

		if(pathToXfbin != "" && File.Exists(pathToXfbin))
		{
			fileBytes = File.ReadAllBytes(pathToXfbin);
			foreach(Transform child_ in GameObject.Find("Mesh List Content").transform)
			{
				Destroy(child_.gameObject);
			}
			GameObject.Find("Mesh List Content").GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
			indexOfMeshes.Clear();

			if(fileBytes[0] == 78 && fileBytes[1] == 85 && fileBytes[2] == 67 && fileBytes[3] == 67)
            {
                int MeshCount = 0;
                for(int x = 0; x < fileBytes.Length - 3; x++)
                {
                    if(fileBytes[x] == 78 && fileBytes[x + 1] == 68 && fileBytes[x + 2] == 80 && fileBytes[x + 3] == 51)
                    {
                        indexOfMeshes.Add(x);
                        List<char> meshName = new List<char>();
                        int meshSize = 48 + (fileBytes[x + 17] * 65536 + fileBytes[x + 18] * 256 + fileBytes[x + 19]) + (fileBytes[x + 21] * 65536 + fileBytes[x + 22] * 256 + fileBytes[x + 23]) + (fileBytes[x + 25] * 65536 + fileBytes[x + 26] * 256 + fileBytes[x + 27]) + (fileBytes[x + 29] * 65536 + fileBytes[x + 30] * 256 + fileBytes[x + 31]);

                        for (int abc = 0; abc < 25; abc++)
                        {
                            meshName.Add((char)fileBytes[x + meshSize + abc]);
                        }

                        int VertexLenght_;
                        if(fileBytes[x + 29] * 65536 + fileBytes[x + 30] * 256 + fileBytes[x + 31] == 0)
                        {
                        	VertexLenght_ = 28;
                        }
                        else
                        {
                        	VertexLenght_ = 64;
                        }
                        GameObject ListItem_ = Instantiate(ListItem);
						GameObject.Find("Mesh List Content").GetComponent<RectTransform>().sizeDelta = GameObject.Find("Mesh List Content").GetComponent<RectTransform>().sizeDelta + new Vector2(0, 37.25f);
                        ListItem_Properties ListItemNew = ListItem_.GetComponent<ListItem_Properties>();
                        ListItemNew.MeshName = new string(meshName.ToArray());
                        ListItemNew.VertexLenght = VertexLenght_;
                        ListItemNew.MeshName_UI.text = ListItemNew.MeshName;
                        ListItemNew.VertexLenght_UI.text = "Vertex Lenght: " + ListItemNew.VertexLenght.ToString();
						ListItem_.transform.SetParent(GameObject.Find("Mesh List Content").transform);
						ListItem_.transform.localScale = new Vector3(1, 1, 1);
						ListItemNew.MeshID = ListItem_.transform.GetSiblingIndex();
						ListItem_.GetComponent<Button>().onClick.AddListener(() => ListItemNew.SelectThisIndex());
						ListItem_.name = "List Item " + MeshCount.ToString();
						MeshCount++;
                    }
                }
                System.Windows.Forms.MessageBox.Show("Import finished. Found " + indexOfMeshes.Count.ToString() + " meshes in file.");
        	}
        	else
        	{
        		System.Windows.Forms.MessageBox.Show("Not a valid .xfbin file.");
        	}
		}
	}

	public void ChangeSelectedIndex(int NewIndex)
	{
		SelectedIndex = NewIndex;
	}

	public void OpenWithStageMode()
	{
		if(indexOfMeshes.Count > 1)
		{
			stageMode = true;
			OpenFile();
		}
		else
		{
			System.Windows.Forms.MessageBox.Show("No .xfbin loaded.");
		}
	}

	public void OpenFile()
	{
		if(indexOfMeshes.Count > 0)
		{
			List<byte> triangleBytes = new List<byte>();
			List<byte> textureBytes = new List<byte>();
			List<byte> vertexBytes = new List<byte>();

			int VertexLenght = 0;
			int SizeOfNDP3 = 0;
			int SizeFirst = 0;
			int SizeTriangles = 0;
			int SizeTextureCoords = 0;
			int SizeVertices = 0;

			SizeOfNDP3 = fileBytes[indexOfMeshes[SelectedIndex] + 5] * 65536 + fileBytes[indexOfMeshes[SelectedIndex] + 6] * 256 + fileBytes[indexOfMeshes[SelectedIndex] + 7];
			List<byte> meshBytes = new List<byte>();

			for(int x = indexOfMeshes[SelectedIndex]; x < indexOfMeshes[SelectedIndex] + SizeOfNDP3; x++)
			{
				meshBytes.Add(fileBytes[x]);
			}

			VertexLenght = GameObject.Find("Mesh List Content").transform.Find("List Item " + SelectedIndex.ToString()).GetComponent<ListItem_Properties>().VertexLenght;
			SizeHeader = 48;
			SizeFirst = meshBytes[16] * 16777216 + meshBytes[17] * 65536 + meshBytes[18] * 256 + meshBytes[19];
			SizeTriangles = meshBytes[20] * 16777216 + meshBytes[21] * 65536 + meshBytes[22] * 256 + meshBytes[23];

			if(VertexLenght == 64)
			{
				SizeTextureCoords = meshBytes[24] * 16777216 + meshBytes[25] * 65536 + meshBytes[26] * 256 + meshBytes[27];
				SizeVertices = meshBytes[28] * 16777216 + meshBytes[29] * 65536 + meshBytes[30] * 256 + meshBytes[31];
			}
			else
			{
				SizeVertices = meshBytes[24] * 16777216 + meshBytes[25] * 65536 + meshBytes[26] * 256 + meshBytes[27];
			}

			// GENERATE TRIANGLE FILE
			for(int x = 0; x < SizeTriangles; x++)
			{
				triangleBytes.Add(meshBytes[SizeHeader + SizeFirst + x]);
			}

			// CHECK VERTEX LENGHT
			if(VertexLenght == 64)
			{
				// GENERATE TEXTURE COORDS FILE
				for(int x = 0; x < SizeTextureCoords; x++)
				{
					textureBytes.Add(meshBytes[SizeHeader + SizeFirst + SizeTriangles + x]);
				}
				// GENERATE VERTICES FILE
				for(int x = 0; x < SizeVertices; x++)
				{
					vertexBytes.Add(meshBytes[SizeHeader + SizeFirst + SizeTriangles + SizeTextureCoords + x]);
				}
			}
			else if(VertexLenght == 28)
			{
				// GENERATE VERTICES FILE
				for(int x = 0; x < SizeVertices; x++)
				{
					vertexBytes.Add(meshBytes[SizeHeader + SizeFirst + SizeTriangles + x]);
				}
			}

			if(stageMode == true)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().stageMode = true;
			}
			int a_ = indexOfMeshes[SelectedIndex];
			GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().OpenModelFromXfbin(VertexLenght, fileBytes, meshBytes.ToArray(), a_, triangleBytes.ToArray(), textureBytes.ToArray(), vertexBytes.ToArray());
			Destroy(GameObject.Find("Welcome Screen"));
		}
		else
		{
			System.Windows.Forms.MessageBox.Show("No .xfbin loaded.");
		}
	}
}
