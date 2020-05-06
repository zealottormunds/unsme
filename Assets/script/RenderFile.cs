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

public class RenderFile : MonoBehaviour {

    public CameraMovement c;

	public bool inCommand = false;
	public bool fileOpen = false;
	public InputField CommandInput;

	public Text modelInformation;

	public int vertexOffset = 0;
	public int byteLength = 24;
	private bool was30 = false;
	[HideInInspector]
	public byte[] fileBytes;
	private bool FinishedDrawingModel = false;
	[HideInInspector]
	public int WeightMode = 0;
	// 0 for normal, 1 for YZ swapped

	private int InitialVertexCount = 0;
	public int VertexCount = 0;
	[HideInInspector]
	public List<GameObject> selectedVertex;
	[HideInInspector]
	public List<Vector3> vertexPosition = new List<Vector3>();

	[HideInInspector]
	public GameObject RenderedMesh;
	public MeshFilter mf;
	private MeshRenderer mr;

	[HideInInspector]
	public List<Vector3> meshVertices = new List<Vector3>();
	[HideInInspector]
	public List<int> meshTriangles = new List<int>();
	[HideInInspector]
	public List<Vector3> meshNormals = new List<Vector3>();
	[HideInInspector]
	public List<Vector2> TextureUVs = new List<Vector2>();
	[HideInInspector]
	public List<Vector4> vertexBone = new List<Vector4>();
	[HideInInspector]
	public List<Vector4> vertexWeight = new List<Vector4>();

	[HideInInspector]
	public string PathToModel;
	[HideInInspector]
	public int importmode = 0;
	[HideInInspector]
	public byte[] triangleFile;
	[HideInInspector]
	public byte[] textureMapFile;

	public bool WindowOpen = false;
	public GameObject toolBoxWindow;
	public GameObject window_boneEditor;
	public GameObject window_boneEditorOG;

	[HideInInspector]
	public byte[] OriginalXfbin;
	[HideInInspector]
	public byte[] OriginalNDP3;
	[HideInInspector]
	public int NDP3Index;

	[HideInInspector]
	public bool wasObjImported = false;
    [HideInInspector]
    public bool stageMode = false;
	[HideInInspector]
	public int textureType = 0;
    [HideInInspector]
	public int groupCount = 1;
    [HideInInspector]
    public int groupsInXfbin = 1;
	private List<Vector3> OriginalVertices = new List<Vector3>();

    [HideInInspector]
    public List<int> undo_action = new List<int>();
    [HideInInspector]
    public List<Vector3> undo_pos = new List<Vector3>();
    [HideInInspector]
    public List<List<int>> undo_sel = new List<List<int>>();

	public GameObject SaveGUI;
	private List<GameObject> OpenUIs = new List<GameObject>();
    [HideInInspector]
    public List<string> CustomBones = new List<string>();

	private bool endianess = false;

    void Start()
    {
        c = this.GetComponent<CameraMovement>();

		RenderedMesh = GameObject.Find("RENDERED MESH");
		mf = RenderedMesh.GetComponent<MeshFilter>();
		mr = RenderedMesh.GetComponent<MeshRenderer>();
		mf.mesh = new Mesh();
		mf.mesh.MarkDynamic();
    }

