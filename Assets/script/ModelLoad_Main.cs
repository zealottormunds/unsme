using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using System.Globalization;
using System.Windows.Forms;
using System.Diagnostics;

public class ModelLoad_Main : MonoBehaviour {
	private List<byte> fileBytes = new List<byte>();
	private List<int> indexOfMeshes = new List<int>();
	private int SizeHeader = 48;

	public GameObject ListItem;
	public int SelectedIndex = 0;
	public bool stageMode = false;
	public int OverallMeshCount = 0;

	private List<string> BoneList = new List<string>();

	public void LoadXfbinWindow()
	{
		// CLEAR OLD LIST IF THERE IS ONE
		for(int x = 0; x < GameObject.Find("Mesh List Content").transform.childCount; x++)
		{
			Destroy(GameObject.Find("Mesh List Content").transform.GetChild(0).gameObject);
		}
		indexOfMeshes = new List<int>();
		fileBytes = new List<byte>();
        SizeHeader = 48;
		SelectedIndex = 0;
		OverallMeshCount = 0;

		// NOW OPEN FILE
		System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		openFileDialog1.DefaultExt = "xfbin";
		openFileDialog1.AddExtension = true;
		openFileDialog1.ShowDialog();

		string pathToXfbin = openFileDialog1.FileName;

		if(pathToXfbin != "" && File.Exists(pathToXfbin))
		{
			byte[] file = File.ReadAllBytes(pathToXfbin);

			for(int a = 0; a < file.Length; a++)
			{
				fileBytes.Add(file[a]);
			}

			foreach(Transform child_ in GameObject.Find("Mesh List Content").transform)
			{
				Destroy(child_.gameObject);
			}
			GameObject.Find("Mesh List Content").GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
			indexOfMeshes.Clear();

			if(fileBytes[0] == 78 && fileBytes[1] == 85 && fileBytes[2] == 67 && fileBytes[3] == 67)
            {
                int MeshCount = 0;
                for(int x = 0; x < fileBytes.Count - 3; x++)
                {
                    if(fileBytes[x] == 78 && fileBytes[x + 1] == 68 && fileBytes[x + 2] == 80 && fileBytes[x + 3] == 51)
                    {
                        indexOfMeshes.Add(x);
                        List<char> meshName = new List<char>();
                        int meshSize = 48 + 
	                        (fileBytes[x + 17] * 0x10000 + fileBytes[x + 18] * 0x100 + fileBytes[x + 19]) + 
	                        (fileBytes[x + 21] * 0x10000 + fileBytes[x + 22] * 0x100 + fileBytes[x + 23]) + 
	                        (fileBytes[x + 25] * 0x10000 + fileBytes[x + 26] * 0x100 + fileBytes[x + 27]) + 
	                        (fileBytes[x + 29] * 0x10000 + fileBytes[x + 30] * 0x100 + fileBytes[x + 31]);

                        for (int abc = 0; abc < 25; abc++)
                        {
                            meshName.Add((char)fileBytes[x + meshSize + abc]);
                        }

                        string MeshType = "";

                        int VertexLength_ = 
                        	(fileBytes[x + 0x18] * 0x1000000 + fileBytes[x + 0x19] * 0x10000 + fileBytes[x + 0x1A] * 0x100 + fileBytes[x + 0x1B])
                        	/
                        	(fileBytes[x + 0x6A] * 0x1000000 + fileBytes[x + 0x6B] * 0x10000 + fileBytes[x + 0x6C] * 0x100 + fileBytes[x + 0x6D]);

						if(VertexLength_ != 0x1C && VertexLength_ != 0x20 && VertexLength_ != 0x40)
						{
							if(fileBytes[x + 28] * 0x1000000 + fileBytes[x + 29] * 0x10000 + fileBytes[x + 30] * 0x100 + fileBytes[x + 31] == 0)
	                        {
	                        	VertexLength_ = 28;
	                        }
	                        else
	                        {
	                        	VertexLength_ = 64;
	                        }
                        }

                        GameObject ListItem_ = Instantiate(ListItem);
						GameObject.Find("Mesh List Content").GetComponent<RectTransform>().sizeDelta = GameObject.Find("Mesh List Content").GetComponent<RectTransform>().sizeDelta + new Vector2(0, 37.25f);
                        ListItem_Properties ListItemNew = ListItem_.GetComponent<ListItem_Properties>();
						ListItemNew.MeshGroups = 
							fileBytes[x + 90] * 0x100 +
                        	fileBytes[x + 91] * 1;
						ListItemNew.MeshName = new string(meshName.ToArray());
                        ListItemNew.VertexLength = VertexLength_;
						ListItemNew.MeshName_UI.text = ListItemNew.MeshName + MeshType;
                        ListItemNew.VertexLength_UI.text = "Groups: " + ListItemNew.MeshGroups.ToString() + " | Length: " + ListItemNew.VertexLength.ToString();
						ListItem_.transform.SetParent(GameObject.Find("Mesh List Content").transform);
						ListItem_.transform.localScale = new Vector3(1, 1, 1);
						ListItemNew.MeshID = OverallMeshCount;
						ListItem_.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ListItemNew.SelectThisIndex());
						ListItem_.name = "List Item " + MeshCount.ToString();
						OverallMeshCount++;
						MeshCount++;
                    }
                }

				// Read bones if it is 1cmn
				DialogResult CustomBones = MessageBox.Show("(Experimental) Do you want the tool to search for bone names inside the file and use the new bone editor? This is known to work in 1cmnbod1 but other models might have wrong IDs.", "", MessageBoxButtons.YesNo);
				if(CustomBones == DialogResult.Yes)
				{
					try
					{
						Int32 StartID = 0x0;
						Int32 EndID = 0x0;

						for(Int32 x = 0; x < fileBytes.Count; x++)
						{
							if(fileBytes[x] == 't' && fileBytes[x + 1] == 'r' && fileBytes[x + 2] == 'a' && fileBytes[x + 3] == 'l' && fileBytes[x + 4] == 'l')
							{
								StartID = x - 9;
								x = fileBytes.Count;
							}
						}

						for(Int32 x = StartID + 1; x < fileBytes.Count; x++)
						{
							if(fileBytes[x] == 'c' && fileBytes[x + 1] == 'e' && fileBytes[x + 2] == 'l' && fileBytes[x + 3] == 's' && fileBytes[x + 4] == 'h' && fileBytes[x + 5] == 'a' && fileBytes[x + 6] == 'd' && fileBytes[x + 7] == 'e')
							{
								EndID = x;
								x = fileBytes.Count;
							}
						}

						UnityEngine.Debug.Log(StartID);
						UnityEngine.Debug.Log(EndID);

						List<string> Lines = new List<string>();
						List<byte> bytesInFile2 = new List<byte>();

						for(int x = StartID; x < EndID; x++)
						{
							bytesInFile2.Add(fileBytes[x]);
						}

						for(int x = 0; x < bytesInFile2.Count; x++)
						{
							if(bytesInFile2[x] == 0x00)
							{
								bytesInFile2[x] = 0x0A;
							}
						}
						string tx = Encoding.ASCII.GetString(bytesInFile2.ToArray());
						Lines = tx.Split('\n').ToList();
						bytesInFile2.Clear();
						Lines.RemoveAt(1);
						Lines.RemoveAt(Lines.Count - 1);

						for(int x = 0; x < Lines.Count; x++)
						{	
							Lines[x] = Lines[x].Remove(0, 4);
						}

						BoneList = Lines;
					}
					catch(Exception e)
					{
						MessageBox.Show("The tool wasn't able to parse the bone IDs. Changing to manual mode.");
						BoneList = new List<string>();
					}
					//File.WriteAllLines("C:\\Users\\santi\\Desktop\\file.txt", Lines.ToArray());
				}
				else
				{
					BoneList = new List<string>();
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
		if(indexOfMeshes.Count > 0)
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

			int VertexLength = 0;
			int SizeOfNDP3 = 0;
			int SizeFirst = 0;
			int SizeTriangles = 0;
			int SizeTextureCoords = 0;
			int SizeVertices = 0;

			SizeOfNDP3 = 
				fileBytes[indexOfMeshes[SelectedIndex] + 4] * 0x1000000 + 
				fileBytes[indexOfMeshes[SelectedIndex] + 5] * 0x10000 + 
				fileBytes[indexOfMeshes[SelectedIndex] + 6] * 0x100 + 
				fileBytes[indexOfMeshes[SelectedIndex] + 7];

			List<byte> meshBytes = new List<byte>();

			for(int x = indexOfMeshes[SelectedIndex]; x < indexOfMeshes[SelectedIndex] + SizeOfNDP3; x++)
			{
				meshBytes.Add(fileBytes[x]);
			}

			VertexLength = GameObject.Find("Mesh List Content").transform.Find("List Item " + SelectedIndex.ToString()).GetComponent<ListItem_Properties>().VertexLength;
			int GroupCount_ = GameObject.Find("Mesh List Content").transform.Find("List Item " + SelectedIndex.ToString()).GetComponent<ListItem_Properties>().MeshGroups;
			SizeHeader = 48;

			SizeFirst = 
				meshBytes[16] * 0x1000000 + 
				meshBytes[17] * 0x10000 + 
				meshBytes[18] * 0x100 + 
				meshBytes[19];
			
			SizeTriangles = 
				meshBytes[20] * 0x1000000 + 
				meshBytes[21] * 0x10000 + 
				meshBytes[22] * 0x100 + 
				meshBytes[23];

			if(VertexLength == 0x40)
			{
				SizeTextureCoords = 
					meshBytes[24] * 0x1000000 + 
					meshBytes[25] * 0x10000 + 
					meshBytes[26] * 0x100 + 
					meshBytes[27];

				SizeVertices = 
					meshBytes[28] * 0x1000000 + 
					meshBytes[29] * 0x10000 + 
					meshBytes[30] * 0x100 + 
					meshBytes[31];
			}
			else
			{
				SizeVertices =
					meshBytes[24] * 0x1000000 + 
					meshBytes[25] * 0x10000 +
					meshBytes[26] * 0x100 + 
					meshBytes[27];
			}

			// GENERATE TRIANGLE FILE
			for(int x = 0; x < SizeTriangles; x++)
			{
				triangleBytes.Add(meshBytes[SizeHeader + SizeFirst + x]);
			}

			// CHECK VERTEX LENGTH
			if(VertexLength == 0x40)
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
			else if(VertexLength == 0x1C || VertexLength == 0x20)
			{
				// GENERATE VERTICES FILE
				for(int x = 0; x < SizeVertices; x++)
				{
					vertexBytes.Add(meshBytes[SizeHeader + SizeFirst + SizeTriangles + x]);
				}
			}

			DialogResult dial2 = MessageBox.Show("1.7 has fixed the way of saving weights. However, you can still use the old mode if you want.\n\nDo you want to use the old weight saving method?", "Warning", MessageBoxButtons.YesNo);

			if(dial2 == DialogResult.Yes)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().WeightMode = 1;
			}

			if(stageMode == true)
			{
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().stageMode = true;
			}
			int a_ = indexOfMeshes[SelectedIndex];
			GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().OpenModelFromXfbin(VertexLength, fileBytes.ToArray(), meshBytes.ToArray(), a_, triangleBytes.ToArray(), textureBytes.ToArray(), vertexBytes.ToArray(), GroupCount_, BoneList);
			Destroy(GameObject.Find("Welcome Screen"));
		}
		else
		{
			System.Windows.Forms.MessageBox.Show("No .xfbin loaded.");
		}
	}

