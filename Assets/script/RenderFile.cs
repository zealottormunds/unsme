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
	public bool canMove = false;
	public bool inCommand = false;
	public bool fileOpen = false;
	public InputField CommandInput;

	public GameObject cameraObject;
	public GameObject Arrows;
	public Transform Pointer;
	public Camera mainCamera;
	private float CameraSpeed = 100;
	private Vector3 camRot = new Vector3(0, 0, 0);

	public GameObject SphereTest;
	public float SensitivityMouse = 200f;

	public Text modelInformation;

	public int vertexOffset = 0;
	public int byteLenght = 24;
	private byte[] fileBytes;
	public bool FinishedDrawingModel = false;

	public int VertexCount = 0;
	public List<GameObject> selectedVertex;
	private List<Vector3> vertexPosition = new List<Vector3>();

	private GameObject RenderedMesh;
	private MeshFilter mf;
	private MeshRenderer mr;

	private List<Vector3> meshVertices = new List<Vector3>();
	private	List<int> meshTriangles = new List<int>();
	private List<Vector3> meshNormals = new List<Vector3>();
	private List<Vector2> TextureUVs = new List<Vector2>();
	public List<Vector3> vertexBone = new List<Vector3>();
	public string PathToModel;
	public int importmode = 0;
	byte[] triangleFile;
	byte[] textureMapFile;
	bool invertTrianglesBool = false;

	public bool WindowOpen = false;
	public GameObject toolBoxWindow;
	public GameObject window_boneEditor;

	private byte[] OriginalXfbin;
	private byte[] OriginalNDP3;
	private int NDP3Index;

	private bool wasObjImported = false;
	public bool stageMode = false;
	int textureType = 0;

    void Start()
    {
		RenderedMesh = GameObject.Find("RENDERED MESH");
		mf = RenderedMesh.GetComponent<MeshFilter>();
		mr = RenderedMesh.GetComponent<MeshRenderer>();
		mf.mesh = new Mesh();
		mf.mesh.MarkDynamic();

		if(File.Exists(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg"))
		{
			SensitivityMouse = int.Parse(File.ReadAllText(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg"));
		}

		StartCoroutine(CheckForUpdates());
    }

    IEnumerator CheckForUpdates()
    {
		WWW www = new WWW("https://pastebin.com/raw/AFmJNVdC");
		yield return www;
		if(www.text != "Beta 1.0")
		{
			ConsoleMessage("\n<color=yellow>There's a new version available! Do /github to open the download page.</color>");
		}
    }

    public void OpenModelFromUnsmf(int VertexLength_, byte[] TriangleFile, byte[] TextureFile, byte[] VertexFile, bool stage)
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

			byteLenght = VertexLength_;
			fileBytes = VertexFile;
			triangleFile = TriangleFile;
			textureMapFile = TextureFile;

			for (int x = 0; x < (fileBytes.Length / byteLenght); x++)
		    {
				float vertexFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[0 + vertexOffset + (x * byteLenght)] * 16777215 + fileBytes[1 + vertexOffset + (x * byteLenght)] * 65535 + fileBytes[2 + vertexOffset + (x * byteLenght)] * 255 + fileBytes[3 + vertexOffset + (x * byteLenght)]), 0);
				float vertexFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[4 + vertexOffset + (x * byteLenght)] * 16777215 + fileBytes[5 + vertexOffset + (x * byteLenght)] * 65535 + fileBytes[6 + vertexOffset + (x * byteLenght)] * 255 + fileBytes[7 + vertexOffset + (x * byteLenght)]), 0);
				float vertexFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[8 + vertexOffset + (x * byteLenght)] * 16777215 + fileBytes[9 + vertexOffset + (x * byteLenght)] * 65535 + fileBytes[10 + vertexOffset + (x * byteLenght)] * 255 + fileBytes[11 + vertexOffset + (x * byteLenght)]), 0);

				if(stageMode == true)
				{
					vertexFloatX = vertexFloatX / 20;
					vertexFloatZ = vertexFloatZ / 20;
					vertexFloatY = vertexFloatY / 20;
				}

				vertexPosition.Add(new Vector3(vertexFloatX, vertexFloatY, vertexFloatZ));

				if(byteLenght == 64)
				{
					float normalFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 0 + (x * byteLenght)] * 16777215 + fileBytes[16 + vertexOffset + 1 + (x * byteLenght)] * 65535 + fileBytes[16 + vertexOffset + 2 + (x * byteLenght)] * 255 + fileBytes[16 + vertexOffset + 3 + (x * byteLenght)]), 0);
					float normalFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 4 + (x * byteLenght)] * 16777215 + fileBytes[16 + vertexOffset + 5 + (x * byteLenght)] * 65535 + fileBytes[16 + vertexOffset + 6 + (x * byteLenght)] * 255 + fileBytes[16 + vertexOffset + 7 + (x * byteLenght)]), 0);
					float normalFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 8 + (x * byteLenght)] * 16777215 + fileBytes[16 + vertexOffset + 9 + (x * byteLenght)] * 65535 + fileBytes[16 + vertexOffset + 10 + (x * byteLenght)] * 255 + fileBytes[16 + vertexOffset + 11 + (x * byteLenght)]), 0);
					meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));

					vertexBone.Add(new Vector3((float)fileBytes[35 + vertexOffset + (x * byteLenght)], (float)fileBytes[39 + vertexOffset + (x * byteLenght)], (float)fileBytes[43 + vertexOffset + (x * byteLenght)]));
				}
				else if(byteLenght == 28)
				{
					float normalFloatX = toFloat(fileBytes[12 + (x * byteLenght)] * 256 + fileBytes[13 + (x * byteLenght)]);
					float normalFloatY = toFloat(fileBytes[14 + (x * byteLenght)] * 256 + fileBytes[15 + (x * byteLenght)]);
					float normalFloatZ = toFloat(fileBytes[16 + (x * byteLenght)] * 256 + fileBytes[17 + (x * byteLenght)]);
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
						num[0] = triangleFile[q] * 256 + triangleFile[q + 1];
						num[1] = triangleFile[q + 2] * 256 + triangleFile[q + 3];
						num[2] = triangleFile[q + 4] * 256 + triangleFile[q + 5];
						q = q + 2;
						a++;

						if(meshVertices.Count >= num[0] && meshVertices.Count >= num[1] && meshVertices.Count >= num[2])
						{
							meshTriangles.Add(num[0]);
							meshTriangles.Add(num[1]);
							meshTriangles.Add(num[2]);

							meshTriangles.Add(num[2]);
							meshTriangles.Add(num[1]);
							meshTriangles.Add(num[0]);

							mf.mesh.triangles = meshTriangles.ToArray();
						}
					}
					else
					{
						q = q + 2;
					}
				}
			}

			if(textureMapFile.Length > 1)
			{
				int x = 4;
				while(textureMapFile[x] != 255)
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

			if(byteLenght == 64)
			{
				if(textureMapFile.Length > 1)
				{
					if(textureType == 0)
					{
						for(int x = 0; x < VertexCount; x++)
						{
							float x_ = toFloat(textureMapFile[4 + (8 * x)] * 256 + textureMapFile[5 + (8 * x)]);
							float y_ = toFloat(textureMapFile[6 + (8 * x)] * 256 + textureMapFile[7 + (8 * x)]);

							TextureUVs.Add(new Vector2(x_, y_));
						}
					}
					else if(textureType == 1)
					{
						for(int x = 0; x < VertexCount; x++)
						{
							float x_ = toFloat(textureMapFile[4 + (12 * x)] * 256 + textureMapFile[5 + (12 * x)]);
							float y_ = toFloat(textureMapFile[6 + (12 * x)] * 256 + textureMapFile[7 + (12 * x)]);

							TextureUVs.Add(new Vector2(x_, y_));
						}
					}
				}
			}
			else if(byteLenght == 28)
			{
				for(int x = 0; x < VertexCount; x++)
				{
					float x_ = toFloat(fileBytes[x * 24] * 256 + fileBytes[x * 25]);
					float y_ = toFloat(fileBytes[x * 26] * 256 + fileBytes[x * 27]);

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
			ConsoleMessage("<color=lime>MODEL LOADED.</color>");
		}
    }

    public void OpenModelFromXfbin(int VertexLength_, byte[] XfbinBytes, byte[] NDP3Bytes, int NDP3Index_, byte[] TriangleFile, byte[] TextureFile, byte[] VertexFile)
    {
		if(fileOpen == false)
		{
			OriginalXfbin = XfbinBytes;
			OriginalNDP3 = NDP3Bytes;
			NDP3Index = NDP3Index_;

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

			fileBytes = VertexFile;
			triangleFile = TriangleFile;
			textureMapFile = TextureFile;
			byteLenght = VertexLength_;

			for (int x = 0; x < (fileBytes.Length / byteLenght); x++)
		    {
				float vertexFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[0 + vertexOffset + (x * byteLenght)] * 16777215 + fileBytes[1 + vertexOffset + (x * byteLenght)] * 65535 + fileBytes[2 + vertexOffset + (x * byteLenght)] * 255 + fileBytes[3 + vertexOffset + (x * byteLenght)]), 0);
				float vertexFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[4 + vertexOffset + (x * byteLenght)] * 16777215 + fileBytes[5 + vertexOffset + (x * byteLenght)] * 65535 + fileBytes[6 + vertexOffset + (x * byteLenght)] * 255 + fileBytes[7 + vertexOffset + (x * byteLenght)]), 0);
				float vertexFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[8 + vertexOffset + (x * byteLenght)] * 16777215 + fileBytes[9 + vertexOffset + (x * byteLenght)] * 65535 + fileBytes[10 + vertexOffset + (x * byteLenght)] * 255 + fileBytes[11 + vertexOffset + (x * byteLenght)]), 0);

				if(stageMode == true)
				{
					vertexFloatX = vertexFloatX / 20;
					vertexFloatZ = vertexFloatZ / 20;
					vertexFloatY = vertexFloatY / 20;
				}

				vertexPosition.Add(new Vector3(vertexFloatX, vertexFloatY, vertexFloatZ));

				if(byteLenght == 64)
				{
					float normalFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 0 + (x * byteLenght)] * 16777215 + fileBytes[16 + vertexOffset + 1 + (x * byteLenght)] * 65535 + fileBytes[16 + vertexOffset + 2 + (x * byteLenght)] * 255 + fileBytes[16 + vertexOffset + 3 + (x * byteLenght)]), 0);
					float normalFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 4 + (x * byteLenght)] * 16777215 + fileBytes[16 + vertexOffset + 5 + (x * byteLenght)] * 65535 + fileBytes[16 + vertexOffset + 6 + (x * byteLenght)] * 255 + fileBytes[16 + vertexOffset + 7 + (x * byteLenght)]), 0);
					float normalFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 8 + (x * byteLenght)] * 16777215 + fileBytes[16 + vertexOffset + 9 + (x * byteLenght)] * 65535 + fileBytes[16 + vertexOffset + 10 + (x * byteLenght)] * 255 + fileBytes[16 + vertexOffset + 11 + (x * byteLenght)]), 0);
					meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));

					vertexBone.Add(new Vector3((float)fileBytes[35 + vertexOffset + (x * byteLenght)], (float)fileBytes[39 + vertexOffset + (x * byteLenght)], (float)fileBytes[43 + vertexOffset + (x * byteLenght)]));
				}
				else if(byteLenght == 28)
				{
					float normalFloatX = toFloat(fileBytes[12 + (x * byteLenght)] * 256 + fileBytes[13 + (x * byteLenght)]);
					float normalFloatY = toFloat(fileBytes[14 + (x * byteLenght)] * 256 + fileBytes[15 + (x * byteLenght)]);
					float normalFloatZ = toFloat(fileBytes[16 + (x * byteLenght)] * 256 + fileBytes[17 + (x * byteLenght)]);
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
						num[0] = triangleFile[q] * 256 + triangleFile[q + 1];
						num[1] = triangleFile[q + 2] * 256 + triangleFile[q + 3];
						num[2] = triangleFile[q + 4] * 256 + triangleFile[q + 5];
						q = q + 2;
						a++;

						if(meshVertices.Count >= num[0] && meshVertices.Count >= num[1] && meshVertices.Count >= num[2])
						{
							meshTriangles.Add(num[0]);
							meshTriangles.Add(num[1]);
							meshTriangles.Add(num[2]);

							meshTriangles.Add(num[2]);
							meshTriangles.Add(num[1]);
							meshTriangles.Add(num[0]);

							mf.mesh.triangles = meshTriangles.ToArray();
						}
					}
					else
					{
						q = q + 2;
					}
				}
			}

			if(textureMapFile.Length > 1)
			{
				int x = 4;
				while(textureMapFile[x] != 255)
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

			if(byteLenght == 64)
			{
				if(textureMapFile.Length > 1)
				{
					if(textureType == 0)
					{
						for(int x = 0; x < VertexCount; x++)
						{
							float x_ = toFloat(textureMapFile[4 + (8 * x)] * 256 + textureMapFile[5 + (8 * x)]);
							float y_ = toFloat(textureMapFile[6 + (8 * x)] * 256 + textureMapFile[7 + (8 * x)]);

							TextureUVs.Add(new Vector2(x_, y_));
						}
					}
					else if(textureType == 1)
					{
						for(int x = 0; x < VertexCount; x++)
						{
							float x_ = toFloat(textureMapFile[4 + (12 * x)] * 256 + textureMapFile[5 + (12 * x)]);
							float y_ = toFloat(textureMapFile[6 + (12 * x)] * 256 + textureMapFile[7 + (12 * x)]);

							TextureUVs.Add(new Vector2(x_, y_));
						}
					}
				}
			}
			else if(byteLenght == 28)
			{
				for(int x = 0; x < VertexCount; x++)
				{
					float x_ = toFloat(fileBytes[x * 24] * 256 + fileBytes[x * 25]);
					float y_ = toFloat(fileBytes[x * 26] * 256 + fileBytes[x * 27]);

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
			ConsoleMessage("<color=lime>MODEL LOADED.</color>");
		}
    }

	public void OpenModelFile(string path__) {
		if(fileOpen == false)
		{
			PathToModel = path__;
			if(File.Exists(PathToModel + "\\modelVertices.unsmf") && File.Exists(PathToModel + "\\modelVertexLenght.unsmf"))
			{
				fileBytes = File.ReadAllBytes(PathToModel + "\\modelVertices.unsmf");
				byteLenght = int.Parse(File.ReadAllText(PathToModel + "\\modelVertexLenght.unsmf"));

				if(File.Exists(PathToModel + "\\modelTriangles.unsmf"))
				{
					triangleFile = File.ReadAllBytes(PathToModel + "\\modelTriangles.unsmf");
				}

				if(File.Exists(PathToModel + "\\modelTextureCoords.unsmf") && byteLenght == 64)
				{
					textureMapFile = File.ReadAllBytes(PathToModel + "\\modelTextureCoords.unsmf");
				}
				//OpenModelFromXfbin(byteLenght, triangleFile, textureMapFile, fileBytes);
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

	void Update () {
		if(CommandInput.isFocused == true || fileOpen == false)
		{
			canMove = false;
		}
		else
		{
			canMove = true;
		}
		if(canMove && inCommand == false)
		{
			if(Input.GetKeyDown(KeyCode.LeftShift))
			{
				CameraSpeed = 200f;
			}
			if(Input.GetKeyUp(KeyCode.LeftShift))
			{
				CameraSpeed = 100f;
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0f )
			{
				cameraObject.transform.position = cameraObject.transform.position + mainCamera.transform.forward * 5 * CameraSpeed * Time.deltaTime;
 			}
 			else if (Input.GetAxis("Mouse ScrollWheel") < 0f )
			{
				cameraObject.transform.position = cameraObject.transform.position - mainCamera.transform.forward * 5 * CameraSpeed * Time.deltaTime;
 			}
			if(Input.GetKey(KeyCode.W))
			{
				cameraObject.transform.position = cameraObject.transform.position + mainCamera.transform.forward * CameraSpeed * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.S))
			{
				cameraObject.transform.position = cameraObject.transform.position - mainCamera.transform.forward * CameraSpeed * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.A))
			{
				cameraObject.transform.position = cameraObject.transform.position - mainCamera.transform.right * CameraSpeed * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.D))
			{
				cameraObject.transform.position = cameraObject.transform.position + mainCamera.transform.right * CameraSpeed * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.X))
			{
				cameraObject.transform.position = cameraObject.transform.position + Vector3.up * CameraSpeed * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.C))
			{
				cameraObject.transform.position = cameraObject.transform.position - Vector3.up * CameraSpeed * Time.deltaTime;
			}
		
			if(camRot.x > 90)
			{
				camRot.x = 90;
			}
			if(camRot.x < -90)
			{
				camRot.x = -90;
			}

			if(Input.GetMouseButton(1))
			{
				UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				UnityEngine.Cursor.lockState = CursorLockMode.None;
			}

			if(Input.GetAxisRaw("Mouse Y") > 0 && Input.GetMouseButton(1))
			{
				camRot.x = camRot.x - SensitivityMouse * Time.deltaTime;
			}
			if(Input.GetAxisRaw("Mouse Y") < 0 && Input.GetMouseButton(1))
			{
				camRot.x = camRot.x + SensitivityMouse * Time.deltaTime;
			}
			if(Input.GetAxisRaw("Mouse X") < 0 && Input.GetMouseButton(1))
			{
				camRot.y = camRot.y - SensitivityMouse * Time.deltaTime;
			}
			if(Input.GetAxisRaw("Mouse X") > 0 && Input.GetMouseButton(1))
			{
				camRot.y = camRot.y + SensitivityMouse * Time.deltaTime;
			}

			mainCamera.transform.rotation = Quaternion.Euler(mainCamera.transform.rotation.x + camRot.x, mainCamera.transform.rotation.y + camRot.y, mainCamera.transform.rotation.z + camRot.z);

			if(Input.GetKey(KeyCode.RightArrow))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position + mainCamera.transform.right / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.LeftArrow))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position - mainCamera.transform.right / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.UpArrow))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position + mainCamera.transform.forward / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.DownArrow))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position - mainCamera.transform.forward / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.O))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position + mainCamera.transform.up / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}
			if(Input.GetKey(KeyCode.L))
			{
				for(int x = 0; x < selectedVertex.Count; x++)
				{
					GameObject vertex_ = selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position - mainCamera.transform.up / 10;
					meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
					mf.mesh.vertices = meshVertices.ToArray();
				}
			}

			Arrows.transform.rotation = Quaternion.Euler(Vector3.forward);

			modelInformation.text = selectedVertex.ToArray().Length.ToString() + " vertex selected";
		}

		float spheresize;

		if(GameObject.Find("SizeOfSphere").GetComponent<Text>().text != "")
		{
			try
			{
				spheresize = float.Parse(GameObject.Find("SizeOfSphere").GetComponent<Text>().text);
			}
			catch(Exception)
			{
				spheresize = 1;
				GameObject.Find("SizeOfSphere").GetComponent<Text>().text = 1.ToString();
				ConsoleMessage("\n<color=red> Incorrect sphere size.</color>");
			}
		}
		else
		{
			spheresize = 1;
			GameObject.Find("SizeOfSphere").GetComponent<Text>().text = 1.ToString();
		}
		SphereTest.transform.localScale = new Vector3(spheresize, spheresize, spheresize);

		if(Input.GetMouseButtonUp(0))
		{
			SphereTest.transform.position = new Vector3(99999, 99999, 99999);
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
		invertTrianglesBool = !invertTrianglesBool;
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

	public void SaveXfbin()
	{
		SaveFileDialog saveFileDialog1 = new SaveFileDialog();
		saveFileDialog1.DefaultExt = ".obj";

		saveFileDialog1.ShowDialog();

		if(saveFileDialog1.FileName != "")
		{
			SaveModelToXfbin(saveFileDialog1.FileName);
		}
	}

	public void SaveModelToXfbin(string path__)
	{
		Transform ModelDataTransform;
		ModelDataTransform = GameObject.Find("Model Data").GetComponent<Transform>();

		List<byte> triangleFileNew = new List<byte>();
		List<byte> vertexFileNew = new List<byte>();
		List<byte> textureFileNew = new List<byte>();

		if(GameObject.Find("Save Vertices").GetComponent<Toggle>().isOn == true)
		{
			if(GameObject.Find("Model Data").transform.childCount * byteLenght < fileBytes.Length)
			{
				for(int x = 0; x < fileBytes.Length; x++)
				{
					vertexFileNew.Add(fileBytes[x]);
				}
			}
			else
			{
				for(int x = 0; x < GameObject.Find("Model Data").transform.childCount; x++)
				{
					for(int z = 0; z < byteLenght; z++)
					{
						vertexFileNew.Add(fileBytes[z]);
					}
				}
			}

			for(int x = 0; x < GameObject.Find("Model Data").transform.childCount; x++)
			{
				byte[] posx = BitConverter.GetBytes(GameObject.Find("Model Data").transform.Find(x.ToString()).transform.position.x).ToArray();
				if(stageMode == true) posx = BitConverter.GetBytes(GameObject.Find("Model Data").transform.Find(x.ToString()).transform.position.x * 20).ToArray();
				Array.Reverse(posx);
				vertexFileNew[0 + (byteLenght * x)] = posx[0];
				vertexFileNew[1 + (byteLenght * x)] = posx[1];
				vertexFileNew[2 + (byteLenght * x)] = posx[2];
				vertexFileNew[3 + (byteLenght * x)] = posx[3];

				byte[] posz = BitConverter.GetBytes(GameObject.Find("Model Data").transform.Find(x.ToString()).transform.position.z).ToArray();
				if(stageMode == true) posz = BitConverter.GetBytes(GameObject.Find("Model Data").transform.Find(x.ToString()).transform.position.z * 20).ToArray();
				Array.Reverse(posz);
				vertexFileNew[4 + (byteLenght * x)] = posz[0];
				vertexFileNew[5 + (byteLenght * x)] = posz[1];
				vertexFileNew[6 + (byteLenght * x)] = posz[2];
				vertexFileNew[7 + (byteLenght * x)] = posz[3];

				byte[] posy = BitConverter.GetBytes(GameObject.Find("Model Data").transform.Find(x.ToString()).transform.position.y).ToArray();
				if(stageMode == true) posy = BitConverter.GetBytes(GameObject.Find("Model Data").transform.Find(x.ToString()).transform.position.y * 20).ToArray();
				Array.Reverse(posy);
				vertexFileNew[8 + (byteLenght * x)] = posy[0];
				vertexFileNew[9 + (byteLenght * x)] = posy[1];
				vertexFileNew[10 + (byteLenght * x)] = posy[2];
				vertexFileNew[11 + (byteLenght * x)] = posy[3];

				if(byteLenght == 28)
				{
					byte[] NXA = ToInt(mf.mesh.normals[x].x);
					Array.Reverse(NXA);

					byte[] NZA = ToInt(mf.mesh.normals[x].z);
					Array.Reverse(NZA);

					byte[] NYA = ToInt(mf.mesh.normals[x].y);
					Array.Reverse(NYA);

					vertexFileNew[12 + (byteLenght * x)] = NXA[0];
					vertexFileNew[13 + (byteLenght * x)] = NXA[1];

					vertexFileNew[14 + (byteLenght * x)] = NZA[0];
					vertexFileNew[15 + (byteLenght * x)] = NZA[1];

					vertexFileNew[16 + (byteLenght * x)] = NYA[0];
					vertexFileNew[17 + (byteLenght * x)] = NYA[1];
				}
				if(byteLenght == 64)
				{
					// BONE DATA
					vertexFileNew[35 + (byteLenght * x)] = (byte)vertexBone[x].x;
					vertexFileNew[39 + (byteLenght * x)] = (byte)vertexBone[x].y;
					vertexFileNew[43 + (byteLenght * x)] = (byte)vertexBone[x].z;

					// NORMALS
					byte[] normalx = BitConverter.GetBytes(mf.mesh.normals[x].x).ToArray();
					Array.Reverse(normalx);

					vertexFileNew[16 + (byteLenght * x)] = normalx[0];
					vertexFileNew[17 + (byteLenght * x)] = normalx[1];
					vertexFileNew[18 + (byteLenght * x)] = normalx[2];
					vertexFileNew[19 + (byteLenght * x)] = normalx[3];

					byte[] normalz = BitConverter.GetBytes(mf.mesh.normals[x].z).ToArray();
					Array.Reverse(normalz);

					vertexFileNew[20 + (byteLenght * x)] = normalz[0];
					vertexFileNew[21 + (byteLenght * x)] = normalz[1];
					vertexFileNew[22 + (byteLenght * x)] = normalz[2];
					vertexFileNew[23 + (byteLenght * x)] = normalz[3];

					byte[] normaly = BitConverter.GetBytes(mf.mesh.normals[x].y).ToArray();
					Array.Reverse(normaly);

					vertexFileNew[24 + (byteLenght * x)] = normaly[0];
					vertexFileNew[25 + (byteLenght * x)] = normaly[1];
					vertexFileNew[26 + (byteLenght * x)] = normaly[2];
					vertexFileNew[27 + (byteLenght * x)] = normaly[3];
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

		if(GameObject.Find("Save UV").GetComponent<Toggle>().isOn == true)
		{
			if(GameObject.Find("Save Vertices").GetComponent<Toggle>().isOn == true || GameObject.Find("Model Data").transform.childCount <= VertexCount)
			{
				for(int x = 0; x < mf.mesh.uv.Length; x++)
				{
					if(byteLenght == 28)
					{
						byte[] UVXA = ToInt(mf.mesh.uv[x].x);
						Array.Reverse(UVXA);

						byte[] UVYA = ToInt(mf.mesh.uv[x].y);
						Array.Reverse(UVYA);

						vertexFileNew[24 + (byteLenght * x)] = UVXA[0];
						vertexFileNew[25 + (byteLenght * x)] = UVXA[1];

						vertexFileNew[26 + (byteLenght * x)] = UVYA[0];
						vertexFileNew[27 + (byteLenght * x)] = UVYA[1];
					}
					else if(byteLenght == 64)
					{
						if(textureType == 0)
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
						else if(textureType == 1)
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
			if(byteLenght == 64)
			{
				for(int x = 0; x < textureMapFile.Length; x++)
				{
					textureFileNew.Add(textureMapFile[x]);
				}
			}
		}

		if(GameObject.Find("Save Triangles").GetComponent<Toggle>().isOn == true)
		{
			for(int q = 0; q <= this.mf.mesh.triangles.Length - 6; q += 6)
			{
				byte[] tri1 = BitConverter.GetBytes(mf.mesh.triangles[q + 0]).ToArray();
				byte[] tri2 = BitConverter.GetBytes(mf.mesh.triangles[q + 1]).ToArray();
				byte[] tri3 = BitConverter.GetBytes(mf.mesh.triangles[q + 2]).ToArray();

				if(invertTrianglesBool == false)
				{
					triangleFileNew.Add(tri1[1]);
					triangleFileNew.Add(tri1[0]);
					triangleFileNew.Add(tri2[1]);
					triangleFileNew.Add(tri2[0]);
					triangleFileNew.Add(tri3[1]);
					triangleFileNew.Add(tri3[0]);
				}
				else
				{
					triangleFileNew.Add(tri3[1]);
					triangleFileNew.Add(tri3[0]);
					triangleFileNew.Add(tri2[1]);
					triangleFileNew.Add(tri2[0]);
					triangleFileNew.Add(tri1[1]);
					triangleFileNew.Add(tri1[0]);
				}

				triangleFileNew.Add(255);
				triangleFileNew.Add(255);
			}

			if(triangleFileNew.Count < triangleFile.Length)
			{
				while(triangleFileNew.ToArray().Length < triangleFile.Length)
				{
					triangleFileNew.Add(0);
				}
			}
		}
		else
		{
			for(int x = 0; x < triangleFile.Length; x++)
			{
				triangleFileNew.Add(triangleFile[x]);
			}
		}

		int OriginalNDP3Size = OriginalNDP3[4] * 16777216 + OriginalNDP3[5] * 65536 + OriginalNDP3[6] * 256 + OriginalNDP3[7];
		int SizeBeforeNDP3Index = 0;
		int sizeMode = 0;

		int x_ = 0;
		while(OriginalXfbin[NDP3Index - 4 + x_] * 16777216 + OriginalXfbin[NDP3Index - 3 + x_] * 65536 + OriginalXfbin[NDP3Index - 2 + x_] * 256 + OriginalXfbin[NDP3Index - 1 + x_] != OriginalNDP3Size)
		{
			x_--;
		}

		SizeBeforeNDP3Index = x_ - 4;

		List<byte> newNDP3File = new List<byte>();
		for(int x = 0; x < 16; x++)
		{
			newNDP3File.Add(OriginalNDP3[x]);
		}

		//Add first section size
		for(int x = 0; x < 4; x++)
		{
			newNDP3File.Add(OriginalNDP3[16 + x]);
		}
		int firstSectionSize_ = newNDP3File[16] * 16777216 + newNDP3File[17] * 65536 + newNDP3File[18] * 256 + newNDP3File[19];

		//Add triangle section size
		int TriangleSectionSize = triangleFileNew.Count;
		if(TriangleSectionSize <= 255)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(TriangleSectionSize);
			newNDP3File.Add(0);
			newNDP3File.Add(0);
			newNDP3File.Add(0);
			newNDP3File.Add(arrayOfSize[0]);
		}
		else if(TriangleSectionSize > 255 && TriangleSectionSize <= 65535)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(TriangleSectionSize);
			newNDP3File.Add(0);
			newNDP3File.Add(0);
			newNDP3File.Add(arrayOfSize[1]);
			newNDP3File.Add(arrayOfSize[0]);
		}
		else if(TriangleSectionSize > 65535 && TriangleSectionSize <= 16777215)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(TriangleSectionSize);
			newNDP3File.Add(0);
			newNDP3File.Add(arrayOfSize[2]);
			newNDP3File.Add(arrayOfSize[1]);
			newNDP3File.Add(arrayOfSize[0]);
		}

		//Check vertex lenght
		if(byteLenght == 64)
		{
			//Add texture section size
			int TextureSectionSize = textureFileNew.Count;
			if(TextureSectionSize <= 255)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(TextureSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(TextureSectionSize > 255 && TextureSectionSize <= 65535)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(TextureSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[1]);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(TextureSectionSize > 65535 && TextureSectionSize <= 16777215)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(TextureSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[2]);
				newNDP3File.Add(arrayOfSize[1]);
				newNDP3File.Add(arrayOfSize[0]);
			}
			//Add vertex section size
			int VertexSectionSize = vertexFileNew.Count;
			if(VertexSectionSize <= 255)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(VertexSectionSize > 255 && VertexSectionSize <= 65535)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[1]);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(VertexSectionSize > 65535 && VertexSectionSize <= 16777215)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[2]);
				newNDP3File.Add(arrayOfSize[1]);
				newNDP3File.Add(arrayOfSize[0]);
			}
		}
		else
		{
			//Add vertex section size
			int VertexSectionSize = vertexFileNew.Count;
			if(VertexSectionSize <= 255)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(VertexSectionSize > 255 && VertexSectionSize <= 65535)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
				newNDP3File.Add(0);
				newNDP3File.Add(0);
				newNDP3File.Add(arrayOfSize[1]);
				newNDP3File.Add(arrayOfSize[0]);
			}
			else if(VertexSectionSize > 65535 && VertexSectionSize <= 16777215)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(VertexSectionSize);
				newNDP3File.Add(0);
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
		for(int x = 32; x < 48; x++)
		{
			newNDP3File.Add(OriginalNDP3[x]);
		}

		// Add first section
		for(int x = 0; x < firstSectionSize_; x++)
		{
			newNDP3File.Add(OriginalNDP3[48 + x]);
		}

		// Add triangle section
		for(int x = 0; x < triangleFileNew.Count; x++)
		{
			newNDP3File.Add(triangleFileNew[x]);
		}

		if(byteLenght == 64)
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
		else
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

		//MessageBox.Show(SizeBeforeNDP3Index.ToString());

		// Add new size before NDP3
		if(newNDP3Size <= 255)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

			newXfbinFile[NDP3Index + SizeBeforeNDP3Index] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 1] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 2] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
		}
		else if(newNDP3Size > 255 && newNDP3Size <= 65535)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

			newXfbinFile[NDP3Index + SizeBeforeNDP3Index] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 1] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 2] = arrayOfSize[1];
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
		}
		else if(newNDP3Size > 65535 && newNDP3Size <= 16777215)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

			newXfbinFile[NDP3Index + SizeBeforeNDP3Index] = 0;
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
		if(newNDP3Size <= 255)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 4] = 0;
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 5] = 0;
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 6] = 0;
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 7] = arrayOfSize[0];
		}
		else if(newNDP3Size > 255 && newNDP3Size <= 65535)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 4] = 0;
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 5] = 0;
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 6] = arrayOfSize[1];
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 7] = arrayOfSize[0];
		}
		else if(newNDP3Size > 65535 && newNDP3Size <= 16777215)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 4] = 0;
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 5] = arrayOfSize[2];
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 6] = arrayOfSize[1];
			newXfbinFile[newXfbinFile.Count - newNDP3File.Count + 7] = arrayOfSize[0];
		}

		// Copy original name and rest of xfbin 
		for(int x = NDP3Index + (48 + firstSectionSize_ + triangleFile.Length + textureMapFile.Length + fileBytes.Length); x < OriginalXfbin.Length; x++)
		{
			newXfbinFile.Add(OriginalXfbin[x]);
		}

		// Fix new NDP3 size 36
		int DifferenceBetweenSizes = (OriginalXfbin[NDP3Index + SizeBeforeNDP3Index - 36] * 16777216 + OriginalXfbin[NDP3Index + SizeBeforeNDP3Index - 35] * 65536 + OriginalXfbin[NDP3Index + SizeBeforeNDP3Index - 34] * 256 + OriginalXfbin[NDP3Index + SizeBeforeNDP3Index - 33]) - OriginalNDP3Size;
		if(newNDP3Size + DifferenceBetweenSizes <= 255)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 36] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 35] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 34] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 33] = arrayOfSize[0];
		}
		else if(newNDP3Size + DifferenceBetweenSizes > 255 && newNDP3Size + DifferenceBetweenSizes <= 65535)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 36] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 35] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 34] = arrayOfSize[1];
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 33] = arrayOfSize[0];
		}
		else if(newNDP3Size + DifferenceBetweenSizes > 65535 && newNDP3Size + DifferenceBetweenSizes <= 16777215)
		{
			byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 36] = 0;
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 35] = arrayOfSize[2];
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 34] = arrayOfSize[1];
			newXfbinFile[NDP3Index + SizeBeforeNDP3Index - 33] = arrayOfSize[0];
		}
		File.WriteAllBytes(path__, newXfbinFile.ToArray());

		MessageBox.Show(".xfbin file saved. If you added more vertices, you might have to fix the 80-byte vertex count for it to display correctly.");
	}

	public void SaveModel(string path__)
	{
		Transform ModelDataTransform;
		ModelDataTransform = GameObject.Find("Model Data").GetComponent<Transform>();

		List<byte> triangleFileNew = new List<byte>();
		List<byte> vertexFileNew = new List<byte>();
		List<byte> textureFileNew = new List<byte>();

		vertexFileNew = fileBytes.ToList();

		int test = vertexFileNew.ToArray().Length / byteLenght;
		for(int x = 0; x < GameObject.Find("Model Data").transform.childCount - test; x++)
		{
			for(int z = 0; z < byteLenght; z++)
			{
				vertexFileNew.Add(fileBytes[z]);
			}
		}

		for(int x = 0; x < GameObject.Find("Model Data").transform.childCount; x++)
		{
			byte[] posx = BitConverter.GetBytes(meshVertices[x].x).ToArray();
			if(stageMode == true) posx = BitConverter.GetBytes(meshVertices[x].x * 20).ToArray();
			Array.Reverse(posx);

			vertexFileNew[0 + (byteLenght * x)] = posx[0];
			vertexFileNew[1 + (byteLenght * x)] = posx[1];
			vertexFileNew[2 + (byteLenght * x)] = posx[2];
			vertexFileNew[3 + (byteLenght * x)] = posx[3];

			byte[] posz = BitConverter.GetBytes(meshVertices[x].z).ToArray();
			if(stageMode == true) posz = BitConverter.GetBytes(meshVertices[x].z * 20).ToArray();
			Array.Reverse(posz);
			vertexFileNew[4 + (byteLenght * x)] = posz[0];
			vertexFileNew[5 + (byteLenght * x)] = posz[1];
			vertexFileNew[6 + (byteLenght * x)] = posz[2];
			vertexFileNew[7 + (byteLenght * x)] = posz[3];

			byte[] posy = BitConverter.GetBytes(meshVertices[x].y).ToArray();
			if(stageMode == true) posy = BitConverter.GetBytes(meshVertices[x].y * 20).ToArray();
			Array.Reverse(posy);
			vertexFileNew[8 + (byteLenght * x)] = posy[0];
			vertexFileNew[9 + (byteLenght * x)] = posy[1];
			vertexFileNew[10 + (byteLenght * x)] = posy[2];
			vertexFileNew[11 + (byteLenght * x)] = posy[3];

			if(byteLenght == 28)
			{
				byte[] NXA = ToInt(mf.mesh.normals[x].x);
				Array.Reverse(NXA);

				byte[] NZA = ToInt(mf.mesh.normals[x].z);
				Array.Reverse(NZA);

				byte[] NYA = ToInt(mf.mesh.normals[x].y);
				Array.Reverse(NYA);

				vertexFileNew[12 + (byteLenght * x)] = NXA[0];
				vertexFileNew[13 + (byteLenght * x)] = NXA[1];

				vertexFileNew[14 + (byteLenght * x)] = NZA[0];
				vertexFileNew[15 + (byteLenght * x)] = NZA[1];

				vertexFileNew[16 + (byteLenght * x)] = NYA[0];
				vertexFileNew[17 + (byteLenght * x)] = NYA[1];
			}
			if(byteLenght == 64)
			{
				// BONE DATA
				vertexFileNew[35 + (byteLenght * x)] = (byte)vertexBone[x].x;
				vertexFileNew[39 + (byteLenght * x)] = (byte)vertexBone[x].y;
				vertexFileNew[43 + (byteLenght * x)] = (byte)vertexBone[x].z;

				// NORMALS
				byte[] normalx = BitConverter.GetBytes(mf.mesh.normals[x].x).ToArray();
				Array.Reverse(normalx);

				vertexFileNew[16 + (byteLenght * x)] = normalx[0];
				vertexFileNew[17 + (byteLenght * x)] = normalx[1];
				vertexFileNew[18 + (byteLenght * x)] = normalx[2];
				vertexFileNew[19 + (byteLenght * x)] = normalx[3];

				byte[] normalz = BitConverter.GetBytes(mf.mesh.normals[x].z).ToArray();
				Array.Reverse(normalz);

				vertexFileNew[20 + (byteLenght * x)] = normalz[0];
				vertexFileNew[21 + (byteLenght * x)] = normalz[1];
				vertexFileNew[22 + (byteLenght * x)] = normalz[2];
				vertexFileNew[23 + (byteLenght * x)] = normalz[3];

				byte[] normaly = BitConverter.GetBytes(mf.mesh.normals[x].y).ToArray();
				Array.Reverse(normaly);

				vertexFileNew[24 + (byteLenght * x)] = normaly[0];
				vertexFileNew[25 + (byteLenght * x)] = normaly[1];
				vertexFileNew[26 + (byteLenght * x)] = normaly[2];
				vertexFileNew[27 + (byteLenght * x)] = normaly[3];
			}
		}

		for(int x = 0; x < mf.mesh.uv.Length; x++)
		{
			if(byteLenght == 28)
			{
				byte[] UVXA = ToInt(mf.mesh.uv[x].x);
				Array.Reverse(UVXA);

				byte[] UVYA = ToInt(mf.mesh.uv[x].y);
				Array.Reverse(UVYA);

				vertexFileNew[24 + (byteLenght * x)] = UVXA[0];
				vertexFileNew[25 + (byteLenght * x)] = UVXA[1];

				vertexFileNew[26 + (byteLenght * x)] = UVYA[0];
				vertexFileNew[27 + (byteLenght * x)] = UVYA[1];
			}
			else if(byteLenght == 64)
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
		
		int q = 0;

		while(q <= mf.mesh.triangles.Length - 6)
		{
			byte[] tri1 = BitConverter.GetBytes(mf.mesh.triangles[q + 0]).ToArray();
			byte[] tri2 = BitConverter.GetBytes(mf.mesh.triangles[q + 1]).ToArray();
			byte[] tri3 = BitConverter.GetBytes(mf.mesh.triangles[q + 2]).ToArray();

			if(invertTrianglesBool == false)
			{
				triangleFileNew.Add(tri1[1]);
				triangleFileNew.Add(tri1[0]);
				triangleFileNew.Add(tri2[1]);
				triangleFileNew.Add(tri2[0]);
				triangleFileNew.Add(tri3[1]);
				triangleFileNew.Add(tri3[0]);
			}
			else
			{
				triangleFileNew.Add(tri3[1]);
				triangleFileNew.Add(tri3[0]);
				triangleFileNew.Add(tri2[1]);
				triangleFileNew.Add(tri2[0]);
				triangleFileNew.Add(tri1[1]);
				triangleFileNew.Add(tri1[0]);
			}

			triangleFileNew.Add(255);
			triangleFileNew.Add(255);

			q = q + 6;
		}

		try
		{
			if(Directory.Exists(path__) == false)
			{
				Directory.CreateDirectory(path__);
			}

			File.WriteAllBytes(path__ + "\\modelVertices.unsmf", vertexFileNew.ToArray());
			File.WriteAllText(path__ + "\\modelVertexLenght.unsmf", byteLenght.ToString());
			if(triangleFile.Length > 0)
			{
				File.WriteAllBytes(path__ + "\\modelTriangles.unsmf", triangleFileNew.ToArray());
			}
			if(byteLenght == 64)
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

	public void OpenToolBox()
	{
		if(fileOpen == true)
		{
			toolBoxWindow.SetActive(true);
			WindowOpen = true;
			ConsoleMessage("\n" + "<color=cyan>Tool box open.</color>");
		}
		else
		{
			ConsoleMessage("\n<color=red>You need to open a model first.</color>");
		}
	}

	public void DoCommand()
	{
		if(CommandInput.text != "")
		{
			string command = CommandInput.text;
			CommandInput.text = "";

			if(command == "/help")
			{
				ConsoleMessage("\n" + "Total help pages: 2. Do '/help 0' to go to page 0.");
			}
			else if(command == "/help 0")
			{
				ConsoleMessage("\nPAGE 0: " + "Write <color=yellow>/help /command</color> to get help on a specific command. Command List: <color=yellow>/saveunsmf /importobj /exporttoobj /translate /rotate /setposition /loadtexture</color>");
			}
			else if(command == "/help 1")
			{
				ConsoleMessage("\nPAGE 1: " + "Write <color=yellow>/help /command</color> to get help on a specific command. Command List: <color=yellow>/scale /lighting /renderset /closefile /selectall /github /inverttriangles /invertnormals</color>");
			}
			else if(command == "/github")
			{
				UnityEngine.Application.OpenURL("https://github.com/zealottormunds/unsme");
			}
			else if(command == "/help /importobj")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/importobj PATH\"</color> imports an .obj file");
			}
			else if(command == "/help /saveunsmf")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/saveunsmf\"</color> saves a model as .unsmf.");
			}
			else if(command == "/help /exportobj")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/exporttoobj PATH\"</color> exports a model to .obj.");
			}
			else if(command == "/help /translate")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/translate X Y Z\"</color> translates the selected vertexes. Example: /translate 0 2 0 will translate the vertices 2 points up.");
			}
			else if(command == "/help /setposition")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/setposition X Y Z\"</color> moves the selected vertices to the same point in worldspace. Example: /setposition % 2 % will set the vertices Y position to 2.");
			}
			else if(command == "/help /renderset")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/renderset I\"</color> changes the render mode. 0 = wireframe, 1 = textured.");
			}
			else if(command == "/help /lighting")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/lighting I\"</color> changes the state of lighting. 0 = disabled, 1 = enabled.");
			}
			else if(command == "/help /scale")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/scale X Y Z\"</color> scales the selected vertices using the center of the points as the origin. Negative magnitudes make the object smaller. Example: /scale 2 2 2");
			}			
			else if(command == "/help /rotate")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/rotate X Y Z\"</color> rotates the selected vertices according to a number of degrees. Example: /rotate 90 0 0.");
			}
			else if(command == "/help /selectall")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/selectall\"</color> selects all the vertices.");
			}
			else if(command == "/help /loadtexture")
			{
				ConsoleMessage("\n" + "<color=yellow>Command \"/loadtexture\"</color> loads a texture to the model.");
			}
			else if(command == "/selectall" && fileOpen)
			{
				for(int x = 0; x < GameObject.Find("Model Data").transform.childCount; x++)
				{
					GameObject vertex_ = GameObject.Find("Model Data").transform.Find(x.ToString()).gameObject;
					vertex_.gameObject.GetComponent<VertexObject>().SelectObject();
				}
				ConsoleMessage("\n" + "<color=yellow>Selected " + GameObject.Find("Model Data").transform.childCount.ToString() + " vertices.</color>");
			}
			else if(command.Length >= 10 && command.Substring(0, 10) == "/translate" && fileOpen)
			{
				try
				{
					int a = 0;
					int b = 0;
					int c = 0;

					char[] xT = new char[10];
					char[] yT = new char[10];
					char[] zT = new char[10];

					while(command[11 + a].ToString() != " ")
					{
						xT[a] = command[11 + a];
						a++;
					}

					a++;
					while(command[11 + a].ToString() != " ")
					{
						yT[b] = command[11 + a];
						b++;
						a++;
					}

					a++;
					while(11 + a < command.Length && command[11 + a].ToString() != " ")
					{
						zT[c] = command[11 + a];
						c++;
						a++;
					}

					float X_ = float.Parse(new string(xT));
					float Y_ = float.Parse(new string(yT));
					float Z_ = float.Parse(new string(zT));

					if(selectedVertex.ToArray().Length == 0)
					{
						ConsoleMessage("\n" + "<color=orange>No vertices selected.</color>");
					}
					else
					{
						for(int x = 0; x < selectedVertex.Count; x++)
						{
							GameObject vertex_ = selectedVertex[x];
							vertex_.transform.position = vertex_.transform.position + new Vector3(X_, Y_, Z_);
							meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
						}
						mf.mesh.vertices = meshVertices.ToArray();
						ConsoleMessage("\n" + "<color=yellow>Translated " + selectedVertex.ToArray().Length + " vertices in " + X_.ToString() + ", " + Y_.ToString() + ", " + Z_.ToString() + ".</color>");
					}
				}
				catch(Exception)
				{
					ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
				}
			}
			else if(command == "/invertnormals" && fileOpen == true)
			{
				try
				{
					InvertNormals();
				}
				catch(Exception)
				{
					ConsoleMessage("\n<color=orange>Error inverting mesh normals.</color>");
				}
			}
			else if(command == "/inverttriangles" && fileOpen == true)
			{
				try
				{
					InvertTriangles();
				}
				catch(Exception)
				{
					ConsoleMessage("\n<color=orange>Error inverting mesh triangles.</color>");
				}
			}
			else if(command.Length >= 12 && command.Substring(0, 12) == "/sensitivity")
			{
				try
				{
					int a = 0;
					int b = 0;
					int c = 0;

					char[] xT = new char[10];

					while(13 + a < command.Length && command[13 + a].ToString() != " ")
					{
						xT[c] = command[13 + a];
						c++;
						a++;
					}

					float X_ = float.Parse(new string(xT));

					SensitivityMouse = X_;
					string CFG_Sensitivity = SensitivityMouse.ToString();
					ConsoleMessage("\n" + "<color=lime>Saved sensitivity settings.</color>");
					File.WriteAllText(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg", CFG_Sensitivity);
				}
				catch(Exception)
				{
					ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
				}
			}
			else if(command.Substring(0, 10) == "/exportobj" && fileOpen)
			{
				if(command.Length > 11)
				{
					List<char> exportPath = new List<char>();

					for(int x = 11; x < command.Length; x++)
					{
						exportPath.Add(command[x]);
					}

					if(new string(exportPath.ToArray()) != "")
					{
						ExportToObj(new string(exportPath.ToArray()));
					}
					else
					{
						ConsoleMessage("\nError exporting model.");
					}
				}
				else
				{
					SaveFileDialog saveFileDialog1 = new SaveFileDialog();
					saveFileDialog1.DefaultExt = ".obj";

					saveFileDialog1.ShowDialog();

					if(saveFileDialog1.FileName != "")
					{
						ExportToObj(saveFileDialog1.FileName);
					}
					else
					{
						ConsoleMessage("\nError exporting model.");
					}
				}
			}
			else if(command.Substring(0, 10) == "/importobj" && fileOpen)
			{
				if(command.Length > 11)
				{
					List<char> importPath = new List<char>();

					for(int x = 11; x < command.Length; x++)
					{
						importPath.Add(command[x]);
					}

					if(new string(importPath.ToArray()) != "")
					{
						ImportModel(new string(importPath.ToArray()));
						wasObjImported = true;
					}
					else
					{
						ConsoleMessage("\nError importing model.");
					}
				}
				else
				{
					OpenFileDialog openFileDialog1 = new OpenFileDialog();
					openFileDialog1.DefaultExt = ".obj";

					openFileDialog1.ShowDialog();

					if(openFileDialog1.FileName != "" && File.Exists(openFileDialog1.FileName))
					{
						ImportModel(openFileDialog1.FileName);
						wasObjImported = true;
					}
					else
					{
						ConsoleMessage("\nError importing model.");
					}
				}
			}
			else if(command.Length >= 12 && command.Substring(0, 12) == "/loadtexture")
			{
				LoadTexture();
			}
			else if(command.Length >= 12 && command.Substring(0, 12) == "/setposition")
			{
				try
				{
					int a = 0;
					int b = 0;
					int c = 0;

					char[] xT = new char[10];
					char[] yT = new char[10];
					char[] zT = new char[10];

					while(command[13 + a].ToString() != " ")
					{
						xT[a] = command[13 + a];
						a++;
					}

					a++;
					while(command[13 + a].ToString() != " ")
					{
						yT[b] = command[13 + a];
						b++;
						a++;
					}

					a++;
					while(13 + a < command.Length && command[13 + a].ToString() != " ")
					{
						zT[c] = command[13 + a];
						c++;
						a++;
					}

					float X_;
					float Y_;
					float Z_;

					if(xT[0] != '%')
					{
						X_ = float.Parse(new string(xT));
					}
					else
					{
						X_ = 65535.65535f;
					}
					if(yT[0] != '%')
					{
						Y_ = float.Parse(new string(yT));
					}
					else
					{
						Y_ = 65535.65535f;
					}
					if(zT[0] != '%')
					{
						Z_ = float.Parse(new string(zT));
					}
					else
					{
						Z_ = 65535.65535f;
					}

					if(selectedVertex.ToArray().Length == 0)
					{
						ConsoleMessage("\n" + "<color=orange>No vertices selected.</color>");
					}
					else
					{
						for(int x = 0; x < selectedVertex.Count; x++)
						{
							GameObject vertex_ = selectedVertex[x];
							Vector3 newPos;
							if(X_ != 65535.65535f)
							{
								newPos.x = X_;
							}
							else
							{
								newPos.x = vertex_.transform.position.x;
							}
							if(Y_ != 65535.65535f)
							{
								newPos.y = Y_;
							}
							else
							{
								newPos.y = vertex_.transform.position.y;
							}
							if(Z_ != 65535.65535f)
							{
								newPos.z = Z_;
							}
							else
							{
								newPos.z = vertex_.transform.position.z;
							}
							vertex_.transform.position = newPos;
							meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
						}
						mf.mesh.vertices = meshVertices.ToArray();
						ConsoleMessage("\n" + "<color=yellow>Changed the position of " + selectedVertex.ToArray().Length + " vertices.</color>");
					}
				}
				catch(Exception)
				{
					ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
				}
			}
			else if(command.Length >= 6 && command.Substring(0, 6) == "/scale" && fileOpen)
			{
				try
				{
					int a = 0;
					int b = 0;
					int c = 0;

					char[] xT = new char[10];
					char[] yT = new char[10];
					char[] zT = new char[10];

					while(command[7 + a].ToString() != " ")
					{
						xT[a] = command[7 + a];
						a++;
					}

					a++;
					while(command[7 + a].ToString() != " ")
					{
						yT[b] = command[7 + a];
						b++;
						a++;
					}

					a++;
					while(7 + a < command.Length && command[7 + a].ToString() != " ")
					{
						zT[c] = command[7 + a];
						c++;
						a++;
					}

					float X_ = float.Parse(new string(xT));
					float Y_ = float.Parse(new string(yT));
					float Z_ = float.Parse(new string(zT));

					ScaleModel(X_, Y_, Z_);
				}
				catch(Exception)
				{
					ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
				}
			}
			else if(command.Length >= 7 && command.Substring(0, 7) == "/rotate" && fileOpen)
			{
				try
				{
					int a = 0;
					int b = 0;
					int c = 0;

					char[] xT = new char[10];
					char[] yT = new char[10];
					char[] zT = new char[10];

					while(command[8 + a].ToString() != " ")
					{
						xT[a] = command[8 + a];
						a++;
					}

					a++;
					while(command[8 + a].ToString() != " ")
					{
						yT[b] = command[8 + a];
						b++;
						a++;
					}

					a++;
					while(8 + a < command.Length && command[8 + a].ToString() != " ")
					{
						zT[c] = command[8 + a];
						c++;
						a++;
					}

					float X_ = float.Parse(new string(xT));
					float Y_ = float.Parse(new string(yT));
					float Z_ = float.Parse(new string(zT));

					RotateModel(X_, Y_, Z_);
				}
				catch(Exception)
				{
					ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
				}
			}
			else if(command.Length >= 10 && command.Substring(0, 10) == "/renderset")
			{
				try
				{
					char mode;
					mode = command[11];

					if(int.Parse(mode.ToString()) == 1)
					{
						RenderedMesh.SetActive(true);
						RenderedMesh.GetComponent<Renderer>().material = RenderedMesh.GetComponent<RenderMaterial>().Materials_[0];
						ConsoleMessage("\n" + "<color=yellow>Changed rendering mode to TEXTURED.</color>");
					}
					else if(int.Parse(mode.ToString()) == 0)
					{
						RenderedMesh.SetActive(true);
						RenderedMesh.GetComponent<Renderer>().material = RenderedMesh.GetComponent<RenderMaterial>().Materials_[1];
						ConsoleMessage("\n" + "<color=yellow>Changed rendering mode to WIREFRAME.</color>");
					}
					else if(int.Parse(mode.ToString()) == 2)
					{
						RenderedMesh.SetActive(false);
						ConsoleMessage("\n" + "<color=yellow>Changed rendering mode to VERTEX ONLY.</color>");
					}
					else
					{
						ConsoleMessage("\n" + "<color=red>Render mode not found.</color>");
					}
				}
				catch(Exception)
				{
					ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
				}
			}
			else if(command.Substring(0, 10) == "/savemodel" && fileOpen)
			{
				if(command.Length > 11)
				{
					try
					{
						List<char> savePath = new List<char>();

						for(int x = 11; x < command.Length; x++)
						{
							savePath.Add(command[x]);
						}

						if(new string(savePath.ToArray()) != "" && Directory.Exists(new string(savePath.ToArray())))
						{
							SaveModelToXfbin(new string(savePath.ToArray()));
						}
						else
						{
							ConsoleMessage("\nError saving model. Is the file in use?");
						}
					}
					catch(Exception)
					{
						ConsoleMessage("\nError saving model. Is the file in use?");
					}
				}
				else
				{
					try
					{
						SaveFileDialog saveFileDialog1 = new SaveFileDialog();
						saveFileDialog1.ShowDialog();

						if(saveFileDialog1.FileName != "")
						{
							SaveModelToXfbin(saveFileDialog1.FileName);
						}
					}
					catch(Exception)
					{
						ConsoleMessage("\nError saving model. Is the file in use?");
					}
				}
			}
			else if(command.Substring(0, 10) == "/saveunsmf" && fileOpen)
			{
				if(command.Length > 11)
				{
					try
					{
						List<char> savePath = new List<char>();

						for(int x = 11; x < command.Length; x++)
						{
							savePath.Add(command[x]);
						}

						if(new string(savePath.ToArray()) != "" && Directory.Exists(new string(savePath.ToArray())))
						{
							SaveModel(new string(savePath.ToArray()));
						}
						else
						{
							ConsoleMessage("\nError saving model. Is the file in use?");
						}
					}
					catch(Exception)
					{
						ConsoleMessage("\nError saving model. Is the file in use?");
					}
				}
				else
				{
					try
					{
						FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
						folderBrowserDialog1.ShowDialog();

						if(folderBrowserDialog1.SelectedPath != "")
						{
							SaveModel(folderBrowserDialog1.SelectedPath);
						}
					}
					catch(Exception)
					{
						ConsoleMessage("\nError saving model. Is the file in use?");
					}
				}
			}
			else if(command == "/closefile")
			{
				DialogResult result = MessageBox.Show("Do you want to close this file?", "Are you sure?", MessageBoxButtons.OKCancel);
				if(result == DialogResult.OK)
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene(0);
				}
			}
			else if(command.Length >= 9 && command.Substring(0, 9) == "/lighting")
			{
				try
				{
					char mode;
					mode = command[11];

					if(int.Parse(mode.ToString()) == 0)
					{
						GameObject.Find("Directional light").GetComponent<Light>().intensity = 0;
						ConsoleMessage("\n" + "<color=yellow>Disabled lighting.</color>");
					}
					else if(int.Parse(mode.ToString()) == 1)
					{
						GameObject.Find("Directional light").GetComponent<Light>().intensity = 1;
						ConsoleMessage("\n" + "<color=yellow>Enabled lighting.</color>");
					}
					else
					{
						ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
					}
				}
				catch(Exception)
				{
					ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
				}
			}
			else
			{
				ConsoleMessage("\n" + "<color=white>Command not found.</color>");
			}
		}
	}

	public void ConsoleMessage(string message_)
	{
		GameObject.Find("Console").GetComponentInChildren<Text>().text = GameObject.Find("Console").GetComponentInChildren<Text>().text + message_;
	}

	public void ImportModel(string importPath)
	{
		string[] objModelLines = File.ReadAllLines(importPath);

		int v = 0;
		int vt = 0;
		int vn = 0;
		int f = 0;

		int vTotal = 0;
		int vtTotal = 0;
		int vnTotal = 0;
		int fTotal = 0;

		TextureUVs.Clear();
		meshNormals.Clear();
		meshTriangles.Clear();

		List<Vector2> TexUV_ = new List<Vector2>();
		List<Vector3> Normals_ = new List<Vector3>();

		for(int x = 0; x < objModelLines.Length; x++)
		{
			string line_ = objModelLines[x];
			if(line_.Length > 0)
			{
				if(line_[0] == 'v' && line_[1] == ' ')
				{
					vTotal++;
				}
				else if(line_[0] == 'v' && line_[1] == 't' && line_[2] == ' ')
				{
					vtTotal++;
				}
				else if(line_[0] == 'v' && line_[1] == 'n' && line_[2] == ' ')
				{
					vnTotal++;
				}
				else if(line_[0] == 'f' && line_[1] == ' ')
				{
					fTotal++;
				}
			}
		}

		for(int x = 0; x < vtTotal; x++)
		{
			TexUV_.Add(new Vector2(0, 0));
		}

		for(int x = 0; x < vnTotal; x++)
		{
			Normals_.Add(new Vector3(0, 0, 0));
		}

		if(vTotal > VertexCount)
		{
			for(int x = 0; x < vTotal; x++)
			{	
				TextureUVs.Add(new Vector2(0, 0));
				meshNormals.Add(new Vector3(0, 0, 0));
			}
		}
		else
		{
			for(int x = 0; x < VertexCount; x++)
			{	
				TextureUVs.Add(new Vector2(0, 0));
				meshNormals.Add(new Vector3(0, 0, 0));
			}
		}

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
					Z_ = float.Parse(new string(zT));

					if(GameObject.Find(v.ToString()) != null)
					{
						vertexPosition[v] = new Vector3(X_ * -1, Y_, Z_);
						GameObject.Find(v.ToString()).transform.position = vertexPosition[v];
						meshVertices[v] = vertexPosition[v];
					}
					else
					{
						vertexPosition.Add(new Vector3(X_, Y_, Z_));
						GameObject actualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
						meshVertices.Add(vertexPosition[v]);

						actualObject.AddComponent<VertexObject>();
						actualObject.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
						actualObject.transform.position = vertexPosition[v];
						actualObject.name = v.ToString();
						actualObject.transform.SetParent(GameObject.Find("Model Data").transform);
						actualObject.transform.SetAsLastSibling();
						actualObject.tag = "Vertex";
						actualObject.layer = 9;
						vertexBone.Add(new Vector3(0, 0, 0));
					}
					v++;
				}
				else if(line_[0] == 'v' && line_[1] == 't' && line_[2] == ' ')
				{
					int a = 0;
					int a1 = 0;
					int b = 0;

					char[] xT = new char[50];
					char[] yT = new char[50];

					char[] line = new char[100];
					line = line_.ToCharArray();

					a = 3;

					while(line[a].ToString() != " ")
					{
						xT[a1] = line[a];
						a1++;
						a++;
					}

					a++;
					while(a < line.Length && line[a].ToString() != " ")
					{
						yT[b] = line[a];
						b++;
						a++;
					}

					float X_;
					float Y_;

					X_ = float.Parse(new string(xT));
					Y_ = float.Parse(new string(yT));

					TexUV_[vt] = new Vector2(X_, Y_);
					vt++;
				}
				else if(line_[0] == 'v' && line_[1] == 'n' && line_[2] == ' ')
				{
					int a = 0;
					int a1 = 0;
					int b = 0;
					int c = 0;

					char[] xT = new char[50];
					char[] yT = new char[50];
					char[] zT = new char[50];

					char[] line = new char[100];
					line = line_.ToCharArray();

					a = 3;

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
					Z_ = float.Parse(new string(zT));

					Normals_[vn] = new Vector3(X_, Y_, Z_);
					vn++;
				}
				else if(line_[0] == 'f' && line_[1] == ' ')
				{
					int ap = 0;

					int[] VertexTriangle = new int[3];

					List<char> vertexT = new List<char>();
					List<char> textureT = new List<char>();
					List<char> normalT = new List<char>(9);

					char[] line;
					line = line_.ToCharArray();

					ap = 2;

					for(int i = 0; i < 3; i++)
					{
						vertexT.Clear();
						textureT.Clear();
						normalT.Clear();
						while(ap < line.Length && line[ap].ToString().All(Char.IsDigit) == true)
						{
							vertexT.Add(line[ap]);
							ap++;
						}

						ap++;
						if(ap < line.Length && line[ap] == ' ')
						{
							textureT.Add('0');
						}
						else
						{
							while(ap < line.Length && line[ap].ToString().All(Char.IsDigit) == true)
							{
								textureT.Add(line[ap]);
								ap++;
							}

							ap++;
							if(ap < line.Length && line[ap] == ' ')
							{
								normalT.Add('0');
							}
							else
							{
								while(ap < line.Length && line[ap].ToString().All(Char.IsDigit) == true)
								{
									normalT.Add(line[ap]);
									ap++;
								}
							}
						}

						int vertex_ = int.Parse(new string(vertexT.ToArray())) - 1;
						int texture_ = int.Parse(new string(textureT.ToArray())) - 1;
						int normal_ = int.Parse(new string(normalT.ToArray())) - 1;

						VertexTriangle[i] = vertex_;

						TextureUVs[vertex_] = TexUV_[texture_];
						meshNormals[vertex_] = Normals_[normal_];

						ap++;
					}

					meshTriangles.Add(VertexTriangle[0]);
					meshTriangles.Add(VertexTriangle[1]);
					meshTriangles.Add(VertexTriangle[2]);
					meshTriangles.Add(VertexTriangle[2]);
					meshTriangles.Add(VertexTriangle[1]);
					meshTriangles.Add(VertexTriangle[0]);
					f++;
				}
			}
		}

		if(vTotal < VertexCount)
		{
			for(int x = vTotal; x < VertexCount; x++)
			{
				GameObject.Find(x.ToString()).transform.position = new Vector3(0, 0, 0);
				vertexPosition[x] = new Vector3(0, 0, 0);
				meshVertices[x] = new Vector3(0, 0, 0);
			}
		}

		mf.mesh.vertices = meshVertices.ToArray();

		for(int x = 0; x < TextureUVs.ToArray().Length; x++)
		{
			TextureUVs[x] = new Vector2(TextureUVs[x].x, TextureUVs[x].y * -1);
		}

		mf.mesh.uv = TextureUVs.ToArray();
		mf.mesh.normals = meshNormals.ToArray();
		mf.mesh.triangles = meshTriangles.ToArray();
		mf.mesh.RecalculateBounds();
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
		if(number == 0)
		{
			vertexBone[vertex] = new Vector3((float)boneID, vertexBone[vertex].y, vertexBone[vertex].z);
		}
		else if(number == 1)
		{
			vertexBone[vertex] = new Vector3(vertexBone[vertex].x, (float)boneID, vertexBone[vertex].z);
		}
		else if(number == 2)
		{
			vertexBone[vertex] = new Vector3(vertexBone[vertex].x, vertexBone[vertex].y, (float)boneID);
		}
	}

	public Vector3 GetBonesOfVertex(int vertex)
	{
		return vertexBone[vertex];
	}

	public void ScaleModel(float scalex, float scaley, float scalez)
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

		mf.mesh.vertices = meshVertices.ToArray();
		ConsoleMessage("\n" + "<color=lime>Scaled model in " + scalex.ToString() + " " + scaley.ToString() + " " + scalez.ToString() + ".</color>");
	}

	public void RotateModel(float rotatex, float rotatey, float rotatez)
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

		mf.mesh.vertices = meshVertices.ToArray();
		ConsoleMessage("\n" + "<color=lime>Rotated model in " + rotatex.ToString() + " " + rotatey.ToString() + " " + rotatez.ToString() + ".</color>");
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
			objModelLines.Add("v " + (meshVertices[x].x * -1).ToString() + " " + meshVertices[x].y.ToString() + " " + meshVertices[x].z.ToString());
		}
		objModelLines.Add("# " + VertexCount.ToString() + " vertices");

		objModelLines.Add("\n");

		for(int x = 0; x < VertexCount; x++)
		{
			if(mf.mesh.uv.Length >= x + 1)
			{
				objModelLines.Add("vt " + mf.mesh.uv[x].x.ToString() + " " + (mf.mesh.uv[x].y * -1).ToString());
			}
			else
			{
				objModelLines.Add("vt 0 0");
			}
		}
		objModelLines.Add("# " + VertexCount.ToString() + " texture coordinates");

		objModelLines.Add("\n");

		for(int x = 0; x < VertexCount; x++)
		{
			objModelLines.Add("vn " + mf.mesh.normals[x].x.ToString() + " " + (mf.mesh.normals[x].y).ToString() + " " + mf.mesh.normals[x].z.ToString());
		}
		objModelLines.Add("# " + VertexCount.ToString() + " normals");

		objModelLines.Add("\n");

		objModelLines.Add("g Mesh01");
		objModelLines.Add("s 1");

		int a = 0;
		while(a < mf.mesh.triangles.Length - 6)
		{
			objModelLines.Add("f " + (mf.mesh.triangles[0 + a] + 1).ToString() + "/" + (mf.mesh.triangles[0 + a] + 1).ToString() + "/" + (mf.mesh.triangles[0 + a] + 1).ToString() + " " + (mf.mesh.triangles[1 + a] + 1).ToString() + "/" + (mf.mesh.triangles[1 + a] + 1).ToString() + "/" + (mf.mesh.triangles[1 + a] + 1).ToString() + " " + (mf.mesh.triangles[2 + a] + 1).ToString() + "/" + (mf.mesh.triangles[2 + a] + 1).ToString() + "/" + (mf.mesh.triangles[2 + a] + 1).ToString());
			objModelLines.Add("f " + (mf.mesh.triangles[2 + a] + 1).ToString() + "/" + (mf.mesh.triangles[2 + a] + 1).ToString() + "/" + (mf.mesh.triangles[2 + a] + 1).ToString() + " " + (mf.mesh.triangles[1 + a] + 1).ToString() + "/" + (mf.mesh.triangles[1 + a] + 1).ToString() + "/" + (mf.mesh.triangles[1 + a] + 1).ToString() + " " + (mf.mesh.triangles[0 + a] + 1).ToString() + "/" + (mf.mesh.triangles[0 + a] + 1).ToString() + "/" + (mf.mesh.triangles[0 + a] + 1).ToString());
			a = a + 6;
		}
		objModelLines.Add("# " + (mf.mesh.triangles.Length / 6).ToString() + " triangles");

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

	public void LoadTexture()
	{
		OpenFileDialog openFileDialog1 = new OpenFileDialog();
		openFileDialog1.ShowDialog();

		if(openFileDialog1.FileName != "" && File.Exists(openFileDialog1.FileName))
		{
			try
			{
				byte[] textureBytes = File.ReadAllBytes(openFileDialog1.FileName);
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
		if(byteLenght == 64)
		{
			WindowOpen = true;
			window_boneEditor.SetActive(true);
			window_boneEditor.GetComponent<w_VertexBoneEditor>().EnableWindow();
		}
		else
		{
			ConsoleMessage("\nThis model doesn't have bone data.");
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
}