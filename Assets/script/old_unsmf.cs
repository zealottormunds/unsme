using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class old_unsmf : MonoBehaviour {

	/*
	RENDERFILE

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
			byte[] posx = BitConverter.GetBytes(meshVertices[x].x).ToArray();
			if(stageMode == true) posx = BitConverter.GetBytes(meshVertices[x].x * 20).ToArray();
			Array.Reverse(posx);

			vertexFileNew[0 + (byteLength * x)] = posx[0];
			vertexFileNew[1 + (byteLength * x)] = posx[1];
			vertexFileNew[2 + (byteLength * x)] = posx[2];
			vertexFileNew[3 + (byteLength * x)] = posx[3];

			byte[] posz = BitConverter.GetBytes(meshVertices[x].z).ToArray();
			if(stageMode == true) posz = BitConverter.GetBytes(meshVertices[x].z * 20).ToArray();
			Array.Reverse(posz);
			vertexFileNew[4 + (byteLength * x)] = posz[0];
			vertexFileNew[5 + (byteLength * x)] = posz[1];
			vertexFileNew[6 + (byteLength * x)] = posz[2];
			vertexFileNew[7 + (byteLength * x)] = posz[3];

			byte[] posy = BitConverter.GetBytes(meshVertices[x].y).ToArray();
			if(stageMode == true) posy = BitConverter.GetBytes(meshVertices[x].y * 20).ToArray();
			Array.Reverse(posy);
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

			byteLength = VertexLength_;
			fileBytes = VertexFile;
			triangleFile = TriangleFile;
			textureMapFile = TextureFile;

			for (int x = 0; x < (fileBytes.Length / byteLength); x++)
		    {
				float vertexFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[0 + vertexOffset + (x * byteLength)] * 16777215 + fileBytes[1 + vertexOffset + (x * byteLength)] * 65535 + fileBytes[2 + vertexOffset + (x * byteLength)] * 255 + fileBytes[3 + vertexOffset + (x * byteLength)]), 0);
				float vertexFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[4 + vertexOffset + (x * byteLength)] * 16777215 + fileBytes[5 + vertexOffset + (x * byteLength)] * 65535 + fileBytes[6 + vertexOffset + (x * byteLength)] * 255 + fileBytes[7 + vertexOffset + (x * byteLength)]), 0);
				float vertexFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[8 + vertexOffset + (x * byteLength)] * 16777215 + fileBytes[9 + vertexOffset + (x * byteLength)] * 65535 + fileBytes[10 + vertexOffset + (x * byteLength)] * 255 + fileBytes[11 + vertexOffset + (x * byteLength)]), 0);

				if(stageMode == true)
				{
					vertexFloatX = vertexFloatX / 20;
					vertexFloatZ = vertexFloatZ / 20;
					vertexFloatY = vertexFloatY / 20;
				}

				vertexPosition.Add(new Vector3(vertexFloatX, vertexFloatY, vertexFloatZ));

				if(byteLength == 0x40)
				{
					float normalFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 0 + (x * byteLength)] * 16777215 + fileBytes[16 + vertexOffset + 1 + (x * byteLength)] * 65535 + fileBytes[16 + vertexOffset + 2 + (x * byteLength)] * 255 + fileBytes[16 + vertexOffset + 3 + (x * byteLength)]), 0);
					float normalFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 4 + (x * byteLength)] * 16777215 + fileBytes[16 + vertexOffset + 5 + (x * byteLength)] * 65535 + fileBytes[16 + vertexOffset + 6 + (x * byteLength)] * 255 + fileBytes[16 + vertexOffset + 7 + (x * byteLength)]), 0);
					float normalFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 8 + (x * byteLength)] * 16777215 + fileBytes[16 + vertexOffset + 9 + (x * byteLength)] * 65535 + fileBytes[16 + vertexOffset + 10 + (x * byteLength)] * 255 + fileBytes[16 + vertexOffset + 11 + (x * byteLength)]), 0);
					meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));

					vertexBone.Add(new Vector3((float)fileBytes[35 + vertexOffset + (x * byteLength)], (float)fileBytes[39 + vertexOffset + (x * byteLength)], (float)fileBytes[43 + vertexOffset + (x * byteLength)]));

					float weightFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 0 + (x * byteLength)] * 16777215 + fileBytes[48 + vertexOffset + 1 + (x * byteLength)] * 65535 + fileBytes[48 + vertexOffset + 2 + (x * byteLength)] * 255 + fileBytes[48 + vertexOffset + 3 + (x * byteLength)]), 0);
					float weightFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 4 + (x * byteLength)] * 16777215 + fileBytes[48 + vertexOffset + 5 + (x * byteLength)] * 65535 + fileBytes[48 + vertexOffset + 6 + (x * byteLength)] * 255 + fileBytes[48 + vertexOffset + 7 + (x * byteLength)]), 0);
					float weightFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[48 + vertexOffset + 8 + (x * byteLength)] * 16777215 + fileBytes[48 + vertexOffset + 9 + (x * byteLength)] * 65535 + fileBytes[48 + vertexOffset + 10 + (x * byteLength)] * 255 + fileBytes[48 + vertexOffset + 11 + (x * byteLength)]), 0);

					vertexWeight.Add(new Vector3(weightFloatX, weightFloatY, weightFloatZ));
				}
				else if(byteLength == 0x1C)
				{
					float normalFloatX = toFloat(fileBytes[12 + (x * byteLength)] * 256 + fileBytes[13 + (x * byteLength)]);
					float normalFloatY = toFloat(fileBytes[14 + (x * byteLength)] * 256 + fileBytes[15 + (x * byteLength)]);
					float normalFloatZ = toFloat(fileBytes[16 + (x * byteLength)] * 256 + fileBytes[17 + (x * byteLength)]);
					meshNormals.Add(new Vector3(normalFloatX, normalFloatY, normalFloatZ));
				}
				else if(byteLength == 0x20)
				{
					float normalFloatX = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 0 + (x * byteLength)] * 16777215 + fileBytes[16 + vertexOffset + 1 + (x * byteLength)] * 65535 + fileBytes[16 + vertexOffset + 2 + (x * byteLength)] * 255 + fileBytes[16 + vertexOffset + 3 + (x * byteLength)]), 0);
					float normalFloatZ = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 4 + (x * byteLength)] * 16777215 + fileBytes[16 + vertexOffset + 5 + (x * byteLength)] * 65535 + fileBytes[16 + vertexOffset + 6 + (x * byteLength)] * 255 + fileBytes[16 + vertexOffset + 7 + (x * byteLength)]), 0);
					float normalFloatY = BitConverter.ToSingle(BitConverter.GetBytes(fileBytes[16 + vertexOffset + 8 + (x * byteLength)] * 16777215 + fileBytes[16 + vertexOffset + 9 + (x * byteLength)] * 65535 + fileBytes[16 + vertexOffset + 10 + (x * byteLength)] * 255 + fileBytes[16 + vertexOffset + 11 + (x * byteLength)]), 0);
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

			if(byteLength == 64)
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
			else if(byteLength == 28)
			{
				for(int x = 0; x < VertexCount; x++)
				{
					float x_ = toFloat(fileBytes[x * 24] * 256 + fileBytes[x * 25]);
					float y_ = toFloat(fileBytes[x * 26] * 256 + fileBytes[x * 27]);

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
			ConsoleMessage("\n<color=lime>MODEL LOADED.</color>");
		}
    }

	*/

	//=======================================================================================================================================

	/*

	ModelLoad_Main

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
				GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().OpenModelFromUnsmf(VertexLength, triangleBytes, textureCoordBytes, vertexBytes, stageMode);
				Destroy(GameObject.Find("Welcome Screen"));
			}
		}
		catch
		{
			stageMode = false;
		}
	}

	*/
}