	public void OpenRawModel(int stage)
	{
		try
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.ShowDialog();

			if(openFileDialog1.FileName == "" || File.Exists(openFileDialog1.FileName) == false)
			{
				MessageBox.Show("Please select a valid file.");
				return;
			}

			string ModelPath = openFileDialog1.FileName;
			openFileDialog1 = new OpenFileDialog();
			openFileDialog1.ShowDialog();

			if(openFileDialog1.FileName == "" || File.Exists(openFileDialog1.FileName) == false)
			{
				MessageBox.Show("Please select a valid file.");
				return;
			}

			string LengthPath = openFileDialog1.FileName;
			openFileDialog1 = new OpenFileDialog();
			openFileDialog1.ShowDialog();

			if(openFileDialog1.FileName == "" || File.Exists(openFileDialog1.FileName) == false)
			{
				MessageBox.Show("Please select a valid file.");
				//return;
			}

			string TrianglePath = openFileDialog1.FileName;

			int VertexLength = int.Parse(File.ReadAllText(LengthPath));
			byte[] vertexBytes = File.ReadAllBytes(ModelPath);
			byte[] triangleBytes = new byte[0];
			byte[] textureCoordBytes = new byte[0];

			if(TrianglePath != "") triangleBytes = File.ReadAllBytes(TrianglePath);

			/*if(File.Exists(folderBrowserDialog1.SelectedPath + "\\modelTriangles.unsmf") == true)
			{
				textureCoordBytes = File.ReadAllBytes(folderBrowserDialog1.SelectedPath + "\\modelTextureCoords.unsmf");
			}*/

			if(stage == 1)
			{
				stageMode = true;
			}

			DialogResult Mode_ = MessageBox.Show("Set mode to \'CUSTOM\'?", "", MessageBoxButtons.YesNo);

			if(Mode_ == DialogResult.No) GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().OpenModelFromUnsmf(VertexLength, triangleBytes, textureCoordBytes, vertexBytes, stageMode, 0);
			else GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().OpenModelFromUnsmf(VertexLength, triangleBytes, textureCoordBytes, vertexBytes, stageMode, 20);

			Destroy(GameObject.Find("Welcome Screen"));
		}
		catch(Exception e)
		{
			stageMode = false;
			UnityEngine.Debug.Log(e.Message);
		}
	}

    //===================================================================================================================
    public void OpenUnsmfFolder(int stage)
    {
        try
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();

            int VertexLength = int.Parse(File.ReadAllText(folderBrowserDialog1.SelectedPath + "\\modelVertexLength.unsmf"));
            byte[] vertexBytes = File.ReadAllBytes(folderBrowserDialog1.SelectedPath + "\\modelVertices.unsmf");
            byte[] triangleBytes = new byte[0];
            byte[] textureCoordBytes = new byte[0];

            if (File.Exists(folderBrowserDialog1.SelectedPath + "\\modelTriangles.unsmf") == true)
            {
                triangleBytes = File.ReadAllBytes(folderBrowserDialog1.SelectedPath + "\\modelTriangles.unsmf");
            }

            if (File.Exists(folderBrowserDialog1.SelectedPath + "\\modelTriangles.unsmf") == true)
            {
                textureCoordBytes = File.ReadAllBytes(folderBrowserDialog1.SelectedPath + "\\modelTextureCoords.unsmf");
            }

            if (stage == 1)
            {
                stageMode = true;
            }

            if (folderBrowserDialog1.SelectedPath != "" && Directory.Exists(folderBrowserDialog1.SelectedPath))
            {
                GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().OpenModelFromUnsmf(VertexLength, triangleBytes, textureCoordBytes, vertexBytes, stageMode, 0);
                Destroy(GameObject.Find("Welcome Screen"));
            }
        }
        catch (Exception e)
        {
            stageMode = false;
            UnityEngine.Debug.Log(e.Message);
        }
    }

    //==================================================================
}