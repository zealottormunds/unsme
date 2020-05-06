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

public class SaveEachGroup : MonoBehaviour {
	public RenderFile rf;

	public void SaveEach()
	{
		SaveFileDialog save = new SaveFileDialog();
		save.ShowDialog();

		if(save.FileName != "")
		{
			for(int x = 0; x < GetComponent<GroupSelection>().Groups.Count; x++)
			{
				if(save.FileName.Contains(".xfbin")) SaveGroupsToXfbin(x, save.FileName.Replace(".xfbin", "_" + GetComponent<GroupSelection>().GroupNames[x] + ".xfbin"));
				else SaveGroupsToXfbin(x, save.FileName + "_" + GetComponent<GroupSelection>().GroupNames[x] + ".xfbin");
			}

			MessageBox.Show(".xfbin files saved.");
		}
		else
		{
			MessageBox.Show("No name specified.");
		}
	}

	[HideInInspector]
	public DialogResult dial = DialogResult.No;

	public void SaveGroupsToXfbin(int groupNum, string path__)
	{
		try
		{
			Transform ModelDataTransform;
			ModelDataTransform = GameObject.Find("Model Data").GetComponent<Transform>();

			List<byte> triangleFileNew = new List<byte>();
			List<byte> vertexFileNew = new List<byte>();
			List<byte> textureFileNew = new List<byte>();

			//DialogResult dial = MessageBox.Show("Fix vertex count and face count? This fix is only useful if you have used /importobj. However, /impobjpos does not need it.", "Warning", MessageBoxButtons.YesNo);

			rf.meshNormals = rf.mf.mesh.normals.ToList();
			rf.meshTriangles = rf.mf.mesh.triangles.ToList();
			rf.meshVertices = rf.mf.mesh.vertices.ToList();

			List<Vector3> meshVertices = rf.meshVertices;
			List<int> meshTriangles_full = rf.meshTriangles;
			List<int> meshTriangles = new List<int>();
			List<Vector3> meshNormals = rf.meshNormals;
			List<Vector2> meshUVs = rf.mf.mesh.uv.ToList();
			List<Vector4> vertexWeight = rf.vertexWeight.ToList();
			List<Vector4> vertexBone = rf.vertexBone.ToList();

			int VertexCount = 0;
			GroupSelection gp = GetComponent<GroupSelection>();

			for(int x = 0; x < meshTriangles_full.Count; x = x + 3)
			{
				if(gp.Groups[groupNum].Contains(meshTriangles_full[x + 0]) && gp.Groups[groupNum].Contains(meshTriangles_full[x + 1]) && gp.Groups[groupNum].Contains(meshTriangles_full[x + 2]))
				{
					meshTriangles.Add(meshTriangles_full[x + 0]);
					meshTriangles.Add(meshTriangles_full[x + 1]);
					meshTriangles.Add(meshTriangles_full[x + 2]);
				}
			}

			VertexCount = meshVertices.Count;

			for(int x = 0; x < VertexCount; x++)
			{
				for(int z = 0; z < rf.byteLength; z++)
				{
					vertexFileNew.Add(0x00);
				}
			}

			for(int x = 0; x < VertexCount; x++)
			{
				byte[] posx = BitConverter.GetBytes(meshVertices[x].x).ToArray();
				if(rf.stageMode == true) posx = BitConverter.GetBytes(meshVertices[x].x * 20).ToArray();
				Array.Reverse(posx);
				vertexFileNew[0x0 + (rf.byteLength * x)] = posx[0];
				vertexFileNew[0x1 + (rf.byteLength * x)] = posx[1];
				vertexFileNew[0x2 + (rf.byteLength * x)] = posx[2];
				vertexFileNew[0x3 + (rf.byteLength * x)] = posx[3];

				byte[] posz = BitConverter.GetBytes(meshVertices[x].z).ToArray();
				if(rf.stageMode == true) posz = BitConverter.GetBytes(meshVertices[x].z * 20).ToArray();
				Array.Reverse(posz);
				vertexFileNew[0x4 + (rf.byteLength * x)] = posz[0];
				vertexFileNew[0x5 + (rf.byteLength * x)] = posz[1];
				vertexFileNew[0x6 + (rf.byteLength * x)] = posz[2];
				vertexFileNew[0x7 + (rf.byteLength * x)] = posz[3];

				byte[] posy = BitConverter.GetBytes(meshVertices[x].y).ToArray();
				if(rf.stageMode == true) posy = BitConverter.GetBytes(meshVertices[x].y * 20).ToArray();
				Array.Reverse(posy);
				vertexFileNew[0x8 + (rf.byteLength * x)] = posy[0];
				vertexFileNew[0x9 + (rf.byteLength * x)] = posy[1];
				vertexFileNew[0xA + (rf.byteLength * x)] = posy[2];
				vertexFileNew[0xB + (rf.byteLength * x)] = posy[3];

				if(rf.byteLength == 0x1C)
				{
					byte[] NXA = RenderFile.ToInt(meshNormals[x].x);
					Array.Reverse(NXA);

					byte[] NYA = RenderFile.ToInt(meshNormals[x].y);
					Array.Reverse(NYA);

					byte[] NZA = RenderFile.ToInt(meshNormals[x].z);
					Array.Reverse(NZA);

					vertexFileNew[0xC + (rf.byteLength * x)] = NXA[0];
					vertexFileNew[0xD + (rf.byteLength * x)] = NXA[1];

					vertexFileNew[0xE + (rf.byteLength * x)] = NYA[0];
					vertexFileNew[0xF + (rf.byteLength * x)] = NYA[1];

					vertexFileNew[0x10 + (rf.byteLength * x)] = NZA[0];
					vertexFileNew[0x11 + (rf.byteLength * x)] = NZA[1];
				}
				else if(rf.byteLength == 0x40)
				{
					// BONE DATA
					vertexFileNew[0x23 + (rf.byteLength * x)] = (byte)vertexBone[x].x;
					vertexFileNew[0x27 + (rf.byteLength * x)] = (byte)vertexBone[x].y;
					vertexFileNew[0x2B + (rf.byteLength * x)] = (byte)vertexBone[x].z;
                    vertexFileNew[0x2F + (rf.byteLength * x)] = (byte)vertexBone[x].w;

					// WEIGHT DATA
					byte[] weightx = BitConverter.GetBytes(vertexWeight[x].x).ToArray();
					Array.Reverse(weightx);

					vertexFileNew[0x30 + (rf.byteLength * x)] = weightx[0];
					vertexFileNew[0x31 + (rf.byteLength * x)] = weightx[1];
					vertexFileNew[0x32 + (rf.byteLength * x)] = weightx[2];
					vertexFileNew[0x33 + (rf.byteLength * x)] = weightx[3];

					byte[] weighty = BitConverter.GetBytes(vertexWeight[x].y).ToArray();
					Array.Reverse(weighty);

					vertexFileNew[0x34 + (rf.byteLength * x)] = weighty[0];
					vertexFileNew[0x35 + (rf.byteLength * x)] = weighty[1];
					vertexFileNew[0x36 + (rf.byteLength * x)] = weighty[2];
					vertexFileNew[0x37 + (rf.byteLength * x)] = weighty[3];

					byte[] weightz = BitConverter.GetBytes(vertexWeight[x].z).ToArray();
					Array.Reverse(weightz);

					vertexFileNew[0x38 + (rf.byteLength * x)] = weightz[0];
					vertexFileNew[0x39 + (rf.byteLength * x)] = weightz[1];
					vertexFileNew[0x3A + (rf.byteLength * x)] = weightz[2];
					vertexFileNew[0x3B + (rf.byteLength * x)] = weightz[3];

                    byte[] weightw = BitConverter.GetBytes(vertexWeight[x].w).ToArray();
                    Array.Reverse(weightw);

                    vertexFileNew[0x3C + (rf.byteLength * x)] = weightw[0];
                    vertexFileNew[0x3D + (rf.byteLength * x)] = weightw[1];
                    vertexFileNew[0x3E + (rf.byteLength * x)] = weightw[2];
                    vertexFileNew[0x3F + (rf.byteLength * x)] = weightw[3];

                    // NORMALS
                    byte[] normalx = BitConverter.GetBytes(meshNormals[x].x).ToArray();
					Array.Reverse(normalx);

					vertexFileNew[0x10 + (rf.byteLength * x)] = normalx[0];
					vertexFileNew[0x11 + (rf.byteLength * x)] = normalx[1];
					vertexFileNew[0x12 + (rf.byteLength * x)] = normalx[2];
					vertexFileNew[0x13 + (rf.byteLength * x)] = normalx[3];

					byte[] normaly = BitConverter.GetBytes(meshNormals[x].y).ToArray();
					Array.Reverse(normaly);

					vertexFileNew[0x14 + (rf.byteLength * x)] = normaly[0];
					vertexFileNew[0x15 + (rf.byteLength * x)] = normaly[1];
					vertexFileNew[0x16 + (rf.byteLength * x)] = normaly[2];
					vertexFileNew[0x17 + (rf.byteLength * x)] = normaly[3];

					byte[] normalz = BitConverter.GetBytes(meshNormals[x].z).ToArray();
					Array.Reverse(normalz);

					vertexFileNew[0x18 + (rf.byteLength * x)] = normalz[0];
					vertexFileNew[0x19 + (rf.byteLength * x)] = normalz[1];
					vertexFileNew[0x1A + (rf.byteLength * x)] = normalz[2];
					vertexFileNew[0x1B + (rf.byteLength * x)] = normalz[3];
				}
				else if(rf.byteLength == 0x20)
				{
					// NORMALS
					byte[] normalx = BitConverter.GetBytes(meshNormals[x].x).ToArray();
					Array.Reverse(normalx);

					vertexFileNew[0xC + (rf.byteLength * x)] = normalx[0];
					vertexFileNew[0xD + (rf.byteLength * x)] = normalx[1];
					vertexFileNew[0xE + (rf.byteLength * x)] = normalx[2];
					vertexFileNew[0xF + (rf.byteLength * x)] = normalx[3];

					byte[] normaly = BitConverter.GetBytes(meshNormals[x].y).ToArray();
					Array.Reverse(normaly);

					vertexFileNew[0x10 + (rf.byteLength * x)] = normaly[0];
					vertexFileNew[0x11 + (rf.byteLength * x)] = normaly[1];
					vertexFileNew[0x12 + (rf.byteLength * x)] = normaly[2];
					vertexFileNew[0x13 + (rf.byteLength * x)] = normaly[3];

					byte[] normalz = BitConverter.GetBytes(meshNormals[x].z).ToArray();
					Array.Reverse(normalz);

					vertexFileNew[0x14 + (rf.byteLength * x)] = normalz[0];
					vertexFileNew[0x15 + (rf.byteLength * x)] = normalz[1];
					vertexFileNew[0x16 + (rf.byteLength * x)] = normalz[2];
					vertexFileNew[0x17 + (rf.byteLength * x)] = normalz[3];
				}
			}
			for(int x = 0; x < meshUVs.Count; x++)
			{
				if(rf.byteLength == 0x1C)
				{
					byte[] UVXA = RenderFile.ToInt(meshUVs[x].x);
					Array.Reverse(UVXA);

					byte[] UVYA = RenderFile.ToInt(meshUVs[x].y);
					Array.Reverse(UVYA);

					vertexFileNew[0x18 + (rf.byteLength * x)] = UVXA[0];
					vertexFileNew[0x19 + (rf.byteLength * x)] = UVXA[1];

					vertexFileNew[0x1A + (rf.byteLength * x)] = UVYA[0];
					vertexFileNew[0x1B + (rf.byteLength * x)] = UVYA[1];
				}
				else if(rf.byteLength == 0x20)
				{
					byte[] UVXA = RenderFile.ToInt(meshUVs[x].x);
					Array.Reverse(UVXA);

					byte[] UVYA = RenderFile.ToInt(meshUVs[x].y);
					Array.Reverse(UVYA);

					vertexFileNew[0x18 + (rf.byteLength * x)] = UVXA[0];
					vertexFileNew[0x19 + (rf.byteLength * x)] = UVXA[1];
					vertexFileNew[0x1A + (rf.byteLength * x)] = UVYA[0];
					vertexFileNew[0x1B + (rf.byteLength * x)] = UVYA[1];

					vertexFileNew[0x1C + (rf.byteLength * x)] = 0;
					vertexFileNew[0x1D + (rf.byteLength * x)] = 0;
					vertexFileNew[0x1E + (rf.byteLength * x)] = 0;
					vertexFileNew[0x1F + (rf.byteLength * x)] = 0;
				}
				else if(rf.byteLength == 0x40)
				{
					if(rf.textureType == 0)
					{
						byte[] UVXA = RenderFile.ToInt(meshUVs[x].x);
						Array.Reverse(UVXA);

						byte[] UVYA = RenderFile.ToInt(meshUVs[x].y);
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
					else if(rf.textureType == 1)
					{
						byte[] UVXA = RenderFile.ToInt(meshUVs[x].x);
						Array.Reverse(UVXA);

						byte[] UVYA = RenderFile.ToInt(meshUVs[x].y);
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
	
			for(int q = 0; q <= meshTriangles.Count - 3; q += 3)
			{
				byte[] tri1 = BitConverter.GetBytes(meshTriangles[q + 0]).ToArray();
				byte[] tri2 = BitConverter.GetBytes(meshTriangles[q + 1]).ToArray();
				byte[] tri3 = BitConverter.GetBytes(meshTriangles[q + 2]).ToArray();

				triangleFileNew.Add(tri3[1]);
				triangleFileNew.Add(tri3[0]);
				triangleFileNew.Add(tri2[1]);
				triangleFileNew.Add(tri2[0]);
				triangleFileNew.Add(tri1[1]);
				triangleFileNew.Add(tri1[0]);

				triangleFileNew.Add(0xFF);
				triangleFileNew.Add(0xFF);
			}

			int OriginalNDP3Size = 
				rf.OriginalNDP3[0x4] * 0x1000000 + 
				rf.OriginalNDP3[0x5] * 0x10000 + 
				rf.OriginalNDP3[0x6] * 0x100 + 
				rf.OriginalNDP3[0x7];

			int SizeBeforeNDP3Index = 0;
			int sizeMode = 0;

			int x_ = 0;
			while(rf.OriginalXfbin[rf.NDP3Index - 4 + x_] * 0x1000000 + rf.OriginalXfbin[rf.NDP3Index - 3 + x_] * 0x10000 + rf.OriginalXfbin[rf.NDP3Index - 2 + x_] * 0x100 + rf.OriginalXfbin[rf.NDP3Index - 1 + x_] != OriginalNDP3Size)
			{
				x_--;
			}

			SizeBeforeNDP3Index = x_ - 4;

			List<byte> newNDP3File = new List<byte>();
			for(int x = 0; x < 0x10; x++)
			{
				newNDP3File.Add(rf.OriginalNDP3[x]);
			}

			//Add first section size
			for(int x = 0; x < 4; x++)
			{
				newNDP3File.Add(rf.OriginalNDP3[0x10 + x]);
			}
			int firstSectionSize_ = newNDP3File[16] * 0x1000000 + newNDP3File[17] * 0x10000 + newNDP3File[18] * 0x100 + newNDP3File[19];

			//Add triangle section size
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
			if(rf.byteLength == 0x40)
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
			else if(rf.byteLength == 0x1C)
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
			else if(rf.byteLength == 0x20)
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
				newNDP3File.Add(rf.OriginalNDP3[x]);
			}

			// Add first section
			for(int x = 0; x < firstSectionSize_; x++)
			{
				newNDP3File.Add(rf.OriginalNDP3[0x30 + x]);
			}

			// Add triangle section
			for(int x = 0; x < triangleFileNew.Count; x++)
			{
				newNDP3File.Add(triangleFileNew[x]);
			}

			if(rf.byteLength == 0x40)
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
			else if(rf.byteLength == 0x1C || rf.byteLength == 0x20)
			{
				// Add vertex section
				for(int x = 0; x < vertexFileNew.Count; x++)
				{
					newNDP3File.Add(vertexFileNew[x]);
				}
			}

			int newNDP3Size = newNDP3File.Count + OriginalNDP3Size - 48 - firstSectionSize_ - rf.triangleFile.Length - rf.textureMapFile.Length - rf.fileBytes.Length;

			List<byte> newXfbinFile = new List<byte>();

			// Copy old .xfbin file until ndp3
			for(int x = 0; x < rf.NDP3Index; x++)
			{
				newXfbinFile.Add(rf.OriginalXfbin[x]);
			}

			// Add new size before NDP3
			if(newNDP3Size < 0x100)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 1] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 2] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x100 && newNDP3Size < 0x10000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 1] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 2] = arrayOfSize[1];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x10000 && newNDP3Size < 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 1] = arrayOfSize[2];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 2] = arrayOfSize[1];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
			}
			else if(newNDP3Size >= 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size);

				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index] = arrayOfSize[3];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 1] = arrayOfSize[2];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 2] = arrayOfSize[1];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index + 3] = arrayOfSize[0];
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
			for(int x = rf.NDP3Index + (0x30 + firstSectionSize_ + rf.triangleFile.Length + rf.textureMapFile.Length + rf.fileBytes.Length); x < rf.OriginalXfbin.Length; x++)
			{
				newXfbinFile.Add(rf.OriginalXfbin[x]);
			}

			// Fix new NDP3 size 36
			int DifferenceBetweenSizes = 
				(rf.OriginalXfbin[rf.NDP3Index + SizeBeforeNDP3Index - 0x24] * 0x1000000 + 
				rf.OriginalXfbin[rf.NDP3Index + SizeBeforeNDP3Index - 0x23] * 0x10000 + 
				rf.OriginalXfbin[rf.NDP3Index + SizeBeforeNDP3Index - 0x22] * 0x100 + 
				rf.OriginalXfbin[rf.NDP3Index + SizeBeforeNDP3Index - 0x21]) - OriginalNDP3Size;

			if(newNDP3Size + DifferenceBetweenSizes < 0x100)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x24] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x23] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x22] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x21] = arrayOfSize[0];
			}
			else if(newNDP3Size + DifferenceBetweenSizes >= 0x100 && newNDP3Size + DifferenceBetweenSizes < 0x10000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x24] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x23] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x22] = arrayOfSize[1];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x21] = arrayOfSize[0];
			}
			else if(newNDP3Size + DifferenceBetweenSizes >= 0x10000 && newNDP3Size + DifferenceBetweenSizes < 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x24] = 0;
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x23] = arrayOfSize[2];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x22] = arrayOfSize[1];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x21] = arrayOfSize[0];
			}
			else if(newNDP3Size + DifferenceBetweenSizes >= 0x1000000)
			{
				byte[] arrayOfSize = BitConverter.GetBytes(newNDP3Size + DifferenceBetweenSizes);
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x24] = arrayOfSize[3];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x23] = arrayOfSize[2];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x22] = arrayOfSize[1];
				newXfbinFile[rf.NDP3Index + SizeBeforeNDP3Index - 0x21] = arrayOfSize[0];
			}

			// FIX 6C AND 80
			if(dial == DialogResult.Yes)
			{
				if(rf.groupCount == 1)
				{
					//Fix 6C
					Int32 vertexCountTemp = VertexCount;

					if(vertexCountTemp <= 0x100)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(vertexCountTemp);
						newXfbinFile[rf.NDP3Index + 0x6A] = 0;
						newXfbinFile[rf.NDP3Index + 0x6B] = 0;
						newXfbinFile[rf.NDP3Index + 0x6C] = 0;
						newXfbinFile[rf.NDP3Index + 0x6D] = arrayOfSize[0];
					}
					else if(vertexCountTemp > 0x100 && vertexCountTemp <= 0x10000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(vertexCountTemp);
						newXfbinFile[rf.NDP3Index + 0x6A] = 0;
						newXfbinFile[rf.NDP3Index + 0x6B] = 0;
						newXfbinFile[rf.NDP3Index + 0x6C] = arrayOfSize[1];
						newXfbinFile[rf.NDP3Index + 0x6D] = arrayOfSize[0];
					}
					else if(vertexCountTemp > 0x10000 && vertexCountTemp <= 0x1000000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(vertexCountTemp);
						newXfbinFile[rf.NDP3Index + 0x6A] = 0;
						newXfbinFile[rf.NDP3Index + 0x6B] = arrayOfSize[2];
						newXfbinFile[rf.NDP3Index + 0x6C] = arrayOfSize[1];
						newXfbinFile[rf.NDP3Index + 0x6D] = arrayOfSize[0];
					}
					else if(vertexCountTemp > 0x1000000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(vertexCountTemp);
						newXfbinFile[rf.NDP3Index + 0x6A] = arrayOfSize[3];
						newXfbinFile[rf.NDP3Index + 0x6B] = arrayOfSize[2];
						newXfbinFile[rf.NDP3Index + 0x6C] = arrayOfSize[1];
						newXfbinFile[rf.NDP3Index + 0x6D] = arrayOfSize[0];
					}

					//Fix 80
					if(triangleFileNew.ToArray().Length / 2 <= 0x100)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(triangleFileNew.ToArray().Length / 2);
						newXfbinFile[rf.NDP3Index + 0x7E] = 0;
						newXfbinFile[rf.NDP3Index + 0x7F] = 0;
						newXfbinFile[rf.NDP3Index + 0x80] = 0;
						newXfbinFile[rf.NDP3Index + 0x81] = arrayOfSize[0];
					}
					else if(triangleFileNew.ToArray().Length / 2 > 0x100 && triangleFileNew.ToArray().Length / 2 <= 0x10000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(triangleFileNew.ToArray().Length / 2);
						newXfbinFile[rf.NDP3Index + 0x7E] = 0;
						newXfbinFile[rf.NDP3Index + 0x7F] = 0;
						newXfbinFile[rf.NDP3Index + 0x80] = arrayOfSize[1];
						newXfbinFile[rf.NDP3Index + 0x81] = arrayOfSize[0];
					}
					else if(triangleFileNew.ToArray().Length / 2 > 0x10000 && triangleFileNew.ToArray().Length / 2 <= 0x1000000)
					{
						byte[] arrayOfSize = BitConverter.GetBytes(triangleFileNew.ToArray().Length / 2);
						newXfbinFile[rf.NDP3Index + 0x7E] = 0;
						newXfbinFile[rf.NDP3Index + 0x7F] = arrayOfSize[2];
						newXfbinFile[rf.NDP3Index + 0x80] = arrayOfSize[1];
						newXfbinFile[rf.NDP3Index + 0x81] = arrayOfSize[0];
					}
					else
					{
						byte[] arrayOfSize = BitConverter.GetBytes(triangleFileNew.ToArray().Length / 2);
						newXfbinFile[rf.NDP3Index + 0x7E] = arrayOfSize[3];
						newXfbinFile[rf.NDP3Index + 0x7F] = arrayOfSize[2];
						newXfbinFile[rf.NDP3Index + 0x80] = arrayOfSize[1];
						newXfbinFile[rf.NDP3Index + 0x81] = arrayOfSize[0];
					}
				}
			}

			File.WriteAllBytes(path__, newXfbinFile.ToArray());
		}
		catch(Exception exception)
		{
			MessageBox.Show(exception.ToString());
		}
	}

}