    public void OpenModelFromXfbin(int VertexLength_, byte[] XfbinBytes, byte[] NDP3Bytes, int NDP3Index_, byte[] TriangleFile, byte[] TextureFile, byte[] VertexFile, int groupCount_, List<string> BoneList)
    {
    	try
    	{
            if (fileOpen == false)
            {
                OriginalXfbin = XfbinBytes;
                OriginalNDP3 = NDP3Bytes;
                NDP3Index = NDP3Index_;

                fileOpen = true;
                selectedVertex.Clear();
                vertexPosition.Clear();

                groupsInXfbin = groupCount_;

                for (int z_ = 0; z_ < GameObject.Find("Model Data").transform.childCount; z_++)
                {
                    Destroy(GameObject.Find("Model Data").transform.Find(z_.ToString()));
                }

                meshVertices.Clear();
                meshTriangles.Clear();
                meshNormals.Clear();
                TextureUVs.Clear();
                vertexWeight.Clear();

                fileBytes = VertexFile;
                triangleFile = TriangleFile;
                textureMapFile = TextureFile;
                byteLength = VertexLength_;

                CustomBones = BoneList;

                int actualGroup = 0;
                List<int> GroupVertexCount = new List<int>();
                GroupSelection g = GetComponent<GroupSelection>();
                for (int x = 0; x < groupsInXfbin; x++)
                {
                    //int actualCount = (NDP3Bytes[0x6C + (x * 0x2E)] * 0x100) + (NDP3Bytes[0x6D + (x * 0x2E)] * 0x1);
                    int actualCount = (NDP3Bytes[0x6C + (x * 0x30)] * 0x100) + (NDP3Bytes[0x6D + (x * 0x30)] * 0x1);
                    GroupVertexCount.Add(actualCount);
                    g.GroupNames.Add("Group_" + x.ToString());
                    g.Groups.Add(new List<int>());
                    //MessageBox.Show(actualCount.ToString("X2"));
                    //g.TrianglesPerGroup.Add((NDP3Bytes[0x80 + (x * 0x30)] * 0x100) + (NDP3Bytes[0x81 + (x * 0x30)] * 0x1));
                    ConsoleMessage("\n<color=cyan>Group \"" + "Group_" + x.ToString() + "\" has been created. Select vertices and add them to the group with /addtogroup NAME.</color>");
                }

				for (int x = 0; x < (fileBytes.Length / byteLength); x++)
			    {
			    	List<byte> a = new List<byte>();
			    	// GET POSITIONS OF VERTICES   
					float vertexFloatX = BitConverter.ToSingle(BitConverter.GetBytes(
					fileBytes[0 + vertexOffset + (x * byteLength)] * 0x1000000 + 
					fileBytes[1 + vertexOffset + (x * byteLength)] * 0x10000 + 
					fileBytes[2 + vertexOffset + (x * byteLength)] * 0x100 + 
					fileBytes[3 + vertexOffset + (x * byteLength)]), 0);

					float vertexFloatY = BitConverter.ToSingle(BitConverter.GetBytes(
					fileBytes[4 + vertexOffset + (x * byteLength)] * 0x1000000 + 
					fileBytes[5 + vertexOffset + (x * byteLength)] * 0x10000 + 
					fileBytes[6 + vertexOffset + (x * byteLength)] * 0x100 + 
					fileBytes[7 + vertexOffset + (x * byteLength)]), 0);

					float vertexFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(
					fileBytes[8 + vertexOffset + (x * byteLength)] * 0x1000000 + 
					fileBytes[9 + vertexOffset + (x * byteLength)] * 0x10000 + 
					fileBytes[10 + vertexOffset + (x * byteLength)] * 0x100 + 
					fileBytes[11 + vertexOffset + (x * byteLength)]), 0);

					// IF STAGE MODE, THEN SCALE THE WHOLE MESH DIVIDING IT BY 20
					if(stageMode == true)
					{
						vertexFloatX = vertexFloatX / 20;
						vertexFloatZ = vertexFloatZ / 20;
						vertexFloatY = vertexFloatY / 20;
					}

					vertexPosition.Add(new Vector3(vertexFloatX, vertexFloatY, vertexFloatZ));

					if(byteLength == 0x40)
					{
						float normalFloatX = BitConverter.ToSingle(BitConverter.GetBytes(
							fileBytes[16 + vertexOffset + 0 + (x * byteLength)] * 0x1000000 + 
							fileBytes[16 + vertexOffset + 1 + (x * byteLength)] * 0x10000 + 
							fileBytes[16 + vertexOffset + 2 + (x * byteLength)] * 0x100 + 
							fileBytes[16 + vertexOffset + 3 + (x * byteLength)]), 0);
						float normalFloatY = BitConverter.ToSingle(BitConverter.GetBytes(
							fileBytes[16 + vertexOffset + 4 + (x * byteLength)] * 0x1000000 + 
							fileBytes[16 + vertexOffset + 5 + (x * byteLength)] * 0x10000 + 
							fileBytes[16 + vertexOffset + 6 + (x * byteLength)] * 0x100 + 
							fileBytes[16 + vertexOffset + 7 + (x * byteLength)]), 0);
						float normalFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(
							fileBytes[16 + vertexOffset + 8 + (x * byteLength)] * 0x1000000 +
							fileBytes[16 + vertexOffset + 9 + (x * byteLength)] * 0x10000 + 
							fileBytes[16 + vertexOffset + 10 + (x * byteLength)] * 0x100 + 
							fileBytes[16 + vertexOffset + 11 + (x * byteLength)]), 0);
						meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));

						vertexBone.Add(new Vector4(
                            (float)fileBytes[35 + vertexOffset + (x * byteLength)], 
                            (float)fileBytes[39 + vertexOffset + (x * byteLength)], 
                            (float)fileBytes[43 + vertexOffset + (x * byteLength)],
                            (float)fileBytes[47 + vertexOffset + (x * byteLength)]));

						float weightFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 0 + (x * byteLength)] * 0x1000000 + fileBytes[48 + vertexOffset + 1 + (x * byteLength)] * 0x10000 + fileBytes[48 + vertexOffset + 2 + (x * byteLength)] * 0x100 + fileBytes[48 + vertexOffset + 3 + (x * byteLength)]), 0);
						float weightFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 4 + (x * byteLength)] * 0x1000000 + fileBytes[48 + vertexOffset + 5 + (x * byteLength)] * 0x10000 + fileBytes[48 + vertexOffset + 6 + (x * byteLength)] * 0x100 + fileBytes[48 + vertexOffset + 7 + (x * byteLength)]), 0);
						float weightFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 8 + (x * byteLength)] * 0x1000000 + fileBytes[48 + vertexOffset + 9 + (x * byteLength)] * 0x10000 + fileBytes[48 + vertexOffset + 10 + (x * byteLength)] * 0x100 + fileBytes[48 + vertexOffset + 11 + (x * byteLength)]), 0);
                        float weightFloatW = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 12 + (x * byteLength)] * 0x1000000 + fileBytes[48 + vertexOffset + 13 + (x * byteLength)] * 0x10000 + fileBytes[48 + vertexOffset + 14 + (x * byteLength)] * 0x100 + fileBytes[48 + vertexOffset + 15 + (x * byteLength)]), 0);

                        if (WeightMode == 0) vertexWeight.Add(new Vector4(weightFloatX, weightFloatY, weightFloatZ, weightFloatW));
						if(WeightMode == 1) vertexWeight.Add(new Vector4(weightFloatX, weightFloatZ, weightFloatY, weightFloatW));

						/*float Unknown =
							toFloat(fileBytes[32 + vertexOffset + (x * byteLength)] * 0x100 +
							fileBytes[33 + vertexOffset + (x * byteLength)]);

						UnknownValue.Add(Unknown);*/
					}
					else if(byteLength == 0x1C)
					{
						float normalFloatX = toFloat(fileBytes[12 + (x * byteLength)] * 0x100 + fileBytes[13 + (x * byteLength)]);
						float normalFloatY = toFloat(fileBytes[14 + (x * byteLength)] * 0x100 + fileBytes[15 + (x * byteLength)]);
						float normalFloatZ = toFloat(fileBytes[16 + (x * byteLength)] * 0x100 + fileBytes[17 + (x * byteLength)]);
						meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));
					}
					else if(byteLength == 0x20)
					{
						float normalFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[0xC + vertexOffset + 0 + (x * byteLength)] * 0x1000000 + fileBytes[0xC + vertexOffset + 1 + (x * byteLength)] * 0x10000 + fileBytes[0xC + vertexOffset + 2 + (x * byteLength)] * 0x100 + fileBytes[0xC + vertexOffset + 3 + (x * byteLength)]), 0);
						float normalFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[0xC + vertexOffset + 4 + (x * byteLength)] * 0x1000000 + fileBytes[0xC + vertexOffset + 5 + (x * byteLength)] * 0x10000 + fileBytes[0xC + vertexOffset + 6 + (x * byteLength)] * 0x100 + fileBytes[0xC + vertexOffset + 7 + (x * byteLength)]), 0);
						float normalFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[0xC + vertexOffset + 8 + (x * byteLength)] * 0x1000000 + fileBytes[0xC + vertexOffset + 9 + (x * byteLength)] * 0x10000 + fileBytes[0xC + vertexOffset + 10 + (x * byteLength)] * 0x100 + fileBytes[0xC + vertexOffset + 11 + (x * byteLength)]), 0);
						meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));
					}

					GameObject actualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
					meshVertices.Add(vertexPosition[x]);

					actualObject.AddComponent<VertexObject>();
					actualObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
					actualObject.transform.position = vertexPosition[x];
					actualObject.name = x.ToString();
					actualObject.transform.SetParent(GameObject.Find("Model Data").transform);
					actualObject.transform.SetAsLastSibling();
					actualObject.tag = "Vertex";
					actualObject.layer = 9;

                    if(InitialVertexCount >= GroupVertexCount[actualGroup])
                    {
                        InitialVertexCount = 0;
                        actualGroup++;
                    }

                    g.AddToGroupByVertexID(actualGroup, x);

                    InitialVertexCount++;
			    }

				mf.mesh.vertices = vertexPosition.ToArray();

				OriginalVertices = vertexPosition;
				InitialVertexCount = vertexPosition.Count;

				VertexCount = GameObject.Find("Model Data").transform.childCount;

				if(triangleFile.Length > 1)
				{
					if(groupsInXfbin == 1)
					{
						int[] num = new int[3];
						int a = 0;
						int q = 0;

						byte[] newTriFi = triangleFile;

						int triangleGroup = 0;
						List<int> offsets = new List<int>();
						offsets.Add(0);

						for(int u = 0; u < newTriFi.Length; u = u + 2)
						{
							if(newTriFi[u] == 0xFF && newTriFi[u + 1] == 0xFF)
							{
								offsets.Add(u + 2);
								triangleGroup++;
							}
						}

						for(int u = 0; u <= triangleGroup; u++)
						{
							int numberOfTriangles = 0;
							if(u < triangleGroup) numberOfTriangles = ((offsets[u + 1] - offsets[u] - 6) / 2);
							if(u == triangleGroup) numberOfTriangles = ((newTriFi.Length - offsets[u] - 6) / 2);

							List<Vector3> Triangles_ = new List<Vector3>();
							Vector3 tri = new Vector3();

							for(int y = 0; y < numberOfTriangles; y++)
							{
								tri.x = (newTriFi[offsets[u] + (y * 2) + 0] * 0x100) + (newTriFi[offsets[u] + (y * 2) + 1]);
								tri.y = (newTriFi[offsets[u] + (y * 2) + 2] * 0x100) + (newTriFi[offsets[u] + (y * 2) + 3]);
								tri.z = (newTriFi[offsets[u] + (y * 2) + 4] * 0x100) + (newTriFi[offsets[u] + (y * 2) + 5]);

								Triangles_.Add(tri);
							}

							bool invert = false;

							for(int h = 0; h < Triangles_.Count; h++)
							{
								if(invert)
								{
									meshTriangles.Add((int)Triangles_[h].z);
									meshTriangles.Add((int)Triangles_[h].y);
									meshTriangles.Add((int)Triangles_[h].x);
								}
								else
								{
									meshTriangles.Add((int)Triangles_[h].x);
									meshTriangles.Add((int)Triangles_[h].y);
									meshTriangles.Add((int)Triangles_[h].z);

								}

								invert = !invert;
							}
						}
						mf.mesh.triangles = meshTriangles.ToArray();
					}
					else
					{
                        List<int> VerticesPerGroup = new List<int>();
						List<int> TrianglesPerGroup = new List<int>();
						for(int x = 0; x < groupsInXfbin; x++)
						{
							int vertices_ = NDP3Bytes[0x6C + (0x30 * x)] * 0x100 + NDP3Bytes[0x6D + (0x30 * x)];
							VerticesPerGroup.Add(vertices_);

							int triangles_ = NDP3Bytes[0x80 + (0x30 * x)] * 0x100 + NDP3Bytes[0x81 + (0x30 * x)];
							TrianglesPerGroup.Add(triangles_ * 2);
						}

                        int totalVertsOfPrev = 0;
						int prevTriangles = 0;

						for(int x = 0; x < groupsInXfbin; x++)
						{
                            g.TrianglesPerGroup.Add(0);

                            int verticesofprev = 0;
							if(x != 0)
							{
								verticesofprev = VerticesPerGroup[x - 1];
							}
							totalVertsOfPrev = totalVertsOfPrev + verticesofprev;

							int startIndexOfTriangleFile = 0;
							if(x != 0)
							{
								startIndexOfTriangleFile = TrianglesPerGroup[x - 1];
							}
							prevTriangles = prevTriangles + startIndexOfTriangleFile;
							startIndexOfTriangleFile = prevTriangles;

							List<byte> SubSectionOfTriangleFile = new List<byte>();

							for(int i = 0; i < TrianglesPerGroup[x]; i++)
							{
								SubSectionOfTriangleFile.Add(triangleFile[startIndexOfTriangleFile + i]);
							}

							byte[] newTriFi = SubSectionOfTriangleFile.ToArray();

							int[] num = new int[3];
							int a = 0;
							int q = 0;

							int triangleGroup = 0;
							List<int> offsets = new List<int>();
							offsets.Add(0);

							for(int u = 0; u < newTriFi.Length; u = u + 2)
							{
								if(newTriFi[u] == 0xFF && newTriFi[u + 1] == 0xFF)
								{
									offsets.Add(u + 2);
									triangleGroup++;
								}
							}

							for(int u = 0; u <= triangleGroup; u++)
							{
								int numberOfTriangles = 0;
								if(u < triangleGroup) numberOfTriangles = ((offsets[u + 1] - offsets[u] - 6) / 2);
								if(u == triangleGroup) numberOfTriangles = ((newTriFi.Length - offsets[u] - 6) / 2);

								List<Vector3> Triangles_ = new List<Vector3>();
								Vector3 tri = new Vector3();

								for(int y = 0; y < numberOfTriangles; y++)
								{
									tri.x = (newTriFi[offsets[u] + (y * 2) + 0] * 0x100) + (newTriFi[offsets[u] + (y * 2) + 1]) + totalVertsOfPrev;
									tri.y = (newTriFi[offsets[u] + (y * 2) + 2] * 0x100) + (newTriFi[offsets[u] + (y * 2) + 3]) + totalVertsOfPrev;
									tri.z = (newTriFi[offsets[u] + (y * 2) + 4] * 0x100) + (newTriFi[offsets[u] + (y * 2) + 5]) + totalVertsOfPrev;

									Triangles_.Add(tri);
								}

								bool invert = true;
                                if (x % 1 == 0) invert = false;

								for(int h = 0; h < Triangles_.Count; h++)
								{
									if(invert)
									{
										meshTriangles.Add((int)Triangles_[h].z);
										meshTriangles.Add((int)Triangles_[h].y);
										meshTriangles.Add((int)Triangles_[h].x);
									}
									else
									{
										meshTriangles.Add((int)Triangles_[h].x);
										meshTriangles.Add((int)Triangles_[h].y);
										meshTriangles.Add((int)Triangles_[h].z);
									}
                                    g.TrianglesPerGroup[x] = g.TrianglesPerGroup[x] + 1;

                                    invert = !invert;
								}
							}
						}
						mf.mesh.triangles = meshTriangles.ToArray();
					}
				}

				if(textureMapFile.Length / 0x8 == vertexPosition.Count)
				{
					textureType = 0;
				}
				else if(textureMapFile.Length / 0xC == vertexPosition.Count)
				{
					textureType = 1;
				}

				if(byteLength == 0x40)
				{
					if(textureMapFile.Length > 1)
					{
						if(textureType == 0)
						{
							for(int x = 0; x < VertexCount; x++)
							{
								float x_ = toFloat(textureMapFile[0x4 + (0x8 * x)] * 0x100 + textureMapFile[0x5 + (0x8 * x)]);
								float y_ = toFloat(textureMapFile[0x6 + (0x8 * x)] * 0x100 + textureMapFile[0x7 + (0x8 * x)]);

								TextureUVs.Add(new Vector2(x_, y_));
							}
						}
						else if(textureType == 1)
						{
							for(int x = 0; x < VertexCount; x++)
							{
								float x_ = toFloat(textureMapFile[0x4 + (0xC * x)] * 0x100 + textureMapFile[0x5 + (0xC * x)]);
								float y_ = toFloat(textureMapFile[0x6 + (0xC * x)] * 0x100 + textureMapFile[0x7 + (0xC * x)]);

								TextureUVs.Add(new Vector2(x_, y_));
							}
						}
					}
				}
				else if(byteLength == 0x1C)
				{
					for(int x = 0; x < VertexCount; x++)
					{
						float x_ = toFloat(fileBytes[(x * 0x1C) + 0x18] * 0x100 + fileBytes[(x * 0x1C) + 0x19]);
						float y_ = toFloat(fileBytes[(x * 0x1C) + 0x1A] * 0x100 + fileBytes[(x * 0x1C) + 0x1B]);

						TextureUVs.Add(new Vector2(x_, y_));
					}
				}
				else if(byteLength == 0x20)
				{
					for(int x = 0; x < VertexCount; x++)
					{
						float x_ = toFloat(fileBytes[(x * 0x20) + 0x18] * 0x100 + fileBytes[(x * 0x20) + 0x19]);
						float y_ = toFloat(fileBytes[(x * 0x20) + 0x1A] * 0x100 + fileBytes[(x * 0x20) + 0x1B]);

						TextureUVs.Add(new Vector2(x_, y_));
					}
				}

				DialogResult result_ = MessageBox.Show("Do you want to load a .png texture?", "Texture loading", MessageBoxButtons.YesNo);
				if(result_ == DialogResult.Yes)
				{
					////ConsoleMessage(" <color=cyan>TEXTURE IMAGE LOADED.</color>");
					OpenFileDialog openFileDialog2 = new OpenFileDialog();
					openFileDialog2.DefaultExt = "png";
					openFileDialog2.ShowDialog();

					if(openFileDialog2.FileName != "" && File.Exists(openFileDialog2.FileName))
					{
						try
						{
							byte[] textureBytes = File.ReadAllBytes(openFileDialog2.FileName);
							Texture2D extTexture = new Texture2D(1024, 1024);
							extTexture.LoadImage(textureBytes);
                            RenderedMesh.GetComponent<Renderer>().material.mainTexture = extTexture;
                            RenderedMesh.GetComponent<RenderMaterial>().Materials_[0] = RenderedMesh.GetComponent<Renderer>().material;
						}
						catch(Exception)
						{
							ConsoleMessage("\n<color=orange>Error loading texture.</color>");
						}
					}
					else
					{
						RenderedMesh.GetComponent<Renderer>().material = RenderedMesh.GetComponent<RenderMaterial>().Materials_[1];
                    }
				}
				else
				{
					//ConsoleMessage(" <color=red>TEXTURE IMAGE NOT FOUND.</color>");
					RenderedMesh.GetComponent<Renderer>().material = RenderedMesh.GetComponent<RenderMaterial>().Materials_[1];
				}

				mf.mesh.uv = TextureUVs.ToArray();
				mf.mesh.normals = meshNormals.ToArray();

				RenderedMesh.GetComponent<MeshCollider>().sharedMesh = mf.mesh;

                groupCount = groupsInXfbin;

		        FinishedDrawingModel = true;
		       	fileOpen = true;
				ConsoleMessage(" <color=lime>MODEL LOADED.</color>");
            }
		}
		catch(Exception e)
		{
			MessageBox.Show(e.ToString());
		}
    }

	bool movingVert = false;
	Vector3 tempVert = new Vector3();
	Vector3 finalVert = new Vector3();

	void FixedUpdate()
	{
		if(c.canMove && inCommand == false && WindowOpen == false)
		{
			if(fileOpen)
			{
				if((Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.Z)) || (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl)))
				{
					Undo();
				}
			}

			if(movingVert == false && (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.L)))
			{
				movingVert = true;
				undo_action.Add(0);
				undo_pos.Add(new Vector3(0, 0, 0));
				undo_sel.Add(new List<int>());

				tempVert = selectedVertex[0].transform.position;

				for(int x = 0; x < selectedVertex.Count; x++)
				{
					undo_sel[undo_sel.Count - 1].Add(int.Parse(selectedVertex[x].name));
				}
			}

			if(movingVert && (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.O) || Input.GetKeyUp(KeyCode.L)))
			{
				movingVert = false;
				finalVert = selectedVertex[0].transform.position;
				Vector3 transformation = new Vector3(finalVert.x - tempVert.x, finalVert.y - tempVert.y, finalVert.z - tempVert.z);
				undo_pos[undo_pos.Count - 1] = transformation;
			}

			if(movingVert && Input.GetKey(KeyCode.RightArrow))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position + c.mainCamera.transform.right / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}

			if(Input.GetKey(KeyCode.LeftArrow))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position - c.mainCamera.transform.right / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.UpArrow))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position + c.mainCamera.transform.forward / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.DownArrow))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position - c.mainCamera.transform.forward / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.O))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position + Vector3.up / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.L))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position - Vector3.up / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}

			modelInformation.text = selectedVertex.ToArray().Length.ToString() + " vertex selected";
		}
	}

	public void InvertNormals()
	{
		List<Vector3> newNormals = mf.mesh.normals.ToList();

		for(int x = 0; x < mf.mesh.normals.Length; x++)
		{
			newNormals[x] = new Vector3(mf.mesh.normals[x].x * -1, mf.mesh.normals[x].y * -1, mf.mesh.normals[x].z * -1);
		}

		mf.mesh.SetNormals(newNormals);
		ConsoleMessage("\n<color=cyan>Mesh normals inverted.</color>");
	}

	public void InvertTriangles()
	{
		for(int x = 0; x < meshTriangles.Count / 3; x++)
		{
			int tri0 = meshTriangles[(x * 3) + 0];
			int tri1 = meshTriangles[(x * 3) + 1];
			int tri2 = meshTriangles[(x * 3) + 2];

			meshTriangles[(x * 3) + 0] = tri2;
			meshTriangles[(x * 3) + 1] = tri1;
			meshTriangles[(x * 3) + 2] = tri0;
		}

		mf.mesh.triangles = meshTriangles.ToArray();
		ConsoleMessage("\n<color=cyan>Mesh triangles inverted.</color>");
	}

	public void UnselectVertex()
	{
		if(fileOpen == true)
		{
			for(int x = 0; x < selectedVertex.ToArray().Length; x++)
			{
				selectedVertex[x].GetComponent<Renderer>().material.color = Color.white;
				selectedVertex[x].GetComponent<VertexObject>().Selected = false;
			}
			selectedVertex.Clear();
			ConsoleMessage("\n<color=cyan>Unselected all vertices.</color>");
		}
		else
		{
			ConsoleMessage("\n<color=red>You need to open a model first.</color>");
		}
	}

	public void OpenSaveWindow(bool mode)
	{
		if(mode) 
		{
			OpenUIs.Add(SaveGUI);
			SaveGUI.SetActive(true);
			WindowOpen = true;
		}
		else
		{
			OpenUIs.Remove(SaveGUI);
			SaveGUI.SetActive(false);
			if(OpenUIs.Count == 0)
	    	{
	    		WindowOpen = false;
	    	}
		}
	}

	public void SaveXfbin()
	{
		SaveFileDialog saveFileDialog1 = new SaveFileDialog();
		saveFileDialog1.DefaultExt = ".xfbin";

		saveFileDialog1.ShowDialog();

		if(saveFileDialog1.FileName != "")
		{
			SaveModelToXfbin(saveFileDialog1.FileName);
		}
	}

	public void SaveModelToXfbin(string path__, bool saveShit = false)
	{
        if(groupsInXfbin > groupCount)
        {
            MessageBox.Show("This .xfbin needs " + groupsInXfbin.ToString() + " groups, while your imported mesh has only " + groupCount.ToString() + ".\nIn order to save this mesh, import a model with the same number of groups as the original.");
            return;
        }

		try
		{
			Transform ModelDataTransform;
			ModelDataTransform = GameObject.Find("Model Data").GetComponent<Transform>();

			List<byte> triangleFileNew = new List<byte>();
			List<byte> vertexFileNew = new List<byte>();
			List<byte> textureFileNew = new List<byte>();

            DialogResult dial;
            //dial = DialogResult.Yes;
            dial = MessageBox.Show("Fix vertex count and face count? If you used /importobj and your new mesh has more/less vertices than the original, then this is recommended.", "Warning", MessageBoxButtons.YesNo);

			meshNormals = mf.mesh.normals.ToList();
			meshTriangles = mf.mesh.triangles.ToList();
			meshVertices = mf.mesh.vertices.ToList();

			// Copy the old vertex list
			if(saveShit || GameObject.Find("Save Vertices").GetComponent<Toggle>().isOn == true)
			{
				for(int x = 0; x < VertexCount; x++)
				{
					if(x < InitialVertexCount)
					{
						for(int z = 0; z < byteLength; z++)
						{
							vertexFileNew.Add(fileBytes[z + (x * byteLength)]);
						}
					}
					else
					{
						for(int z = 0; z < byteLength; z++)
						{
							vertexFileNew.Add(fileBytes[z]);
						}
					}
				}

				for(int x = 0; x < VertexCount; x++)
				{
					byte[] posx = BitConverter.GetBytes(mf.mesh.vertices[x].x).ToArray();
					if(stageMode == true) posx = BitConverter.GetBytes(mf.mesh.vertices[x].x * 20).ToArray();
					Array.Reverse(posx);
					vertexFileNew[0x0 + (byteLength * x)] = posx[0];
					vertexFileNew[0x1 + (byteLength * x)] = posx[1];
					vertexFileNew[0x2 + (byteLength * x)] = posx[2];
					vertexFileNew[0x3 + (byteLength * x)] = posx[3];

					byte[] posz = BitConverter.GetBytes(mf.mesh.vertices[x].y).ToArray();
					if(stageMode == true) posz = BitConverter.GetBytes(mf.mesh.vertices[x].y * 20).ToArray();
					Array.Reverse(posz);
					vertexFileNew[0x4 + (byteLength * x)] = posz[0];
					vertexFileNew[0x5 + (byteLength * x)] = posz[1];
					vertexFileNew[0x6 + (byteLength * x)] = posz[2];
					vertexFileNew[0x7 + (byteLength * x)] = posz[3];

					byte[] posy = BitConverter.GetBytes(mf.mesh.vertices[x].z).ToArray();
					if(stageMode == true) posy = BitConverter.GetBytes(mf.mesh.vertices[x].z * 20).ToArray();
					Array.Reverse(posy);
					vertexFileNew[0x8 + (byteLength * x)] = posy[0];
					vertexFileNew[0x9 + (byteLength * x)] = posy[1];
					vertexFileNew[0xA + (byteLength * x)] = posy[2];
					vertexFileNew[0xB + (byteLength * x)] = posy[3];
	
					if(byteLength == 0x1C)
					{
						byte[] NXA = ToInt(meshNormals[x].x);
						Array.Reverse(NXA);

						byte[] NYA = ToInt(meshNormals[x].y);
						Array.Reverse(NYA);

						byte[] NZA = ToInt(meshNormals[x].z);
						Array.Reverse(NZA);

						vertexFileNew[0xC + (byteLength * x)] = NXA[0];
						vertexFileNew[0xD + (byteLength * x)] = NXA[1];

						vertexFileNew[0xE + (byteLength * x)] = NYA[0];
						vertexFileNew[0xF + (byteLength * x)] = NYA[1];

						vertexFileNew[0x10 + (byteLength * x)] = NZA[0];
						vertexFileNew[0x11 + (byteLength * x)] = NZA[1];
					}
					else if(byteLength == 0x40)
					{
						// BONE DATA
						vertexFileNew[0x23 + (byteLength * x)] = (byte)vertexBone[x].x;
						vertexFileNew[0x27 + (byteLength * x)] = (byte)vertexBone[x].y;
						vertexFileNew[0x2B + (byteLength * x)] = (byte)vertexBone[x].z;
                        vertexFileNew[0x2F + (byteLength * x)] = (byte)vertexBone[x].w;

						// WEIGHT DATA
						byte[] weightx = BitConverter.GetBytes(vertexWeight[x].x).ToArray();
                        Array.Reverse(weightx);

						byte[] weighty = BitConverter.GetBytes(vertexWeight[x].y).ToArray();
						if(WeightMode == 1) weighty = BitConverter.GetBytes(vertexWeight[x].z).ToArray();
						Array.Reverse(weighty);

						byte[] weightz = BitConverter.GetBytes(vertexWeight[x].z).ToArray();
						if(WeightMode == 1) weightz = BitConverter.GetBytes(vertexWeight[x].y).ToArray();
						Array.Reverse(weightz);

                        byte[] weightw = BitConverter.GetBytes(vertexWeight[x].w).ToArray();
                        Array.Reverse(weightw);

                        vertexFileNew[0x30 + (byteLength * x)] = weightx[0];
						vertexFileNew[0x31 + (byteLength * x)] = weightx[1];
						vertexFileNew[0x32 + (byteLength * x)] = weightx[2];
						vertexFileNew[0x33 + (byteLength * x)] = weightx[3];

						vertexFileNew[0x34 + (byteLength * x)] = weighty[0];
						vertexFileNew[0x35 + (byteLength * x)] = weighty[1];
						vertexFileNew[0x36 + (byteLength * x)] = weighty[2];
						vertexFileNew[0x37 + (byteLength * x)] = weighty[3];

						vertexFileNew[0x38 + (byteLength * x)] = weightz[0];
						vertexFileNew[0x39 + (byteLength * x)] = weightz[1];
						vertexFileNew[0x3A + (byteLength * x)] = weightz[2];
						vertexFileNew[0x3B + (byteLength * x)] = weightz[3];

                        vertexFileNew[0x3C + (byteLength * x)] = weightw[0];
                        vertexFileNew[0x3D + (byteLength * x)] = weightw[1];
                        vertexFileNew[0x3E + (byteLength * x)] = weightw[2];
                        vertexFileNew[0x3F + (byteLength * x)] = weightw[3];

                        // NORMALS
                        byte[] normalx = BitConverter.GetBytes(meshNormals[x].x).ToArray();
						Array.Reverse(normalx);

						vertexFileNew[0x10 + (byteLength * x)] = normalx[0];
						vertexFileNew[0x11 + (byteLength * x)] = normalx[1];
						vertexFileNew[0x12 + (byteLength * x)] = normalx[2];
						vertexFileNew[0x13 + (byteLength * x)] = normalx[3];

						byte[] normaly = BitConverter.GetBytes(meshNormals[x].y).ToArray();
						Array.Reverse(normaly);

						vertexFileNew[0x14 + (byteLength * x)] = normaly[0];
						vertexFileNew[0x15 + (byteLength * x)] = normaly[1];
						vertexFileNew[0x16 + (byteLength * x)] = normaly[2];
						vertexFileNew[0x17 + (byteLength * x)] = normaly[3];

						byte[] normalz = BitConverter.GetBytes(meshNormals[x].z).ToArray();
						Array.Reverse(normalz);

						vertexFileNew[0x18 + (byteLength * x)] = normalz[0];
						vertexFileNew[0x19 + (byteLength * x)] = normalz[1];
						vertexFileNew[0x1A + (byteLength * x)] = normalz[2];
						vertexFileNew[0x1B + (byteLength * x)] = normalz[3];
					}
					else if(byteLength == 0x20)
					{
						// NORMALS
						byte[] normalx = BitConverter.GetBytes(meshNormals[x].x).ToArray();
						Array.Reverse(normalx);

						vertexFileNew[0xC + (byteLength * x)] = normalx[0];
						vertexFileNew[0xD + (byteLength * x)] = normalx[1];
						vertexFileNew[0xE + (byteLength * x)] = normalx[2];
						vertexFileNew[0xF + (byteLength * x)] = normalx[3];

						byte[] normaly = BitConverter.GetBytes(meshNormals[x].y).ToArray();
						Array.Reverse(normaly);

						vertexFileNew[0x10 + (byteLength * x)] = normaly[0];
						vertexFileNew[0x11 + (byteLength * x)] = normaly[1];
						vertexFileNew[0x12 + (byteLength * x)] = normaly[2];
						vertexFileNew[0x13 + (byteLength * x)] = normaly[3];

						byte[] normalz = BitConverter.GetBytes(meshNormals[x].z).ToArray();
						Array.Reverse(normalz);

						vertexFileNew[0x14 + (byteLength * x)] = normalz[0];
						vertexFileNew[0x15 + (byteLength * x)] = normalz[1];
						vertexFileNew[0x16 + (byteLength * x)] = normalz[2];
						vertexFileNew[0x17 + (byteLength * x)] = normalz[3];
					}
				}
			}
			else
			{
				for(int x = 0; x < fileBytes.Length; x++)
				{
					vertexFileNew.Add(fileBytes[x]);
				}
			}

			if(saveShit || GameObject.Find("Save UV").GetComponent<Toggle>().isOn == true)
			{
				if((saveShit || GameObject.Find("Save Vertices").GetComponent<Toggle>().isOn) || GameObject.Find("Model Data").transform.childCount <= VertexCount)
				{
					for(int x = 0; x < TextureUVs.Count; x++)
					{
						if(byteLength == 0x1C)
						{
							byte[] UVXA = ToInt(TextureUVs[x].x);
							Array.Reverse(UVXA);

							byte[] UVYA = ToInt(TextureUVs[x].y);
							Array.Reverse(UVYA);

							vertexFileNew[0x18 + (byteLength * x)] = UVXA[0];
							vertexFileNew[0x19 + (byteLength * x)] = UVXA[1];

							vertexFileNew[0x1A + (byteLength * x)] = UVYA[0];
							vertexFileNew[0x1B + (byteLength * x)] = UVYA[1];
						}
						else if(byteLength == 0x20)
						{
							byte[] UVXA = ToInt(TextureUVs[x].x);
							Array.Reverse(UVXA);

							byte[] UVYA = ToInt(TextureUVs[x].y);
							Array.Reverse(UVYA);

							vertexFileNew[0x18 + (byteLength * x)] = UVXA[0];
							vertexFileNew[0x19 + (byteLength * x)] = UVXA[1];
							vertexFileNew[0x1A + (byteLength * x)] = UVYA[0];
							vertexFileNew[0x1B + (byteLength * x)] = UVYA[1];

							vertexFileNew[0x1C + (byteLength * x)] = 0;
							vertexFileNew[0x1D + (byteLength * x)] = 0;
							vertexFileNew[0x1E + (byteLength * x)] = 0;
							vertexFileNew[0x1F + (byteLength * x)] = 0;
						}
						else if(byteLength == 0x40)
						{
							if(textureType == 0)
							{
								byte[] UVXA = ToInt(TextureUVs[x].x);
								Array.Reverse(UVXA);

								byte[] UVYA = ToInt(TextureUVs[x].y);
								Array.Reverse(UVYA);

								textureFileNew.Add(0xFF);
								textureFileNew.Add(0xFF);
								textureFileNew.Add(0xFF);
								textureFileNew.Add(0xFF);	

								textureFileNew.Add(UVXA[0]);
								textureFileNew.Add(UVXA[1]);
								textureFileNew.Add(UVYA[0]);
								textureFileNew.Add(UVYA[1]);
							}
							else if(textureType == 1)
							{
								byte[] UVXA = ToInt(TextureUVs[x].x);
								Array.Reverse(UVXA);

								byte[] UVYA = ToInt(TextureUVs[x].y);
								Array.Reverse(UVYA);

								textureFileNew.Add(0xFF);
								textureFileNew.Add(0xFF);
								textureFileNew.Add(0xFF);
								textureFileNew.Add(0xFF);	

								textureFileNew.Add(UVXA[0]);
								textureFileNew.Add(UVXA[1]);
								textureFileNew.Add(UVYA[0]);
								textureFileNew.Add(UVYA[1]);

								textureFileNew.Add(UVXA[0]);
								textureFileNew.Add(UVXA[1]);
								textureFileNew.Add(UVYA[0]);
								textureFileNew.Add(UVYA[1]);
							}
						}
					}
				}
				else
				{
					MessageBox.Show("The new texture UV can't be saved because there's more UVs than vertex count. The count has to be the same. Fix this by saving vertices.");
				}
			}
			else
			{
				if(byteLength == 0x40)
				{
					for(int x = 0; x < textureMapFile.Length; x++)
					{
						textureFileNew.Add(textureMapFile[x]);
					}
				}
			}

            List<int> endOfTriangles = new List<int>();
            if (saveShit || GameObject.Find("Save Triangles").GetComponent<Toggle>().isOn == true)
			{
                GroupSelection g = GetComponent<GroupSelection>();
                int remove = 0;
                int actualGroup_ = 0;
                int prevTriangles = g.TrianglesPerGroup[actualGroup_];

                int TriangleTotal = 0;
                for(int x = 0; x < groupsInXfbin; x++)
                {
                    TriangleTotal = TriangleTotal + g.TrianglesPerGroup[x];
                    //MessageBox.Show("G" + x.ToString() + ": " + g.TrianglesPerGroup[x].ToString("X2"));
                }

                TriangleTotal = mf.mesh.triangles.Length / 3;

                for (int q = 0; q < TriangleTotal; q++)
                {
                    byte[] tri3 = BitConverter.GetBytes(mf.mesh.triangles[(q * 3) + 0] - remove).ToArray();
                    byte[] tri2 = BitConverter.GetBytes(mf.mesh.triangles[(q * 3) + 1] - remove).ToArray();
                    byte[] tri1 = BitConverter.GetBytes(mf.mesh.triangles[(q * 3) + 2] - remove).ToArray();

                    triangleFileNew.Add(tri3[1]);
                    triangleFileNew.Add(tri3[0]);
                    triangleFileNew.Add(tri2[1]);
                    triangleFileNew.Add(tri2[0]);
                    triangleFileNew.Add(tri1[1]);
                    triangleFileNew.Add(tri1[0]);

                    triangleFileNew.Add(0xFF);
                    triangleFileNew.Add(0xFF);

                    if(q >= prevTriangles)
                    {
                        //MessageBox.Show("Nos pasamos pavo");
                        endOfTriangles.Add(triangleFileNew.Count);
                        //MessageBox.Show("Added end at " + triangleFileNew.Count.ToString("X2"));
                        remove = remove + g.Groups[actualGroup_].Count;
                        actualGroup_++;
                        prevTriangles = prevTriangles + g.TrianglesPerGroup[actualGroup_];
                    }
                }
                endOfTriangles.Add(triangleFileNew.Count);
            }
			else
			{
				for(int x = 0; x < triangleFile.Length; x++)
				{
					triangleFileNew.Add(triangleFile[x]);
				}
			}

			int OriginalNDP3Size = 
				OriginalNDP3[0x4] * 0x1000000 + 
				OriginalNDP3[0x5] * 0x10000 + 
				OriginalNDP3[0x6] * 0x100 + 
				OriginalNDP3[0x7];

			int SizeBeforeNDP3Index = 0;
			int sizeMode = 0;

			int x_ = 0;
			while(OriginalXfbin[NDP3Index - 4 + x_] * 0x1000000 + OriginalXfbin[NDP3Index - 3 + x_] * 0x10000 + OriginalXfbin[NDP3Index - 2 + x_] * 0x100 + OriginalXfbin[NDP3Index - 1 + x_] != OriginalNDP3Size)
			{
				x_--;
			}

			SizeBeforeNDP3Index = x_ - 4;

			List<byte> newNDP3File = new List<byte>();
			for(int x = 0; x < 0x10; x++)
			{
				newNDP3File.Add(OriginalNDP3[x]);
			}

			//Add first section size
			for(int x = 0; x < 4; x++)
			{
				newNDP3File.Add(OriginalNDP3[0x10 + x]);
			}
			int firstSectionSize_ = newNDP3File[16] * 0x1000000 + newNDP3File[17] * 0x10000 + newNDP3File[18] * 0x100 + newNDP3File[19];

			int TriangleSectionSize = triangleFileNew.Count;

			if(TriangleSectionSize < 0x100)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(TriangleSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(TriangleSectionSize >= 0x100 && TriangleSectionSize < 0x10000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(TriangleSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[1]);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(TriangleSectionSize >= 0x10000 && TriangleSectionSize < 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(TriangleSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[2]);
				newNDP3File.Add(arrayOfSize[1]);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(TriangleSectionSize >= 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(TriangleSectionSize);
				newNDP3File.Add(arrayOfSize[3]);
				newNDP3File.Add(arrayOfSize[2]);
				newNDP3File.Add(arrayOfSize[1]);
				newNDP3File.Add(arrayOfSize[0]);
			}

			//Check vertex length
			if(byteLength == 0x40)
			{
				//Add texture section size
				int TextureSectionSize = textureFileNew.Count;

				if(TextureSectionSize < 0x100)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(TextureSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(TextureSectionSize >= 0x100 && TextureSectionSize < 0x10000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(TextureSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(TextureSectionSize >= 0x10000 && TextureSectionSize < 0x1000000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(TextureSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[2]);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(TextureSectionSize >= 0x1000000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(TextureSectionSize);
					newNDP3File.Add(arrayOfSize[3]);
					newNDP3File.Add(arrayOfSize[2]);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}

				//Add vertex section size
				int VertexSectionSize = vertexFileNew.Count;

				if(VertexSectionSize < 0x100)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x100 && VertexSectionSize < 0x10000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x10000 && VertexSectionSize < 0x1000000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[2]);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x1000000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(arrayOfSize[3]);
					newNDP3File.Add(arrayOfSize[2]);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
			}
			else if(byteLength == 0x1C)
			{
				//Add vertex section size
				int VertexSectionSize = vertexFileNew.Count;

				if(VertexSectionSize < 0x100)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x100 && VertexSectionSize < 0x10000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x10000 && VertexSectionSize < 0x1000000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[2]);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x1000000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(arrayOfSize[3]);
					newNDP3File.Add(arrayOfSize[2]);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
			}
			else if(byteLength == 0x20)
			{
				//Add vertex section size
				int VertexSectionSize = vertexFileNew.Count;

				if(VertexSectionSize < 0x100)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x100 && VertexSectionSize < 0x10000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x10000 && VertexSectionSize < 0x1000000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(0);
					newNDP3File.Add(arrayOfSize[2]);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				else if(VertexSectionSize >= 0x1000000)
				{
					byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
					newNDP3File.Add(arrayOfSize[3]);
					newNDP3File.Add(arrayOfSize[2]);
					newNDP3File.Add(arrayOfSize[1]);
					newNDP3File.Add(arrayOfSize[0]);
				}
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
			}

			// Add rest of header
			for(int x = 0x20; x < 0x30; x++)
			{
				newNDP3File.Add(OriginalNDP3[x]);
			}

			// Add first section
			for(int x = 0; x < firstSectionSize_; x++)
			{
				newNDP3File.Add(OriginalNDP3[0x30 + x]);
			}

			// Add triangle section
			for(int x = 0; x < triangleFileNew.Count; x++)
			{
				newNDP3File.Add(triangleFileNew[x]);
			}

			if(byteLength == 0x40)
			{
				// Add texture section
				for(int x = 0; x < textureFileNew.Count; x++)
				{
					newNDP3File.Add(textureFileNew[x]);
				}
				// Add vertex section
				for(int x = 0; x < vertexFileNew.Count; x++)
				{
					newNDP3File.Add(vertexFileNew[x]);
				}
			}
			else if(byteLength == 0x1C || byteLength == 0x20)
			{
				// Add vertex section
				for(int x = 0; x < vertexFileNew.Count; x++)
				{
					newNDP3File.Add(vertexFileNew[x]);
				}
			}

			int newNDP3Size = newNDP3File.Count + OriginalNDP3Size - 48 - firstSectionSize_ - triangleFile.Length - textureMapFile.Length - fileBytes.Length;

			List<byte> newXfbinFile = new List<byte>();

			// Copy old .xfbin file until ndp3
			for(int x = 0; x < NDP3Index; x++)
			{
				newXfbinFile.Add(OriginalXfbin[x]);
			}

			// Add new size before NDP3
			if(newNDP3Size < 0x100)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

				newXfbinFile[NDP3Index + SizeBeforeNDP3Index] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 1] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 2] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x100 && newNDP3Size < 0x10000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

				newXfbinFile[NDP3Index + SizeBeforeNDP3Index] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 1] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 2] = arrayOfSize[1];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x10000 && newNDP3Size < 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

				newXfbinFile[NDP3Index + SizeBeforeNDP3Index] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 1] = arrayOfSize[2];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 2] = arrayOfSize[1];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

				newXfbinFile[NDP3Index + SizeBeforeNDP3Index] = arrayOfSize[3];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 1] = arrayOfSize[2];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 2] = arrayOfSize[1];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
			}

			// Copy new NDP3
			for(int x = 0; x < newNDP3File.Count; x++)
			{
				newXfbinFile.Add(newNDP3File[x]);
			}

			// Fix new NDP3 size
			if(newNDP3Size < 0x100)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 4] = 0;
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 5] = 0;
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 6] = 0;
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 7] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x100 && newNDP3Size < 0x10000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 4] = 0;
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 5] = 0;
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 6] = arrayOfSize[1];
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 7] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x10000 && newNDP3Size < 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 4] = 0;
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 5] = arrayOfSize[2];
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 6] = arrayOfSize[1];
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 7] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 4] = arrayOfSize[3];
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 5] = arrayOfSize[2];
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 6] = arrayOfSize[1];
				newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 7] = arrayOfSize[0];
			}

			// Copy original name and rest of xfbin 
			for(int x = NDP3Index + (0x30 + firstSectionSize_ + triangleFile.Length + textureMapFile.Length + fileBytes.Length); x < OriginalXfbin.Length; x++)
			{
				newXfbinFile.Add(OriginalXfbin[x]);
			}

			// Fix new NDP3 size 36
			int DifferenceBetweenSizes = 
			(OriginalXfbin[NDP3Index + SizeBeforeNDP3Index - 0x24] * 0x1000000 + 
			OriginalXfbin[NDP3Index + SizeBeforeNDP3Index - 0x23] * 0x10000 + 
			OriginalXfbin[NDP3Index + SizeBeforeNDP3Index - 0x22] * 0x100 + 
			OriginalXfbin[NDP3Index + SizeBeforeNDP3Index - 0x21]) - OriginalNDP3Size;

			if(newNDP3Size + DifferenceBetweenSizes < 0x100)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x24] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x23] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x22] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x21] = arrayOfSize[0];
			}
			else if(newNDP3Size + DifferenceBetweenSizes >= 0x100 && newNDP3Size + DifferenceBetweenSizes < 0x10000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x24] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x23] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x22] = arrayOfSize[1];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x21] = arrayOfSize[0];
			}
			else if(newNDP3Size + DifferenceBetweenSizes >= 0x10000 && newNDP3Size + DifferenceBetweenSizes < 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x24] = 0;
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x23] = arrayOfSize[2];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x22] = arrayOfSize[1];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x21] = arrayOfSize[0];
			}
			else if(newNDP3Size + DifferenceBetweenSizes >= 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x24] = arrayOfSize[3];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x23] = arrayOfSize[2];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x22] = arrayOfSize[1];
				newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 0x21] = arrayOfSize[0];
			}

			// FIX 6C AND 80
			if(dial == DialogResult.Yes)
			{
				if(groupsInXfbin == 1)
				{
					//Fix 6C
					Int32 vertexCountTemp = VertexCount;

					if(vertexCountTemp <= 0x100)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(vertexCountTemp);
						//newXfbinFile[NDP3Index + 0x6A] = 0;
						//newXfbinFile[NDP3Index + 0x6B] = 0;
						newXfbinFile[NDP3Index + 0x6C] = 0;
						newXfbinFile[NDP3Index + 0x6D] = arrayOfSize[0];
					}
					else if(vertexCountTemp > 0x100 && vertexCountTemp <= 0x10000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(vertexCountTemp);
						//newXfbinFile[NDP3Index + 0x6A] = 0;
						//newXfbinFile[NDP3Index + 0x6B] = 0;
						newXfbinFile[NDP3Index + 0x6C] = arrayOfSize[1];
						newXfbinFile[NDP3Index + 0x6D] = arrayOfSize[0];
					}
					else if(vertexCountTemp > 0x10000 && vertexCountTemp <= 0x1000000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(vertexCountTemp);
						//newXfbinFile[NDP3Index + 0x6A] = 0;
						//newXfbinFile[NDP3Index + 0x6B] = arrayOfSize[2];
						newXfbinFile[NDP3Index + 0x6C] = arrayOfSize[1];
						newXfbinFile[NDP3Index + 0x6D] = arrayOfSize[0];
					}
					else if(vertexCountTemp > 0x1000000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(vertexCountTemp);
						//newXfbinFile[NDP3Index + 0x6A] = arrayOfSize[3];
						//newXfbinFile[NDP3Index + 0x6B] = arrayOfSize[2];
						newXfbinFile[NDP3Index + 0x6C] = arrayOfSize[1];
						newXfbinFile[NDP3Index + 0x6D] = arrayOfSize[0];
					}

					//Fix 80
					if(triangleFileNew.ToArray().Length / 2 <= 0x100)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(triangleFileNew.ToArray().Length / 2);
						//newXfbinFile[NDP3Index + 0x7E] = 0;
						//newXfbinFile[NDP3Index + 0x7F] = 0;
						newXfbinFile[NDP3Index + 0x80] = 0;
						newXfbinFile[NDP3Index + 0x81] = arrayOfSize[0];
					}
					else if(triangleFileNew.ToArray().Length / 2 > 0x100 && triangleFileNew.ToArray().Length / 2 <= 0x10000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(triangleFileNew.ToArray().Length / 2);
						//newXfbinFile[NDP3Index + 0x7E] = 0;
						//newXfbinFile[NDP3Index + 0x7F] = 0;
						newXfbinFile[NDP3Index + 0x80] = arrayOfSize[1];
						newXfbinFile[NDP3Index + 0x81] = arrayOfSize[0];
					}
					else if(triangleFileNew.ToArray().Length / 2 > 0x10000 && triangleFileNew.ToArray().Length / 2 <= 0x1000000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(triangleFileNew.ToArray().Length / 2);
						//newXfbinFile[NDP3Index + 0x7E] = 0;
						//newXfbinFile[NDP3Index + 0x7F] = arrayOfSize[2];
						newXfbinFile[NDP3Index + 0x80] = arrayOfSize[1];
						newXfbinFile[NDP3Index + 0x81] = arrayOfSize[0];
					}
					else
					{
						byte[] arrayOfSize = BitConverter.GetBytes(triangleFileNew.ToArray().Length / 2);
						//newXfbinFile[NDP3Index + 0x7E] = arrayOfSize[3];
						//newXfbinFile[NDP3Index + 0x7F] = arrayOfSize[2];
						newXfbinFile[NDP3Index + 0x80] = arrayOfSize[1];
						newXfbinFile[NDP3Index + 0x81] = arrayOfSize[0];
					}
				}
				else
				{
                    //Fix triangle and vertex count in each group
                    GroupSelection g = GetComponent<GroupSelection>();
                    int prevVertexCount = 0;
                    int prevTriSize = 0;
                    int prevTextureLength = 0;
                    for (int actualGroup = 0; actualGroup < groupsInXfbin; actualGroup++)
                    {
                        //int previousVertexCount = 0;
                        //if (actualGroup > 0) previousVertexCount = g.Groups[actualGroup - 1].Count;
                        //byte[] correctVertexSectionStart = BitConverter.GetBytes(byteLength * previousVertexCount);
                        if (actualGroup > 0)
                        {
                            int vertCount = (g.Groups[actualGroup - 1].Count * byteLength) + prevVertexCount;
                            //MessageBox.Show("byteLength = " + byteLength.ToString("X2") + ", " + vertCount.ToString("X2"));
                            prevVertexCount = vertCount;
                            byte[] correctVertexSectionStart = BitConverter.GetBytes(vertCount);
                            newXfbinFile[NDP3Index + 0x68 + (actualGroup * 0x30)] = correctVertexSectionStart[3];
                            newXfbinFile[NDP3Index + 0x69 + (actualGroup * 0x30)] = correctVertexSectionStart[2];
                            newXfbinFile[NDP3Index + 0x6A + (actualGroup * 0x30)] = correctVertexSectionStart[1];
                            newXfbinFile[NDP3Index + 0x6B + (actualGroup * 0x30)] = correctVertexSectionStart[0];
                        }

                        byte[] correctVertCount = BitConverter.GetBytes(g.Groups[actualGroup].Count);
                        newXfbinFile[NDP3Index + 0x6C + (actualGroup * 0x30)] = correctVertCount[1];
                        newXfbinFile[NDP3Index + 0x6D + (actualGroup * 0x30)] = correctVertCount[0];

                        // FIX TRIANGLE COUNT XD
                        /*int remove = 0;
                        if (actualGroup != 0) remove = endOfTriangles[actualGroup - 1];
                        int triCount = (endOfTriangles[actualGroup] - remove) / 2;
                        byte[] correctTriCount = BitConverter.GetBytes(triCount);*/

                        int remove = 0;
                        if (actualGroup != 0) remove = endOfTriangles[actualGroup - 1];
                        int triCount = (endOfTriangles[actualGroup] - remove) / 2;
                        byte[] correctTriCount = BitConverter.GetBytes(triCount);

                        //byte[] correctTriCount = BitConverter.GetBytes((g.TrianglesPerGroup[actualGroup]));
                        //int actualtris = g.TrianglesPerGroup[actualGroup];
                        //newXfbinFile[NDP3Index + 0x7E + (actualGroup * 0x30)] = correctTriCount[3];
                        //newXfbinFile[NDP3Index + 0x7F + (actualGroup * 0x30)] = correctTriCount[2];
                        newXfbinFile[NDP3Index + 0x80 + (actualGroup * 0x30)] = correctTriCount[1];
                        newXfbinFile[NDP3Index + 0x81 + (actualGroup * 0x30)] = correctTriCount[0];

                        if(actualGroup < groupsInXfbin - 1)
                        {
                            //int previous = (g.TrianglesPerGroup[actualGroup] * 4) + prevTriSize;
                            int previous = endOfTriangles[actualGroup] + prevTriSize;
                            //prevTriSize = previous;
                            byte[] correctTriangleSectionSize = BitConverter.GetBytes(previous);
                            //byte[] correctTriangleSectionSize = BitConverter.GetBytes((8 * g.TrianglesPerGroup[actualGroup]) + ((groupsInXfbin - 1 - actualGroup) * 8));
                            newXfbinFile[NDP3Index + 0x90 + (actualGroup * 0x30)] = correctTriangleSectionSize[3];
                            newXfbinFile[NDP3Index + 0x91 + (actualGroup * 0x30)] = correctTriangleSectionSize[2];
                            newXfbinFile[NDP3Index + 0x92 + (actualGroup * 0x30)] = correctTriangleSectionSize[1];
                            newXfbinFile[NDP3Index + 0x93 + (actualGroup * 0x30)] = correctTriangleSectionSize[0];
                        }

                        if (actualGroup < groupsInXfbin - 1)
                        {
                            //jajaja
                            int previous = ((8 + (4 * textureType)) * g.Groups[actualGroup].Count) + prevTextureLength;
                            prevTextureLength = previous;
                            byte[] correctTextureSectionSize = BitConverter.GetBytes(previous);
                            //int textureLength = 8 + (4 * textureType);
                            //byte[] correctTextureSectionSize = BitConverter.GetBytes(textureLength * g.Groups[actualGroup].Count);
                            newXfbinFile[NDP3Index + 0x94 + (actualGroup * 0x30)] = correctTextureSectionSize[3];
                            newXfbinFile[NDP3Index + 0x95 + (actualGroup * 0x30)] = correctTextureSectionSize[2];
                            newXfbinFile[NDP3Index + 0x96 + (actualGroup * 0x30)] = correctTextureSectionSize[1];
                            newXfbinFile[NDP3Index + 0x97 + (actualGroup * 0x30)] = correctTextureSectionSize[0];
                        }

                    }
				}
			}

			meshNormals = mf.mesh.normals.ToList();
			meshTriangles = mf.mesh.triangles.ToList();
			meshVertices = mf.mesh.vertices.ToList();

			File.WriteAllBytes(path__, newXfbinFile.ToArray());

			MessageBox.Show(".xfbin file saved.");
		}
		catch(Exception exception)
		{
			MessageBox.Show(exception.ToString());
		}
	}

	public void OpenToolBox()
	{
		if(fileOpen == true)
		{
			OpenUIs.Add(toolBoxWindow);
			toolBoxWindow.SetActive(true);
			WindowOpen = true;
			ConsoleMessage("\n" + "<color=cyan>Tool box open.</color>");
		}
		else
		{
			ConsoleMessage("\n<color=red>You need to open a model first.</color>");
		}
	}

	public void CloseToolBox()
	{
		OpenUIs.Remove(toolBoxWindow);
		toolBoxWindow.SetActive(false);

    	if(OpenUIs.Count == 0)
    	{
    		WindowOpen = false;
    	}
	}

	public void DoCommand()
	{
		GetComponent<ConsoleCommandBehaviour>().ConsoleBehaviour(CommandInput.text);
		CommandInput.text = "";
	}

	public void ConsoleMessage(string message_)
	{
		GameObject.Find("Console").GetComponentInChildren<Text>().text = GameObject.Find("Console").GetComponentInChildren<Text>().text + message_;
	}

	/*public void ImportBones(string FilePath)
	{
		try
		{
			DialogResult di = MessageBox.Show("Do you want to clean the previous list of bones?", "Bone importing", MessageBoxButtons.YesNo);

			if(di == DialogResult.Yes)
			{
				vertexBone.Clear();
				vertexWeight.Clear();
			}

			string[] boneLines = File.ReadAllLines(FilePath);

			List<Vector4> BoneIndexObj = new List<Vector4>();
			List<Vector4> BoneWeightObj = new List<Vector4>();

			for(int h = 0; h < boneLines.ToList().Count / 6; h++)
			{
				int[] bones_ = new int[3];
				bones_[0] = int.Parse((boneLines[(6 * h) + 0]));
				bones_[1] = int.Parse((boneLines[(6 * h) + 1]));
				bones_[2] = int.Parse((boneLines[(6 * h) + 2]));

				float[] weights_ = new float[3];
				weights_[0] = float.Parse((boneLines[(6 * h) + 3]));
				weights_[1] = float.Parse((boneLines[(6 * h) + 4]));
				weights_[2] = float.Parse((boneLines[(6 * h) + 5]));

				BoneIndexObj.Add(new Vector3(bones_[0], bones_[1], bones_[2]));
				BoneWeightObj.Add(new Vector3(weights_[0], weights_[1], weights_[2]));
			}

			for(int x = 0; x < VertexCount; x++)
			{
				vertexBone.Add(BoneIndexObj[x]);
				vertexWeight.Add(BoneWeightObj[x]);
			}

			if(VertexCount > vertexBone.Count && byteLength == 0x40)
			{
				MessageBox.Show("Some vertices were filled with bones of ID 0.");
				int boneC = vertexBone.Count;
				for(int x = boneC; x < VertexCount; x++)
				{
					vertexBone.Add(new Vector3(0, 0, 0));
					vertexWeight.Add(new Vector3(0, 0, 0));
				}
			}

			MessageBox.Show("Bones imported successfully.");
		}
		catch(Exception e)
		{
			MessageBox.Show("Error importing bones.\n\n" + e.Message);
		}
	}*/

	public void ImportModelPos(string importPath)
	{
		try
		{
			string[] objModelLines = File.ReadAllLines(importPath);
			List<Vector3> VerticesInObj = new List<Vector3>();

			// Read vert position and save it to a variable
			for(int x = 0; x < objModelLines.Length; x++)
			{
				string line_ = objModelLines[x];
				if(line_.Length > 2)
				{
					if(line_[0] == 'v' && line_[1] == ' ')
					{
						int a = 0;
						int a1 = 0;
						int b = 0;
						int c = 0;

						char[] xT = new char[25];
						char[] yT = new char[25];
						char[] zT = new char[25];

						char[] line;
						line = line_.ToCharArray();

						a = 2;
						while(line_[a] == ' ')
						{
							a++;
						}

						while(line[a].ToString() != " ")
						{
							xT[a1] = line[a];
							a1++;
							a++;
						}

						a++;
						while(line[a].ToString() != " ")
						{
							yT[b] = line[a];
							b++;
							a++;
						}

						a++;
						while(a < line.Length && line[a].ToString() != " ")
						{
							zT[c] = line[a];
							c++;
							a++;
						}

						float X_;
						float Y_;
						float Z_;

						X_ = float.Parse(new string(xT));
						Y_ = float.Parse(new string(yT));
						Z_ = -1 * float.Parse(new string(zT));

						VerticesInObj.Add(new Vector3(X_, Y_, Z_));
					}
				}
			}

			if(VerticesInObj.Count == vertexPosition.Count)
			{
				for(int x = 0; x < GameObject.Find("Model Data").transform.childCount; x++)
				{
					GameObject.Find("Model Data").transform.Find(x.ToString()).transform.position = VerticesInObj[x];
					vertexPosition[x] = VerticesInObj[x];
				}

				meshVertices = vertexPosition;

				mf.mesh.vertices = meshVertices.ToArray();
			}
			else
			{
				MessageBox.Show("This model file has more vertices than the original, and it can't be imported.");
			}
		}
		catch(Exception e)
		{
			MessageBox.Show(e.Message);
		}
	}

	public float GetUVX(int vertex)
	{
		return mf.mesh.uv[vertex].x;
	}

	public float GetUVY(int vertex)
	{
		return mf.mesh.uv[vertex].y;
	}

	public void ChangeBoneID(int vertex, int number, int boneID)
	{
        switch(number)
        {
            case 0:
                vertexBone[vertex] = new Vector4((float)boneID, vertexBone[vertex].y, vertexBone[vertex].z, vertexBone[vertex].w);
                break;
            case 1:
                vertexBone[vertex] = new Vector4(vertexBone[vertex].x, (float)boneID, vertexBone[vertex].z, vertexBone[vertex].w);
                break;
            case 2:
                vertexBone[vertex] = new Vector4(vertexBone[vertex].x, vertexBone[vertex].y, (float)boneID, vertexBone[vertex].w);
                break;
            case 3:
                vertexBone[vertex] = new Vector4(vertexBone[vertex].x, vertexBone[vertex].y, vertexBone[vertex].z, (float)boneID);
                break;
        }
	}

	public void ChangeWeight(int vertex, int number, float boneID)
	{
        switch (number)
        {
            case 0:
                vertexBone[vertex] = new Vector4((float)boneID, vertexBone[vertex].y, vertexBone[vertex].z, vertexBone[vertex].w);
                break;
            case 1:
                vertexBone[vertex] = new Vector4(vertexBone[vertex].x, (float)boneID, vertexBone[vertex].z, vertexBone[vertex].w);
                break;
            case 2:
                vertexBone[vertex] = new Vector4(vertexBone[vertex].x, vertexBone[vertex].y, (float)boneID, vertexBone[vertex].w);
                break;
            case 3:
                vertexBone[vertex] = new Vector4(vertexBone[vertex].x, vertexBone[vertex].y, vertexBone[vertex].z, (float)boneID);
                break;
        }
    }

	public Vector4 GetBonesOfVertex(int vertex)
	{
		return vertexBone[vertex];
	}

	public Vector4 GetWeightsOfVertex(int vertex)
	{
		return vertexWeight[vertex];
	}

	public void ScaleModel(float scalex, float scaley, float scalez, bool isUndo)
	{
		float xCenter = 0;
		float yCenter = 0;
		float zCenter = 0;

		for(int x = 0; x < selectedVertex.Count; x++)
		{
			GameObject vertexpoint = selectedVertex[x];
			xCenter = xCenter + vertexpoint.transform.position.x;
			yCenter = yCenter + vertexpoint.transform.position.y;
			zCenter = zCenter + vertexpoint.transform.position.z;
		}

		xCenter = xCenter / selectedVertex.ToArray().Length;
		yCenter = yCenter / selectedVertex.ToArray().Length;
		zCenter = zCenter / selectedVertex.ToArray().Length;

		Vector3 CenterPoint = new Vector3(xCenter, yCenter, zCenter);

		GameObject scaleFactor = new GameObject();
		scaleFactor.transform.position = CenterPoint;
		scaleFactor.transform.SetParent(GameObject.Find("Model Data").transform);

		for(int x = 0; x < selectedVertex.Count; x++)
		{
			GameObject vertexpoint = selectedVertex[x];
			vertexpoint.transform.SetParent(scaleFactor.transform);
		}

		scaleFactor.transform.localScale = new Vector3(scaleFactor.transform.localScale.x * scalex, scaleFactor.transform.localScale.y * scaley, scaleFactor.transform.localScale.z * scalez);

		for(int x = 0; x < selectedVertex.Count; x++)
		{
			GameObject vertexpoint = selectedVertex[x];
			meshVertices[int.Parse(vertexpoint.name)] = vertexpoint.transform.position;
			vertexpoint.transform.SetParent(GameObject.Find("Model Data").transform);
			vertexpoint.transform.localScale = (new Vector3(0.4f, 0.4f, 0.4f));
		}

		Destroy(scaleFactor);

		vertexPosition = meshVertices;
		mf.mesh.vertices = meshVertices.ToArray();
		if(isUndo == false) ConsoleMessage("\n" + "<color=lime>Scaled model in " + scalex.ToString() + " " + scaley.ToString() + " " + scalez.ToString() + ".</color>");
	}

	public void RotateModel(float rotatex, float rotatey, float rotatez, bool isUndo)
	{
		float xCenter = 0;
		float yCenter = 0;
		float zCenter = 0;

		for(int x = 0; x < selectedVertex.Count; x++)
		{
			GameObject vertexpoint = selectedVertex[x];
			xCenter = xCenter + vertexpoint.transform.position.x;
			yCenter = yCenter + vertexpoint.transform.position.y;
			zCenter = zCenter + vertexpoint.transform.position.z;
		}

		xCenter = xCenter / selectedVertex.ToArray().Length;
		yCenter = yCenter / selectedVertex.ToArray().Length;
		zCenter = zCenter / selectedVertex.ToArray().Length;

		Vector3 CenterPoint = new Vector3(xCenter, yCenter, zCenter);

		GameObject rotateFactor = new GameObject();
		rotateFactor.transform.position = CenterPoint;
		rotateFactor.transform.SetParent(GameObject.Find("Model Data").transform);

		for(int x = 0; x < selectedVertex.Count; x++)
		{
			GameObject vertexpoint = selectedVertex[x];
			vertexpoint.transform.SetParent(rotateFactor.transform);
		}

		rotateFactor.transform.rotation = Quaternion.Euler(rotateFactor.transform.rotation.x + rotatex, rotateFactor.transform.rotation.y + rotatey, rotateFactor.transform.rotation.z + rotatez);

		for(int x = 0; x < selectedVertex.Count; x++)
		{
			GameObject vertexpoint = selectedVertex[x];
			meshVertices[int.Parse(vertexpoint.name)] = vertexpoint.transform.position;
			vertexpoint.transform.SetParent(GameObject.Find("Model Data").transform);
			vertexpoint.transform.localScale = (new Vector3(0.4f, 0.4f, 0.4f));
		}

		Destroy(rotateFactor);

		vertexPosition = meshVertices;

		mf.mesh.vertices = meshVertices.ToArray();
		if(isUndo == false) ConsoleMessage("\n" + "<color=lime>Rotated model in " + rotatex.ToString() + " " + rotatey.ToString() + " " + rotatez.ToString() + ".</color>");
	}

	public static float toFloat( int hbits )
	{
	    int mant = hbits & 0x03ff;            // 10 bits mantissa
	    int exp =  hbits & 0x7c00;            // 5 bits exponent
	    if( exp == 0x7c00 )                   // NaN/Inf
	        exp = 0x3fc00;                    // -> NaN/Inf
	    else if( exp != 0 )                   // normalized value
	    {
	        exp += 0x1c000;                   // exp - 15 + 127
	        if( mant == 0 && exp > 0x1c400 )  // smooth transition
	            return BitConverter.ToSingle(BitConverter.GetBytes( ( hbits & 0x8000 ) << 16
	                                            | exp << 13 | 0x3ff ), 0);
	    }
	    else if( mant != 0 )                  // && exp==0 -> subnormal
	    {
	        exp = 0x1c400;                    // make it normal
	        do {
	            mant <<= 1;                   // mantissa * 2
	            exp -= 0x400;                 // decrease exp by 1
	        } while( ( mant & 0x400 ) == 0 ); // while not normal
	        mant &= 0x3ff;                    // discard subnormal bit
	    }                                     // else +/-0 -> +/-0
	    return BitConverter.ToSingle(BitConverter.GetBytes(          // combine all parts
	        ( hbits & 0x8000 ) << 16          // sign  << ( 31 - 15 )
	        | ( exp | mant ) << 13 ), 0);         // value << ( 23 - 10 )
	}

	private static byte[] I2B(int input)
	{
	    var bytes = BitConverter.GetBytes(input);
	    return new byte[] { bytes[0], bytes[1] };
	}

	public static byte[] ToInt(float twoByteFloat)
	{
	    int fbits = BitConverter.ToInt32(BitConverter.GetBytes(twoByteFloat), 0);
	    int sign = fbits >> 16 & 0x8000;
	    int val = (fbits & 0x7fffffff) + 0x1000;
	    if (val >= 0x47800000)
	    {
	        if ((fbits & 0x7fffffff) >= 0x47800000)
	        {
	            if (val < 0x7f800000) return I2B(sign | 0x7c00);
	            return I2B(sign | 0x7c00 | (fbits & 0x007fffff) >> 13);
	        }
	        return I2B(sign | 0x7bff);
	    }
	    if (val >= 0x38800000) return I2B(sign | val - 0x38000000 >> 13);
	    if (val < 0x33000000) return I2B(sign);
	    val = (fbits & 0x7fffffff) >> 23;
	    return I2B(sign | ((fbits & 0x7fffff | 0x800000) + (0x800000 >> val - 102) >> 126 - val));
	}

	public void ExportToObj(string path_obj)
	{
		Transform ModelDataTransform;
		ModelDataTransform = GameObject.Find("Model Data").GetComponent<Transform>();

		List<string> objModelLines = new List<string>();

		objModelLines.Add("# Obj exported by UNS4 Model Editor by Tormunds");
		objModelLines.Add("\n");

		for(int x = 0; x < VertexCount; x++)
		{
			objModelLines.Add("v " + (meshVertices[x].x * -1).ToString().Replace(',','.') + " " + meshVertices[x].y.ToString().Replace(',', '.') + " " + meshVertices[x].z.ToString().Replace(',', '.'));
		}
		objModelLines.Add("# " + VertexCount.ToString() + " vertices");

		objModelLines.Add("\n");

		for(int x = 0; x < mf.mesh.uv.Length; x++)
		{
			if(mf.mesh.uv.Length >= x + 1)
			{
				objModelLines.Add("vt " + mf.mesh.uv[x].x.ToString().Replace(',', '.') + " " + (mf.mesh.uv[x].y * -1).ToString().Replace(',', '.'));
			}
			else
			{
				objModelLines.Add("vt 0 0");
			}
		}
		objModelLines.Add("# " + VertexCount.ToString() + " texture coordinates");

		objModelLines.Add("\n");

		for(int x = 0; x < mf.mesh.normals.Length; x++)
		{
			objModelLines.Add("vn " + (mf.mesh.normals[x] * -1).x.ToString().Replace(',', '.') + " " + (mf.mesh.normals[x].y).ToString().Replace(',', '.') + " " + mf.mesh.normals[x].z.ToString().Replace(',', '.'));
		}

		objModelLines.Add("# " + VertexCount.ToString() + " normals");

		objModelLines.Add("\n");

		objModelLines.Add("g Mesh01");
		objModelLines.Add("s 1");
	
		int a = 0;
		int triNum = mf.mesh.triangles.Length / 3;

		while(a < mf.mesh.triangles.Length - 3)
		{
			objModelLines.Add("f " + 
			(mf.mesh.triangles[2 + a] + 1).ToString() + "/" + 
			(mf.mesh.triangles[2 + a] + 1).ToString() + "/" + 
			(mf.mesh.triangles[2 + a] + 1).ToString() + " " + 

			(mf.mesh.triangles[1 + a] + 1).ToString() + "/" + 
			(mf.mesh.triangles[1 + a] + 1).ToString() + "/" + 
			(mf.mesh.triangles[1 + a] + 1).ToString() + " " + 

			(mf.mesh.triangles[0 + a] + 1).ToString() + "/" + 
			(mf.mesh.triangles[0 + a] + 1).ToString() + "/" + 
			(mf.mesh.triangles[0 + a] + 1).ToString());
			a = a + 3;
		}
		objModelLines.Add("# " + (mf.mesh.triangles.Length / 3).ToString() + " triangles");

		try
		{
			File.WriteAllLines(path_obj, objModelLines.ToArray());
			ConsoleMessage("\n<color=yellow>Model exported correctly.</color>");
		}
		catch(Exception)
		{
			ConsoleMessage("\n<color=red>Error exporting to obj (is the path and filename right?)</color>");
		}
	}

	public void LoadTexture(string p = "")
	{
		/*OpenFileDialog openFileDialog1 = new OpenFileDialog();
        if (p == "") openFileDialog1.ShowDialog();
        else openFileDialog1.FileName = p;*/

		if(p != "" && File.Exists(p))
		{
			try
			{
				byte[] textureBytes = File.ReadAllBytes(p);
				Texture2D extTexture = new Texture2D(1024, 1024);
				extTexture.LoadImage(textureBytes);
				RenderedMesh.GetComponent<RenderMaterial>().Materials_[0].mainTexture = extTexture;
				RenderedMesh.GetComponent<Renderer>().material = RenderedMesh.GetComponent<RenderMaterial>().Materials_[0];
				ConsoleMessage("\n<color=cyan>Texture loaded.</color>");
			}
			catch(Exception)
			{
				ConsoleMessage("\n<color=orange>Error loading texture.</color>");
			}
		}
		else
		{
			ConsoleMessage("\n<color=orange>TEXTURE IMAGE NOT FOUND.</color>");
		}
	}

	public void OpenVertexBoneEditor()
	{
		if(byteLength == 64)
		{
			if(CustomBones.Count > 0)
			{
				OpenUIs.Add(window_boneEditor);
				window_boneEditor.SetActive(true);
				WindowOpen = true;
				window_boneEditor.GetComponent<BoneEditor>().EnableWindow();
			}
			else
			{
				OpenUIs.Add(window_boneEditorOG);
				window_boneEditorOG.SetActive(true);
				WindowOpen = true;
				window_boneEditorOG.GetComponent<w_VertexBoneEditor>().EnableWindow();
			}
		}
		else
		{
			ConsoleMessage("\nThis model doesn't have bone data.");
		}
	}

	public void CloseVertexBoneEditor()
	{
		if(CustomBones.Count > 0)
		{
			window_boneEditor.GetComponent<BoneEditor>().CloseWindow();
			OpenUIs.Remove(window_boneEditor);
			window_boneEditor.SetActive(false);
		}
		else
		{
			window_boneEditorOG.GetComponent<w_VertexBoneEditor>().CloseWindow();
			OpenUIs.Remove(window_boneEditorOG);
			window_boneEditorOG.SetActive(false);
		}
		if(OpenUIs.Count == 0)
    	{
    		WindowOpen = false;
    	}
	}

	public Mesh ReturnActualMesh()
	{
		return mf.mesh;
	}

	public void UpdateUV(Vector2[] list_uv)
	{
		mf.mesh.SetUVs(0, list_uv.ToList());
	}

	//================================================================================================================================

	public void SaveModel(string path__)
	{
		Transform ModelDataTransform;
		ModelDataTransform = GameObject.Find("Model Data").GetComponent<Transform>();

		List<byte> triangleFileNew = new List<byte>();
		List<byte> vertexFileNew = new List<byte>();
		List<byte> textureFileNew = new List<byte>();

		vertexFileNew = fileBytes.ToList();

		int test = vertexFileNew.ToArray().Length / byteLength;
		for(int x = 0; x < GameObject.Find("Model Data").transform.childCount - test; x++)
		{
			for(int z = 0; z < byteLength; z++)
			{
				vertexFileNew.Add(fileBytes[z]);
			}
		}

		for(int x = 0; x < GameObject.Find("Model Data").transform.childCount; x++)
		{
			byte[] posx = BitConverter.GetBytes(meshVertices[x].x / 150).ToArray();
			if(stageMode == true) posx = BitConverter.GetBytes(meshVertices[x].x * 20).ToArray();
			if(endianess == false) Array.Reverse(posx);

			vertexFileNew[0 + (byteLength * x)] = posx[0];
			vertexFileNew[1 + (byteLength * x)] = posx[1];
			vertexFileNew[2 + (byteLength * x)] = posx[2];
			vertexFileNew[3 + (byteLength * x)] = posx[3];

			byte[] posz = BitConverter.GetBytes(meshVertices[x].z / 150).ToArray();
			if(stageMode == true) posz = BitConverter.GetBytes(meshVertices[x].z * 20).ToArray();
			if(endianess == false) Array.Reverse(posz);
			vertexFileNew[4 + (byteLength * x)] = posz[0];
			vertexFileNew[5 + (byteLength * x)] = posz[1];
			vertexFileNew[6 + (byteLength * x)] = posz[2];
			vertexFileNew[7 + (byteLength * x)] = posz[3];

			byte[] posy = BitConverter.GetBytes(meshVertices[x].y / 150).ToArray();
			if(stageMode == true) posy = BitConverter.GetBytes(meshVertices[x].y * 20).ToArray();
			if(endianess == false) Array.Reverse(posy);
			vertexFileNew[8 + (byteLength * x)] = posy[0];
			vertexFileNew[9 + (byteLength * x)] = posy[1];
			vertexFileNew[10 + (byteLength * x)] = posy[2];
			vertexFileNew[11 + (byteLength * x)] = posy[3];

			if(byteLength == 28)
			{
				byte[] NXA = ToInt(mf.mesh.normals[x].x);
				Array.Reverse(NXA);

				byte[] NZA = ToInt(mf.mesh.normals[x].z);
				Array.Reverse(NZA);

				byte[] NYA = ToInt(mf.mesh.normals[x].y);
				Array.Reverse(NYA);

				vertexFileNew[12 + (byteLength * x)] = NXA[0];
				vertexFileNew[13 + (byteLength * x)] = NXA[1];

				vertexFileNew[14 + (byteLength * x)] = NZA[0];
				vertexFileNew[15 + (byteLength * x)] = NZA[1];

				vertexFileNew[16 + (byteLength * x)] = NYA[0];
				vertexFileNew[17 + (byteLength * x)] = NYA[1];
			}
			if(byteLength == 64)
			{
				// BONE DATA
				vertexFileNew[35 + (byteLength * x)] = (byte)vertexBone[x].x;
				vertexFileNew[39 + (byteLength * x)] = (byte)vertexBone[x].y;
				vertexFileNew[43 + (byteLength * x)] = (byte)vertexBone[x].z;
                vertexFileNew[47 + (byteLength * x)] = (byte)vertexBone[x].w;

				// NORMALS
				byte[] normalx = BitConverter.GetBytes(mf.mesh.normals[x].x).ToArray();
				Array.Reverse(normalx);

				vertexFileNew[16 + (byteLength * x)] = normalx[0];
				vertexFileNew[17 + (byteLength * x)] = normalx[1];
				vertexFileNew[18 + (byteLength * x)] = normalx[2];
				vertexFileNew[19 + (byteLength * x)] = normalx[3];

				byte[] normalz = BitConverter.GetBytes(mf.mesh.normals[x].z).ToArray();
				Array.Reverse(normalz);

				vertexFileNew[20 + (byteLength * x)] = normalz[0];
				vertexFileNew[21 + (byteLength * x)] = normalz[1];
				vertexFileNew[22 + (byteLength * x)] = normalz[2];
				vertexFileNew[23 + (byteLength * x)] = normalz[3];

				byte[] normaly = BitConverter.GetBytes(mf.mesh.normals[x].y).ToArray();
				Array.Reverse(normaly);

				vertexFileNew[24 + (byteLength * x)] = normaly[0];
				vertexFileNew[25 + (byteLength * x)] = normaly[1];
				vertexFileNew[26 + (byteLength * x)] = normaly[2];
				vertexFileNew[27 + (byteLength * x)] = normaly[3];
			}
			if(byteLength == 0x20)
			{
				// NORMALS
				byte[] normalx = BitConverter.GetBytes(mf.mesh.normals[x].x).ToArray();
				Array.Reverse(normalx);

				vertexFileNew[12 + (byteLength * x)] = normalx[0];
				vertexFileNew[13 + (byteLength * x)] = normalx[1];
				vertexFileNew[14 + (byteLength * x)] = normalx[2];
				vertexFileNew[15 + (byteLength * x)] = normalx[3];

				byte[] normalz = BitConverter.GetBytes(mf.mesh.normals[x].z).ToArray();
				Array.Reverse(normalz);

				vertexFileNew[16 + (byteLength * x)] = normalz[0];
				vertexFileNew[17 + (byteLength * x)] = normalz[1];
				vertexFileNew[18 + (byteLength * x)] = normalz[2];
				vertexFileNew[19 + (byteLength * x)] = normalz[3];

				byte[] normaly = BitConverter.GetBytes(mf.mesh.normals[x].y).ToArray();
				Array.Reverse(normaly);

				vertexFileNew[20 + (byteLength * x)] = normaly[0];
				vertexFileNew[21 + (byteLength * x)] = normaly[1];
				vertexFileNew[22 + (byteLength * x)] = normaly[2];
				vertexFileNew[23 + (byteLength * x)] = normaly[3];
			}
		}

		for(int x = 0; x < mf.mesh.uv.Length; x++)
		{
			if(byteLength == 28)
			{
				byte[] UVXA = ToInt(mf.mesh.uv[x].x);
				Array.Reverse(UVXA);

				byte[] UVYA = ToInt(mf.mesh.uv[x].y);
				Array.Reverse(UVYA);

				vertexFileNew[24 + (byteLength * x)] = UVXA[0];
				vertexFileNew[25 + (byteLength * x)] = UVXA[1];

				vertexFileNew[26 + (byteLength * x)] = UVYA[0];
				vertexFileNew[27 + (byteLength * x)] = UVYA[1];
			}
			else if(byteLength == 0x20)
			{
				byte[] UVXA = BitConverter.GetBytes(mf.mesh.uv[x].x);
				Array.Reverse(UVXA);

				byte[] UVYA = BitConverter.GetBytes(mf.mesh.uv[x].y);
				Array.Reverse(UVYA);

				vertexFileNew[0x18 + (byteLength * x)] = UVXA[0];
				vertexFileNew[0x19 + (byteLength * x)] = UVXA[1];
				vertexFileNew[0x1A + (byteLength * x)] = UVXA[2];
				vertexFileNew[0x1B + (byteLength * x)] = UVXA[3];

				vertexFileNew[0x1C + (byteLength * x)] = UVYA[0];
				vertexFileNew[0x1D + (byteLength * x)] = UVYA[1];
				vertexFileNew[0x1E + (byteLength * x)] = UVYA[2];
				vertexFileNew[0x1F + (byteLength * x)] = UVYA[3];
			}
			else if(byteLength == 64)
			{
				byte[] UVXA = ToInt(mf.mesh.uv[x].x);
				Array.Reverse(UVXA);

				byte[] UVYA = ToInt(mf.mesh.uv[x].y);
				Array.Reverse(UVYA);

				textureFileNew.Add(255);
				textureFileNew.Add(255);
				textureFileNew.Add(255);
				textureFileNew.Add(255);	

				textureFileNew.Add(UVXA[0]);
				textureFileNew.Add(UVXA[1]);

				textureFileNew.Add(UVYA[0]);
				textureFileNew.Add(UVYA[1]);
			}
		}

		for(int z = 0; z < triangleFile.Length; z++)
		{
			triangleFile[z] = 0;
		}

		for(int q = 0; q <= this.mf.mesh.triangles.Length - 3; q += 3)
		{
			byte[] tri1 = BitConverter.GetBytes(mf.mesh.triangles[q + 0]).ToArray();
			//if(endianess == true) tri1.Reverse();
			byte[] tri2 = BitConverter.GetBytes(mf.mesh.triangles[q + 1]).ToArray();
			//if(endianess == true) tri2.Reverse();
			byte[] tri3 = BitConverter.GetBytes(mf.mesh.triangles[q + 2]).ToArray();
			//if(endianess == true) tri3.Reverse();

			triangleFileNew.Add(tri3[0]);
			triangleFileNew.Add(tri3[1]);
			triangleFileNew.Add(tri2[0]);
			triangleFileNew.Add(tri2[1]);
			triangleFileNew.Add(tri1[0]);
			triangleFileNew.Add(tri1[1]);
		}

		try
		{
			if(Directory.Exists(path__) == false)
			{
				Directory.CreateDirectory(path__);
			}

			File.WriteAllBytes(path__ + "\\modelVertices.unsmf", vertexFileNew.ToArray());
			File.WriteAllText(path__ + "\\modelVertexLength.unsmf", byteLength.ToString());
			if(triangleFile.Length > 0)
			{
				File.WriteAllBytes(path__ + "\\modelTriangles.unsmf", triangleFileNew.ToArray());
			}
			if(byteLength == 64)
			{
				File.WriteAllBytes(path__ + "\\modelTextureCoords.unsmf", textureFileNew.ToArray());
			}

			ConsoleMessage("\n" + "<color=yellow>Saved files to \"" + path__ + "\"</color>");
		}
		catch(Exception)
		{
			ConsoleMessage("\n" + "<color=red>Error saving model.</color>");
		}
	}

	public void OpenModelFile(string path__) {
		if(fileOpen == false)
		{
			PathToModel = path__;
			if(File.Exists(PathToModel + "\\modelVertices.unsmf") && File.Exists(PathToModel + "\\modelVertexLength.unsmf"))
			{
				fileBytes = File.ReadAllBytes(PathToModel + "\\modelVertices.unsmf");
				byteLength = int.Parse(File.ReadAllText(PathToModel + "\\modelVertexLength.unsmf"));

				if(File.Exists(PathToModel + "\\modelTriangles.unsmf"))
				{
					triangleFile = File.ReadAllBytes(PathToModel + "\\modelTriangles.unsmf");
				}

				if(File.Exists(PathToModel + "\\modelTextureCoords.unsmf") && byteLength == 64)
				{
					textureMapFile = File.ReadAllBytes(PathToModel + "\\modelTextureCoords.unsmf");
				}
				//OpenModelFromXfbin(byteLength, triangleFile, textureMapFile, fileBytes);
	        }
	        else
	        {
				ConsoleMessage("\n" + "<color=red>TARGET MODEL NOT FOUND.</color>");
	        }
        }
        else
        {
			ConsoleMessage("\n" + "<color=red>Restart the program to open a new .unsmf file.</color>");
        }
	}

	public void OpenModelFromUnsmf(int VertexLength_, byte[] TriangleFile, byte[] TextureFile, byte[] VertexFile, bool stage, int mod)
    {
		if(fileOpen == false)
		{
			if(stage == true)
			{
				stageMode = true;
			}
			fileOpen = true;
			selectedVertex.Clear();
			vertexPosition.Clear();

			for(int z_ = 0; z_ < GameObject.Find("Model Data").transform.childCount; z_++)
			{
				Destroy(GameObject.Find("Model Data").transform.Find(z_.ToString()));
			}

			meshVertices.Clear();
			meshTriangles.Clear();
			meshNormals.Clear();
			TextureUVs.Clear();

			byteLength = VertexLength_;
			fileBytes = VertexFile;
			triangleFile = TriangleFile;
			textureMapFile = TextureFile;

			DialogResult asd = DialogResult.No;
			asd = MessageBox.Show("Do you want to invert the endianess?", "", MessageBoxButtons.YesNo);

			if(asd == DialogResult.Yes)
			{
				endianess = true;
			}

            MessageBox.Show("RE2 mode");

			for (int x = 0; x < (fileBytes.Length / byteLength); x++)
		    {
				float vertexFloatX = 0;
				float vertexFloatZ = 0;
				float vertexFloatY = 0;

		    	if(asd == DialogResult.No)
		    	{
					vertexFloatX = 80 * BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[0 + vertexOffset + (x * byteLength)] * 0x1000000 + fileBytes[1 + vertexOffset + (x * byteLength)] * 0x10000 + fileBytes[2 + vertexOffset + (x * byteLength)] * 0x100 + fileBytes[3 + vertexOffset + (x * byteLength)]), 0);
					vertexFloatZ = 80 * BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[4 + vertexOffset + (x * byteLength)] * 0x1000000 + fileBytes[5 + vertexOffset + (x * byteLength)] * 0x10000 + fileBytes[6 + vertexOffset + (x * byteLength)] * 0x100 + fileBytes[7 + vertexOffset + (x * byteLength)]), 0);
					vertexFloatY = 80 * BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[8 + vertexOffset + (x * byteLength)] * 0x1000000 + fileBytes[9 + vertexOffset + (x * byteLength)] * 0x10000 + fileBytes[10 + vertexOffset + (x * byteLength)] * 0x100 + fileBytes[11 + vertexOffset + (x * byteLength)]), 0);
				}
				else
				{
					vertexFloatX = 80 * BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[0 + vertexOffset + (x * byteLength)] * 1 + fileBytes[1 + vertexOffset + (x * byteLength)] * 0x100 + fileBytes[2 + vertexOffset + (x * byteLength)] * 0x10000 + fileBytes[3 + vertexOffset + (x * byteLength)] * 0x1000000), 0);
					vertexFloatZ = 80 * BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[4 + vertexOffset + (x * byteLength)] * 1 + fileBytes[5 + vertexOffset + (x * byteLength)] * 0x100 + fileBytes[6 + vertexOffset + (x * byteLength)] * 0x10000 + fileBytes[7 + vertexOffset + (x * byteLength)] * 0x1000000), 0);
					vertexFloatY = 80 * BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[8 + vertexOffset + (x * byteLength)] * 1 + fileBytes[9 + vertexOffset + (x * byteLength)] * 0x100 + fileBytes[10 + vertexOffset + (x * byteLength)] * 0x10000 + fileBytes[11 + vertexOffset + (x * byteLength)] * 0x1000000), 0);
				}

				if(stageMode == true)
				{
					vertexFloatX = vertexFloatX / 20;
					vertexFloatZ = vertexFloatZ / 20;
					vertexFloatY = vertexFloatY / 20;
				}

				vertexPosition.Add(new Vector3(vertexFloatX, vertexFloatY, vertexFloatZ));

				if(byteLength == 0x40)
				{
					float normalFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 0 + (x * byteLength)] * 0x1000000 + fileBytes[16 + vertexOffset + 1 + (x * byteLength)] * 0x10000 + fileBytes[16 + vertexOffset + 2 + (x * byteLength)] * 0x100 + fileBytes[16 + vertexOffset + 3 + (x * byteLength)]), 0);
					float normalFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 4 + (x * byteLength)] * 0x1000000 + fileBytes[16 + vertexOffset + 5 + (x * byteLength)] * 0x10000 + fileBytes[16 + vertexOffset + 6 + (x * byteLength)] * 0x100 + fileBytes[16 + vertexOffset + 7 + (x * byteLength)]), 0);
					float normalFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 8 + (x * byteLength)] * 0x1000000 + fileBytes[16 + vertexOffset + 9 + (x * byteLength)] * 0x10000 + fileBytes[16 + vertexOffset + 10 + (x * byteLength)] * 0x100 + fileBytes[16 + vertexOffset + 11 + (x * byteLength)]), 0);
					meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));

					vertexBone.Add(new Vector4(
                        (float)fileBytes[35 + vertexOffset + (x * byteLength)], 
                        (float)fileBytes[39 + vertexOffset + (x * byteLength)], 
                        (float)fileBytes[43 + vertexOffset + (x * byteLength)],
                        (float)fileBytes[47 + vertexOffset + (x * byteLength)]));

					float weightFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 0 + (x * byteLength)] * 0x1000000 + fileBytes[48 + vertexOffset + 1 + (x * byteLength)] * 0x10000 + fileBytes[48 + vertexOffset + 2 + (x * byteLength)] * 0x100 + fileBytes[48 + vertexOffset + 3 + (x * byteLength)]), 0);
					float weightFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 4 + (x * byteLength)] * 0x1000000 + fileBytes[48 + vertexOffset + 5 + (x * byteLength)] * 0x10000 + fileBytes[48 + vertexOffset + 6 + (x * byteLength)] * 0x100 + fileBytes[48 + vertexOffset + 7 + (x * byteLength)]), 0);
					float weightFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 8 + (x * byteLength)] * 0x1000000 + fileBytes[48 + vertexOffset + 9 + (x * byteLength)] * 0x10000 + fileBytes[48 + vertexOffset + 10 + (x * byteLength)] * 0x100 + fileBytes[48 + vertexOffset + 11 + (x * byteLength)]), 0);
                    float weightFloatW = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 12 + (x * byteLength)] * 0x1000000 + fileBytes[48 + vertexOffset + 13 + (x * byteLength)] * 0x10000 + fileBytes[48 + vertexOffset + 14 + (x * byteLength)] * 0x100 + fileBytes[48 + vertexOffset + 15 + (x * byteLength)]), 0);
                    vertexWeight.Add(new Vector4(weightFloatX, weightFloatY, weightFloatZ, weightFloatW));
				}
				else if(byteLength == 0x1C)
				{
					float normalFloatX = toFloat(fileBytes[12 + (x * byteLength)] * 0x100 + fileBytes[13 + (x * byteLength)]);
					float normalFloatY = toFloat(fileBytes[14 + (x * byteLength)] * 0x100 + fileBytes[15 + (x * byteLength)]);
					float normalFloatZ = toFloat(fileBytes[16 + (x * byteLength)] * 0x100 + fileBytes[17 + (x * byteLength)]);
					meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));
				}
				else if(byteLength == 0x20)
				{
					float normalFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 0 + (x * byteLength)] * 0x1000000 + fileBytes[16 + vertexOffset + 1 + (x * byteLength)] * 0x10000 + fileBytes[16 + vertexOffset + 2 + (x * byteLength)] * 0x100 + fileBytes[16 + vertexOffset + 3 + (x * byteLength)]), 0);
					float normalFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 4 + (x * byteLength)] * 0x1000000 + fileBytes[16 + vertexOffset + 5 + (x * byteLength)] * 0x10000 + fileBytes[16 + vertexOffset + 6 + (x * byteLength)] * 0x100 + fileBytes[16 + vertexOffset + 7 + (x * byteLength)]), 0);
					float normalFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 8 + (x * byteLength)] * 0x1000000 + fileBytes[16 + vertexOffset + 9 + (x * byteLength)] * 0x10000 + fileBytes[16 + vertexOffset + 10 + (x * byteLength)] * 0x100 + fileBytes[16 + vertexOffset + 11 + (x * byteLength)]), 0);
					meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));
				}

				GameObject actualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
				meshVertices.Add(vertexPosition[x]);

				actualObject.AddComponent<VertexObject>();
				actualObject.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
				actualObject.transform.position = vertexPosition[x];
				actualObject.name = x.ToString();
				actualObject.transform.SetParent(GameObject.Find("Model Data").transform);
				actualObject.transform.SetAsLastSibling();
				actualObject.tag = "Vertex";
				actualObject.layer = 9;
		    }

			mf.mesh.vertices = vertexPosition.ToArray();

			VertexCount = GameObject.Find("Model Data").transform.childCount;

			if(triangleFile.Length > 1)
			{
				//ConsoleMessage(" <color=lime>TRIANGLES LOADED.</color>");

				int[] num = new int[3];
				int a = 0;
				int q = 0;

				while(q + 5 < triangleFile.Length)
				{
					if(triangleFile[q].ToString("X2") + triangleFile[q + 1].ToString("X2") != "FFFF" && triangleFile[q + 2].ToString("X2") + triangleFile[q + 3].ToString("X2") != "FFFF" && triangleFile[q + 4].ToString("X2") + triangleFile[q + 5].ToString("X2") != "FFFF")
					{
						if(mod == 0) 
						{
							num[0] = triangleFile[q] * 0x100 + triangleFile[q + 1];
							num[1] = triangleFile[q + 2] * 0x100 + triangleFile[q + 3];
							num[2] = triangleFile[q + 4] * 0x100 + triangleFile[q + 5];
							q = q + 2;
						}
						else 
						{
							num[0] = triangleFile[q + 1] * 0x100 + triangleFile[q + 0];
							num[1] = triangleFile[q + 3] * 0x100 + triangleFile[q + 2];
							num[2] = triangleFile[q + 5] * 0x100 + triangleFile[q + 4];
							q = q + 6;
						}
						a++;

						if(meshVertices.Count >= num[0] && meshVertices.Count >= num[1] && meshVertices.Count >= num[2])
						{
							meshTriangles.Add(num[0]);
							meshTriangles.Add(num[1]);
							meshTriangles.Add(num[2]);

							if(endianess == false) meshTriangles.Add(num[2]);
							if(endianess == false) meshTriangles.Add(num[1]);
							if(endianess == false) meshTriangles.Add(num[0]);

							mf.mesh.triangles = meshTriangles.ToArray();
						}
					}
					else
					{
						if(mod == 0) 
						{
							q = q + 2;
						}
						else 
						{
							q = q + 6;
						}
					}
				}
			}

			if(textureMapFile.Length > 1)
			{
				int x = 4;
				while(textureMapFile[x] != 0x100)
				{
					x++;
				}

				if(x == 8)
				{
					textureType = 0;
				}
				else
				{
					textureType = 1;
				}
			}

			if(byteLength == 64)
			{
				if(textureMapFile.Length > 1)
				{
					if(textureType == 0)
					{
						for(int x = 0; x < VertexCount; x++)
						{
							float x_ = toFloat(textureMapFile[4 + (8 * x)] * 0x100 + textureMapFile[5 + (8 * x)]);
							float y_ = toFloat(textureMapFile[6 + (8 * x)] * 0x100 + textureMapFile[7 + (8 * x)]);

							TextureUVs.Add(new Vector2(x_, y_));
						}
					}
					else if(textureType == 1)
					{
						for(int x = 0; x < VertexCount; x++)
						{
							float x_ = toFloat(textureMapFile[4 + (12 * x)] * 0x100 + textureMapFile[5 + (12 * x)]);
							float y_ = toFloat(textureMapFile[6 + (12 * x)] * 0x100 + textureMapFile[7 + (12 * x)]);

							TextureUVs.Add(new Vector2(x_, y_));
						}
					}
				}
			}
			else if(byteLength == 28)
			{
				for(int x = 0; x < VertexCount; x++)
				{
					float x_ = toFloat(fileBytes[x * 24] * 0x100 + fileBytes[x * 25]);
					float y_ = toFloat(fileBytes[x * 26] * 0x100 + fileBytes[x * 27]);

					TextureUVs.Add(new Vector2(x_, y_));
				}
			}
			else if(byteLength == 0x20)
			{
				for(int x = 0; x < VertexCount; x++)
				{
					float x_ = BitConverter.ToSingle(
						BitConverter.GetBytes(
							fileBytes[x * 0x18] * 0x1000000 + 
							fileBytes[x * 0x19] * 0x10000 + 
							fileBytes[x * 0x1A] * 0x100 + 
							fileBytes[x * 0x1B]), 0);

					float y_ = BitConverter.ToSingle(
						BitConverter.GetBytes(
							fileBytes[x * 0x1C] * 0x1000000 + 
							fileBytes[x * 0x1D] * 0x10000 + 
							fileBytes[x * 0x1E] * 0x100 + 
							fileBytes[x * 0x1F]), 0);

					TextureUVs.Add(new Vector2(x_, y_));
				}
			}

			DialogResult result_ = MessageBox.Show("Do you want to load a .png texture?", "Texture loading", MessageBoxButtons.YesNo);
			if(result_ == DialogResult.Yes)
			{
				////ConsoleMessage(" <color=cyan>TEXTURE IMAGE LOADED.</color>");
				OpenFileDialog openFileDialog2 = new OpenFileDialog();
				openFileDialog2.DefaultExt = "png";
				openFileDialog2.ShowDialog();

				if(openFileDialog2.FileName != "" && File.Exists(openFileDialog2.FileName))
				{
					try
					{
						byte[] textureBytes = File.ReadAllBytes(openFileDialog2.FileName);
						Texture2D extTexture = new Texture2D(1024, 1024);
						extTexture.LoadImage(textureBytes);
						RenderedMesh.GetComponent<Renderer>().material.mainTexture = extTexture;
						RenderedMesh.GetComponent<RenderMaterial>().Materials_[0] = RenderedMesh.GetComponent<Renderer>().material;
					}
					catch(Exception)
					{
						ConsoleMessage("\n<color=orange>Error loading texture.</color>");
					}
				}
				else
				{
					RenderedMesh.GetComponent<Renderer>().material = RenderedMesh.GetComponent<RenderMaterial>().Materials_[1];
				}
			}
			else
			{
				//ConsoleMessage(" <color=red>TEXTURE IMAGE NOT FOUND.</color>");
				RenderedMesh.GetComponent<Renderer>().material = RenderedMesh.GetComponent<RenderMaterial>().Materials_[1];
			}

			mf.mesh.uv = TextureUVs.ToArray();
			mf.mesh.normals = meshNormals.ToArray();
	        FinishedDrawingModel = true;
	       	fileOpen = true;
			ConsoleMessage(" <color=lime>MODEL LOADED.</color>");
		}
    }

    public void ImportModelRE2(string importPath)
    {
        try
        {
            string[] objModelLines = File.ReadAllLines(importPath);
            GroupSelection g = GetComponent<GroupSelection>();
            g.GroupNames.Clear();
            g.Groups.Clear();
            g.TrianglesPerGroup.Clear();

            TextureUVs.Clear();

            // For auto rigger
            List<Vector3> oldMeshVertices = new List<Vector3>();
            List<Vector3> oldVertexPosition = new List<Vector3>();

            for (int x = 0; x < meshVertices.Count; x++)
            {
                oldMeshVertices.Add(meshVertices[x]);
                oldVertexPosition.Add(vertexPosition[x]);
            }

            List<Vector4> oldVertexBone = new List<Vector4>();
            List<Vector4> oldVertexWeight = new List<Vector4>();
            for (int x = 0; x < oldMeshVertices.Count; x++)
            {
                oldVertexBone.Add(vertexBone[x]);
                oldVertexWeight.Add(vertexWeight[x]);
            }

            selectedVertex.Clear();
            meshVertices.Clear();
            vertexPosition.Clear();
            meshNormals.Clear();
            meshTriangles.Clear();

            List<List<int>> groupsInObj = new List<List<int>>();

            // Clear mesh data
            mf.mesh.Clear();

            // Destroy all vertex gameobjects in the world
            for (int x = 0; x < VertexCount; x++)
            {
                int indexinlist = vertexPosition.IndexOf(GameObject.Find(x.ToString()).transform.position);
                Destroy(GameObject.Find(x.ToString()));
            }

            // Set vertexcount to 0
            VertexCount = 0;

            List<Vector3> VerticesInObj = new List<Vector3>();
            List<Vector3> NormalsInObj = new List<Vector3>();
            List<Vector2> TextureInObj = new List<Vector2>();

            DialogResult dialog = DialogResult.No;

            string[] boneLines = new string[0];

            if (byteLength == 0x40)
            {
                //dialog = MessageBox.Show("Do you want to load a .bones file? It has to have the same vertex number as the original model.", "Bone importing", MessageBoxButtons.YesNo);
                dialog = DialogResult.No;
            }

            string fileP = "";
            /*if (dialog == DialogResult.Yes)
            {
                vertexBone.Clear();
                vertexWeight.Clear();

                OpenFileDialog openf = new OpenFileDialog();
                openf.ShowDialog();

                if (openf.FileName != "" && File.Exists(openf.FileName))
                {
                    fileP = openf.FileName;
                    boneLines = File.ReadAllLines(fileP);
                }
            }*/

            bool importBones = false;

            List<Vector4> BoneIndexObj = new List<Vector4>();
            List<Vector4> BoneWeightObj = new List<Vector4>();

            if (fileP != "")
            {
                /*boneLines = File.ReadAllLines(fileP);
                importBones = true;
                for (int h = 0; h < boneLines.ToList().Count / 6; h++)
                {
                    int[] bones_ = new int[3];
                    bones_[0] = int.Parse((boneLines[(6 * h) + 0]));
                    bones_[1] = int.Parse((boneLines[(6 * h) + 1]));
                    bones_[2] = int.Parse((boneLines[(6 * h) + 2]));

                    float[] weights_ = new float[3];
                    weights_[0] = float.Parse((boneLines[(6 * h) + 3]));
                    weights_[1] = float.Parse((boneLines[(6 * h) + 4]));
                    weights_[2] = float.Parse((boneLines[(6 * h) + 5]));

                    BoneIndexObj.Add(new Vector4(bones_[0], bones_[1], bones_[2], bones_[3]));
                    BoneWeightObj.Add(new Vector4(weights_[0], weights_[1], weights_[2], weights_[3]));
                }

                vertexBone = BoneIndexObj;
                vertexWeight = BoneWeightObj;*/
            }
            else
            {
                dialog = MessageBox.Show("[Experimental] Do you want to try rigging this model automatically?", "Auto-rigger", MessageBoxButtons.YesNo);

                /*dialog = MessageBox.Show("Do you want to fill all the bones with 0s?", "Bone importing", MessageBoxButtons.YesNo);

                if (dialog == DialogResult.Yes)
                {
                    for (int x = 0; x < vertexBone.Count; x++)
                    {
                        BoneIndexObj.Add(new Vector3(0, 0, 0));
                        BoneWeightObj.Add(new Vector3(0, 0, 0));
                    }
                }*/
            }

            List<List<Vector2>> VerticesUVIndex = new List<List<Vector2>>();

            // Read vert position and save it to a variable
            for (int x = 0; x < objModelLines.Length; x++)
            {
                string line_ = objModelLines[x];
                if (line_.Length > 2)
                {
                    string[] array2 = line_.Split(' ');
                    if (array2[0] == "v")
                    {
                        float posx = 0f - float.Parse(array2[1], CultureInfo.InvariantCulture);
                        float posy = float.Parse(array2[2], CultureInfo.InvariantCulture);
                        float posz = float.Parse(array2[3], CultureInfo.InvariantCulture);
                        VerticesInObj.Add(new Vector3(posx, posy, posz));
                        VerticesUVIndex.Add(new List<Vector2>());
                    }
                    else if (array2[0] == "vt")
                    {
                        float x2 = float.Parse(array2[1], CultureInfo.InvariantCulture);
                        float y2 = float.Parse(array2[2], CultureInfo.InvariantCulture);
                        TextureInObj.Add(new Vector2(x2, y2));
                    }
                    else if (array2[0] == "vn")
                    {
                        float x3 = 0f - float.Parse(array2[1], CultureInfo.InvariantCulture);
                        float y3 = float.Parse(array2[2], CultureInfo.InvariantCulture);
                        float z2 = float.Parse(array2[3], CultureInfo.InvariantCulture);
                        NormalsInObj.Add(new Vector3(x3, y3, z2));
                    }
                }
            }

            int actualGroup = -1;

            // Look for faces. 3 1 2 creates a new face with new vertices positions (3, 1, 2)
            for (int x = 0; x < objModelLines.Length; x++)
            {
                string line_ = objModelLines[x];

                if (line_.Length > 2)
                {
                    if ((line_[0] == 'g' || line_[0] == 'o') && line_[1] == ' ')
                    {
                        string gName = line_.Substring(2, line_.Length - 2);
                        List<string> par = new List<string>() { gName };
                        GetComponent<ConsoleCommandBehaviour>().Command_CreateGroup(par);
                        groupsInObj.Add(new List<int>());
                        actualGroup++;
                        g.TrianglesPerGroup.Add(0);
                    }

                    if (line_[0] == 'f' && line_[1] == ' ')
                    {
                        g.TrianglesPerGroup[actualGroup] = g.TrianglesPerGroup[actualGroup] + 1;
                        int numberofslash = 0;
                        for (int _x = 0; _x < line_.Length; _x++)
                        {
                            if (line_[_x] == '/')
                            {
                                numberofslash++;
                            }
                        }

                        if (numberofslash != 0 && numberofslash != 3 && numberofslash != 6)
                        {
                            MessageBox.Show("SN = " + numberofslash.ToString() + ". This error isn't supposed to happen. Report it to Zealot.");
                        }

                        // If numberofslash is 0, it means that the face is only composed of vertex numbers
                        if (numberofslash == 0)
                        {
                            List<char> charOfVertex1 = new List<char>();
                            List<char> charOfVertex2 = new List<char>();
                            List<char> charOfVertex3 = new List<char>();

                            int a = 0;
                            // Get number of vertex 1
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfVertex1.Add(line_[a]);
                                a++;
                            }

                            // Skip space
                            a++;

                            // Get number of vertex 2
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfVertex2.Add(line_[a]);
                                a++;
                            }

                            // Skip space
                            a++;

                            // Get number of vertex 3
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfVertex3.Add(line_[a]);
                                a++;
                            }

                            int[] triVert = new int[3];
                            int[] triVertID = new int[3];

                            triVert[0] = int.Parse(new String(charOfVertex1.ToArray())) - 1;
                            triVert[1] = int.Parse(new String(charOfVertex2.ToArray())) - 1;
                            triVert[2] = int.Parse(new String(charOfVertex3.ToArray())) - 1;

                            bool[] vertExists = new bool[3] { false, false, false };
                            int[] vertNum = new int[3] { 0, 0, 0 };

                            for (int w = 0; w < 3; w++)
                            {
                                if (VerticesUVIndex[triVert[w]].Count == 0)
                                {
                                    vertExists[w] = false;
                                }
                                else
                                {
                                    vertExists[w] = true;
                                    vertNum[w] = int.Parse(VerticesUVIndex[triVert[w]][0].x.ToString());
                                }
                            }

                            for (int q = 0; q < 3; q++)
                            {
                                if (vertExists[q] == false)
                                {
                                    GameObject actualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    triVertID[q] = VertexCount;
                                    meshVertices.Add(VerticesInObj[triVert[q]]);

                                    if (byteLength == 0x40 && importBones && BoneIndexObj.Count > triVert[q])
                                    {
                                        vertexBone.Add(BoneIndexObj[triVert[q]]);
                                        vertexWeight.Add(BoneWeightObj[triVert[q]]);
                                    }

                                    actualObject.AddComponent<VertexObject>();
                                    actualObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                    actualObject.transform.position = VerticesInObj[triVert[q]];
                                    actualObject.transform.SetParent(GameObject.Find("Model Data").transform);
                                    actualObject.name = VertexCount.ToString();
                                    actualObject.transform.SetAsLastSibling();
                                    actualObject.tag = "Vertex";
                                    actualObject.layer = 9;

                                    VerticesUVIndex[triVert[q]].Add(new Vector2(triVertID[q], 0));

                                    VertexCount++;
                                }
                                else
                                {
                                    triVertID[q] = vertNum[q];
                                }
                            }

                            meshTriangles.Add(triVertID[2]);
                            meshTriangles.Add(triVertID[1]);
                            meshTriangles.Add(triVertID[0]);

                            if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[0]);
                            if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[1]);
                            if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[2]);
                        }
                        else if (numberofslash == 3)
                        {
                            List<char> charOfVertex1 = new List<char>();
                            List<char> charOfTexture1 = new List<char>();

                            List<char> charOfVertex2 = new List<char>();
                            List<char> charOfTexture2 = new List<char>();

                            List<char> charOfVertex3 = new List<char>();
                            List<char> charOfTexture3 = new List<char>();

                            int a = 0;
                            // Get number of vertex 1
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfVertex1.Add(line_[a]);
                                a++;
                            }

                            // Skip slash
                            a++;

                            // Get number of texture 1
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfTexture1.Add(line_[a]);
                                a++;
                            }

                            // Skip space
                            a++;

                            // Get number of vertex 2
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfVertex2.Add(line_[a]);
                                a++;
                            }

                            // Skip slash
                            a++;

                            // Get number of texture 2
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfTexture2.Add(line_[a]);
                                a++;
                            }

                            // Skip space
                            a++;

                            // Get number of vertex 3
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfVertex3.Add(line_[a]);
                                a++;
                            }

                            // Skip slash
                            a++;

                            // Get number of texture 3
                            for (int b = a; b < line_.Length; b++)
                            {
                                if (Char.IsDigit(line_[b]))
                                {
                                    a = b;
                                    break;
                                }
                            }
                            while (a < line_.Length && Char.IsDigit(line_[a]))
                            {
                                charOfTexture3.Add(line_[a]);
                                a++;
                            }

                            int[] triVert = new int[3];
                            triVert[0] = int.Parse(new String(charOfVertex1.ToArray())) - 1;
                            triVert[1] = int.Parse(new String(charOfVertex2.ToArray())) - 1;
                            triVert[2] = int.Parse(new String(charOfVertex3.ToArray())) - 1;
                            int[] triVertID = new int[3];

                            int[] triTex = new int[3];
                            triTex[0] = int.Parse(new String(charOfTexture1.ToArray())) - 1;
                            triTex[1] = int.Parse(new String(charOfTexture2.ToArray())) - 1;
                            triTex[2] = int.Parse(new String(charOfTexture3.ToArray())) - 1;

                            bool[] vertExists = new bool[3] { false, false, false };
                            int[] vertNum = new int[3] { 0, 0, 0 };

                            for (int w = 0; w < 3; w++)
                            {
                                if (VerticesUVIndex[triVert[w]].Count == 0)
                                {
                                    vertExists[w] = false;
                                }
                                else
                                {
                                    for (int e = 0; e < VerticesUVIndex[triVert[w]].Count; e++)
                                    {
                                        if (VerticesUVIndex[triVert[w]][e].y == triTex[w])
                                        {
                                            vertExists[w] = true;
                                            vertNum[w] = int.Parse(VerticesUVIndex[triVert[w]][e].x.ToString());
                                        }
                                    }
                                }
                            }

                            for (int q = 0; q < 3; q++)
                            {
                                if (vertExists[q] == false)
                                {
                                    GameObject actualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    triVertID[q] = VertexCount;
                                    meshVertices.Add(VerticesInObj[triVert[q]]);

                                    if (byteLength == 0x40 && importBones && BoneIndexObj.Count > triVert[q])
                                    {
                                        vertexBone.Add(BoneIndexObj[triVert[q]]);
                                        vertexWeight.Add(BoneWeightObj[triVert[q]]);
                                    }

                                    actualObject.AddComponent<VertexObject>();
                                    actualObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                    actualObject.transform.position = VerticesInObj[triVert[q]];
                                    actualObject.transform.SetParent(GameObject.Find("Model Data").transform);
                                    actualObject.name = VertexCount.ToString();
                                    actualObject.transform.SetAsLastSibling();
                                    actualObject.tag = "Vertex";
                                    actualObject.layer = 9;

                                    VerticesUVIndex[triVert[q]].Add(new Vector2(triVertID[q], triTex[q]));

                                    VertexCount++;

                                    TextureUVs.Add(TextureInObj[triTex[q]]);
                                }
                                else
                                {
                                    triVertID[q] = vertNum[q];
                                }
                            }

                            meshTriangles.Add(triVertID[2]);
                            meshTriangles.Add(triVertID[1]);
                            meshTriangles.Add(triVertID[0]);
                            if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[0]);
                            if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[1]);
                            if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[2]);
                        }
                        else if (numberofslash == 6)
                        {
                            bool isTexture = true;
                            for (int _x = 0; _x < line_.Length - 1; _x++)
                            {
                                if (line_[_x] == '/' && line_[_x + 1] == '/')
                                {
                                    isTexture = false;
                                    break;
                                }
                            }

                            if (isTexture)
                            {
                                string[] LineSplit = line_.Split(' ');
                                string[] Line1 = LineSplit[1].Split('/');
                                string[] Line2 = LineSplit[2].Split('/');
                                string[] Line3 = LineSplit[3].Split('/');

                                int Vertex1 = int.Parse(Line1[0]);
                                int Texture1 = int.Parse(Line1[1]);
                                int Normal1 = int.Parse(Line1[2]);

                                int Vertex2 = int.Parse(Line2[0]);
                                int Texture2 = int.Parse(Line2[1]);
                                int Normal2 = int.Parse(Line2[2]);

                                int Vertex3 = int.Parse(Line3[0]);
                                int Texture3 = int.Parse(Line3[1]);
                                int Normal3 = int.Parse(Line3[2]);

                                int[] triVert = new int[3];
                                triVert[0] = Vertex1 - 1;
                                triVert[1] = Vertex2 - 1;
                                triVert[2] = Vertex3 - 1;
                                int[] triVertID = new int[3];

                                int[] triTex = new int[3];
                                triTex[0] = Texture1 - 1;
                                triTex[1] = Texture2 - 1;
                                triTex[2] = Texture3 - 1;

                                int[] triNormal = new int[3];
                                triNormal[0] = Normal1 - 1;
                                triNormal[1] = Normal2 - 1;
                                triNormal[2] = Normal3 - 1;

                                bool[] vertExists = new bool[3] { false, false, false };
                                int[] vertNum = new int[3] { 0, 0, 0 };

                                for (int w = 0; w < 3; w++)
                                {
                                    if (VerticesUVIndex[triVert[w]].Count == 0)
                                    {
                                        vertExists[w] = false;
                                    }
                                    else
                                    {
                                        for (int e = 0; e < VerticesUVIndex[triVert[w]].Count; e++)
                                        {
                                            if (VerticesUVIndex[triVert[w]][e].y == triTex[w])
                                            {
                                                vertExists[w] = true;
                                                vertNum[w] = int.Parse(VerticesUVIndex[triVert[w]][e].x.ToString());
                                            }
                                        }
                                    }
                                }

                                for (int q = 0; q < 3; q++)
                                {
                                    if (vertExists[q] == false)
                                    {
                                        GameObject actualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                        triVertID[q] = VertexCount;
                                        meshVertices.Add(VerticesInObj[triVert[q]]);

                                        if (byteLength == 0x40 && importBones && BoneIndexObj.Count > triVert[q])
                                        {
                                            vertexBone.Add(BoneIndexObj[triVert[q]]);
                                            vertexWeight.Add(BoneWeightObj[triVert[q]]);
                                        }

                                        actualObject.AddComponent<VertexObject>();
                                        actualObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                        actualObject.transform.position = VerticesInObj[triVert[q]];
                                        actualObject.transform.SetParent(GameObject.Find("Model Data").transform);
                                        actualObject.name = VertexCount.ToString();
                                        actualObject.transform.SetAsLastSibling();
                                        actualObject.tag = "Vertex";
                                        actualObject.layer = 9;

                                        VerticesUVIndex[triVert[q]].Add(new Vector2(triVertID[q], triTex[q]));

                                        VertexCount++;

                                        TextureUVs.Add(TextureInObj[triTex[q]]);
                                        meshNormals.Add(NormalsInObj[triNormal[q]]);
                                    }
                                    else
                                    {
                                        triVertID[q] = vertNum[q];
                                    }
                                }

                                meshTriangles.Add(triVertID[2]);
                                meshTriangles.Add(triVertID[1]);
                                meshTriangles.Add(triVertID[0]);
                                if (groupsInObj.Count != 0 && !groupsInObj[groupsInObj.Count - 1].Contains(triVertID[0])) groupsInObj[groupsInObj.Count - 1].Add(triVertID[0]);
                                if (groupsInObj.Count != 0 && !groupsInObj[groupsInObj.Count - 1].Contains(triVertID[1])) groupsInObj[groupsInObj.Count - 1].Add(triVertID[1]);
                                if (groupsInObj.Count != 0 && !groupsInObj[groupsInObj.Count - 1].Contains(triVertID[2])) groupsInObj[groupsInObj.Count - 1].Add(triVertID[2]);
                            }
                            else
                            {
                                List<char> charOfVertex1 = new List<char>();
                                List<char> charOfNormal1 = new List<char>();

                                List<char> charOfVertex2 = new List<char>();
                                List<char> charOfNormal2 = new List<char>();

                                List<char> charOfVertex3 = new List<char>();
                                List<char> charOfNormal3 = new List<char>();

                                int a = 0;
                                // Get number of vertex 1
                                for (int b = a; b < line_.Length; b++)
                                {
                                    if (Char.IsDigit(line_[b]))
                                    {
                                        a = b;
                                        break;
                                    }
                                }
                                while (a < line_.Length && Char.IsDigit(line_[a]))
                                {
                                    charOfVertex1.Add(line_[a]);
                                    a++;
                                }

                                // Skip slash
                                a++;
                                a++;

                                // Get number of normal 1
                                for (int b = a; b < line_.Length; b++)
                                {
                                    if (Char.IsDigit(line_[b]))
                                    {
                                        a = b;
                                        break;
                                    }
                                }
                                while (a < line_.Length && Char.IsDigit(line_[a]))
                                {
                                    charOfNormal1.Add(line_[a]);
                                    a++;
                                }

                                // Skip space
                                a++;

                                // Get number of vertex 2
                                for (int b = a; b < line_.Length; b++)
                                {
                                    if (Char.IsDigit(line_[b]))
                                    {
                                        a = b;
                                        break;
                                    }
                                }
                                while (a < line_.Length && Char.IsDigit(line_[a]))
                                {
                                    charOfVertex2.Add(line_[a]);
                                    a++;
                                }

                                // Skip slash
                                a++;
                                a++;

                                // Get number of normal 2
                                for (int b = a; b < line_.Length; b++)
                                {
                                    if (Char.IsDigit(line_[b]))
                                    {
                                        a = b;
                                        break;
                                    }
                                }
                                while (a < line_.Length && Char.IsDigit(line_[a]))
                                {
                                    charOfNormal2.Add(line_[a]);
                                    a++;
                                }

                                // Skip space
                                a++;

                                // Get number of vertex 3
                                for (int b = a; b < line_.Length; b++)
                                {
                                    if (Char.IsDigit(line_[b]))
                                    {
                                        a = b;
                                        break;
                                    }
                                }
                                while (a < line_.Length && Char.IsDigit(line_[a]))
                                {
                                    charOfVertex3.Add(line_[a]);
                                    a++;
                                }

                                // Skip slash
                                a++;
                                a++;

                                // Get number of normal 3
                                for (int b = a; b < line_.Length; b++)
                                {
                                    if (Char.IsDigit(line_[b]))
                                    {
                                        a = b;
                                        break;
                                    }
                                }
                                while (a < line_.Length && Char.IsDigit(line_[a]))
                                {
                                    charOfNormal3.Add(line_[a]);
                                    a++;
                                }

                                int[] triVert = new int[3];
                                triVert[0] = int.Parse(new String(charOfVertex1.ToArray())) - 1;
                                triVert[1] = int.Parse(new String(charOfVertex2.ToArray())) - 1;
                                triVert[2] = int.Parse(new String(charOfVertex3.ToArray())) - 1;
                                int[] triVertID = new int[3];

                                int[] triNormal = new int[3];
                                triNormal[0] = int.Parse(new String(charOfNormal1.ToArray())) - 1;
                                triNormal[1] = int.Parse(new String(charOfNormal2.ToArray())) - 1;
                                triNormal[2] = int.Parse(new String(charOfNormal3.ToArray())) - 1;

                                bool[] vertExists = new bool[3] { false, false, false };
                                int[] vertNum = new int[3] { 0, 0, 0 };

                                for (int w = 0; w < 3; w++)
                                {
                                    if (VerticesUVIndex[triVert[w]].Count == 0)
                                    {
                                        vertExists[w] = false;
                                    }
                                    else
                                    {
                                        vertExists[w] = true;
                                        vertNum[w] = int.Parse(VerticesUVIndex[triVert[w]][0].x.ToString());
                                    }
                                }

                                for (int q = 0; q < 3; q++)
                                {
                                    if (vertExists[q] == false)
                                    {
                                        GameObject actualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                        triVertID[q] = VertexCount;
                                        meshVertices.Add(VerticesInObj[triVert[q]]);

                                        if (byteLength == 0x40 && importBones && BoneIndexObj.Count > triVert[q])
                                        {
                                            vertexBone.Add(BoneIndexObj[triVert[q]]);
                                            vertexWeight.Add(BoneWeightObj[triVert[q]]);
                                        }

                                        actualObject.AddComponent<VertexObject>();
                                        actualObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                        actualObject.transform.position = VerticesInObj[triVert[q]];
                                        actualObject.transform.SetParent(GameObject.Find("Model Data").transform);
                                        actualObject.name = VertexCount.ToString();
                                        actualObject.transform.SetAsLastSibling();
                                        actualObject.tag = "Vertex";
                                        actualObject.layer = 9;
                                        VertexCount++;

                                        TextureUVs.Add(Vector2.zero);
                                        meshNormals.Add(NormalsInObj[triNormal[q]]);
                                    }
                                    else
                                    {
                                        triVertID[q] = vertNum[q];
                                    }
                                }

                                meshTriangles.Add(triVertID[2]);
                                meshTriangles.Add(triVertID[1]);
                                meshTriangles.Add(triVertID[0]);
                                if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[0]);
                                if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[1]);
                                if (groupsInObj.Count != 0) groupsInObj[groupsInObj.Count - 1].Add(triVertID[2]);
                            }
                        }
                    }
                }
            }

            GetComponent<GroupSelection>().Groups = groupsInObj;

            if (meshVertices.Count == 0)
            {
                GameObject actualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                meshVertices.Add(new Vector3(0, 0, 0));

                if (byteLength == 0x40)
                {
                    vertexBone.Add(new Vector4(1, 1, 1, 1));
                    vertexWeight.Add(new Vector4(1, 0, 0, 0));
                }

                actualObject.AddComponent<VertexObject>();
                actualObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                actualObject.transform.position = meshVertices[0];
                actualObject.transform.SetParent(GameObject.Find("Model Data").transform);
                actualObject.name = VertexCount.ToString();
                actualObject.transform.SetAsLastSibling();
                actualObject.tag = "Vertex";
                actualObject.layer = 9;

                VertexCount++;

                TextureUVs.Add(new Vector2(0, 0));

                meshTriangles.Add(0);
                meshTriangles.Add(0);
                meshTriangles.Add(0);
            }

            vertexPosition = meshVertices;

            MessageBox.Show("VertexCount: " + VertexCount.ToString() + ", VertexBone: " + vertexBone.Count);

            if (VertexCount > vertexBone.Count && byteLength == 0x40)
            {
                MessageBox.Show("New vertices were filled with bones of ID 1.");
                int boneC = vertexBone.Count;
                for (int x = boneC; x < VertexCount; x++)
                {
                    vertexBone.Add(new Vector4(1, 1, 1, 1));
                    vertexWeight.Add(new Vector4(1, 0, 0, 0));
                }
            }

            // AUTO RIGGER STUFF (EXPERIMENTAL)
            if(dialog == DialogResult.Yes)
            {
                //MessageBox.Show("Auto rigging...");
                float autorigger_closestdistance = -1;
                int autorigger_closestindex = -1;
                for (int x = 0; x < meshVertices.Count; x++)
                {
                    for (int y = 0; y < oldMeshVertices.Count; y++)
                    {
                        if (autorigger_closestdistance == -1)
                        {
                            autorigger_closestdistance = Vector3.Distance(meshVertices[x], oldMeshVertices[y]);
                            autorigger_closestindex = y;
                        }
                        else
                        {
                            float thisDistance = Vector3.Distance(meshVertices[x], oldMeshVertices[y]);
                            if (thisDistance < autorigger_closestdistance)
                            {
                                autorigger_closestdistance = thisDistance;
                                autorigger_closestindex = y;
                            }
                        }
                    }

                    UnityEngine.Debug.Log("bone: " + x.ToString() + ", id: " + autorigger_closestindex.ToString());
                    vertexBone[x] = oldVertexBone[autorigger_closestindex];
                    vertexWeight[x] = oldVertexWeight[autorigger_closestindex];
                    autorigger_closestindex = -1;
                    autorigger_closestdistance = -1;
                }
                MessageBox.Show("Finished auto rigging. Some bones might be inaccurate. Check your model in game.");
            }

            for (int x = 0; x < TextureUVs.ToArray().Length; x++)
            {
                TextureUVs[x] = new Vector2(TextureUVs[x].x, TextureUVs[x].y * -1);
            }

            mf.mesh.vertices = meshVertices.ToArray();

            while (TextureUVs.ToArray().Length < meshVertices.ToArray().Length)
            {
                TextureUVs.Add(new Vector2(0, 0));
            }

            while (meshNormals.ToArray().Length < meshVertices.ToArray().Length)
            {
                meshNormals.Add(Vector3.forward);
            }

            mf.mesh.uv = TextureUVs.ToArray();
            mf.mesh.normals = meshNormals.ToArray();
            mf.mesh.triangles = meshTriangles.ToArray();

            /*MessageBox.Show("Mesh imported correctly.\n\nInitial vertices: " + InitialVertexCount.ToString() + "\nNew vertices: " + VertexCount.ToString());
            MessageBox.Show(
                    "Group 1 Vertices: " + g.Groups[0].Count.ToString("X2") + "\n" +
                    "Group 1 Triangles: " + g.TrianglesPerGroup[0].ToString("X2") + "\n" +
                    "Group 2 Vertices: " + g.Groups[1].Count.ToString("X2") + "\n" +
                    "Group 2 Triangles: " + g.TrianglesPerGroup[1].ToString("X2") + "\n");*/
        }
        catch (Exception exe)
        {
            MessageBox.Show(exe.ToString());
        }
    }

    public void Undo()
	{
		if(undo_action.Count == 0) return;

		if(undo_action[undo_action.Count - 1] == 0)
		{
			for(int x = 0; x < undo_sel[undo_sel.Count - 1].Count; x++)
			{
				int actualVert = undo_sel[undo_sel.Count - 1][x];

				meshVertices[actualVert] = new Vector3(
					meshVertices[actualVert].x - undo_pos[undo_pos.Count - 1].x, 
					meshVertices[actualVert].y - undo_pos[undo_pos.Count - 1].y,
					meshVertices[actualVert].z - undo_pos[undo_pos.Count - 1].z);

				GameObject.Find("Model Data").transform.Find(actualVert.ToString()).transform.position = meshVertices[actualVert];
			}

			vertexPosition = meshVertices;
			mf.mesh.vertices = meshVertices.ToArray();
			undo_action.RemoveAt(undo_action.Count - 1);
			undo_pos.RemoveAt(undo_pos.Count - 1);
			undo_sel.RemoveAt(undo_sel.Count - 1);

			ConsoleMessage("<color=orange>\nReverted last action</color>.");
		}
		else if(undo_action[undo_action.Count - 1] == 1)
		{
			List<GameObject> selectedVertexTemp = new List<GameObject>();
			for(int x = 0; x < undo_sel[undo_sel.Count - 1].Count; x++)
			{
				int actualVert = undo_sel[undo_sel.Count - 1][x];

				selectedVertexTemp.Add(GameObject.Find("Model Data").transform.Find(actualVert.ToString()).gameObject);
			}

			RotateModel(-undo_pos[undo_pos.Count - 1].x, -undo_pos[undo_pos.Count - 1].y, -undo_pos[undo_pos.Count - 1].z, true);

			undo_action.RemoveAt(undo_action.Count - 1);
			undo_pos.RemoveAt(undo_pos.Count - 1);
			undo_sel.RemoveAt(undo_sel.Count - 1);

			ConsoleMessage("<color=orange>\nReverted last action</color>.");
		}
		else if(undo_action[undo_action.Count - 1] == 2)
		{
			List<GameObject> selectedVertexTemp = new List<GameObject>();
			for(int x = 0; x < undo_sel[undo_sel.Count - 1].Count; x++)
			{
				int actualVert = undo_sel[undo_sel.Count - 1][x];

				selectedVertexTemp.Add(GameObject.Find("Model Data").transform.Find(actualVert.ToString()).gameObject);
			}

			ScaleModel(1 / undo_pos[undo_pos.Count - 1].x, 1 / undo_pos[undo_pos.Count - 1].y, 1 / undo_pos[undo_pos.Count - 1].z, true);

			undo_action.RemoveAt(undo_action.Count - 1);
			undo_pos.RemoveAt(undo_pos.Count - 1);
			undo_sel.RemoveAt(undo_sel.Count - 1);

			ConsoleMessage("<color=orange>\nReverted last action</color>.");
		}
	}

}