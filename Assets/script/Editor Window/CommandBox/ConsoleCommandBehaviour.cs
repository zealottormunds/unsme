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

public class ConsoleCommandBehaviour : MonoBehaviour {

	private List<string> Commands = new List<string>();
	private List<int> Parameters = new List<int>();
	private List<string> CommandDescription = new List<string>();
	public RenderFile R;

	void Start()
	{
		RenderFile R = this.gameObject.GetComponent<RenderFile>();

		AddCommand("/start", 0, "Use /help PAGE to get the full list of commands. Use /help: /command to get help on a specific command.");
		AddCommand("/help", 1, "PAGE -> prints a list of the commands in a certain page.");
		AddCommand("/help:", 1, "COMMAND -> shows a description of a certain command.");
		AddCommand("/closefile", 0, "-> closes the current file.");
		AddCommand("/loadtexture", 0, "-> loads a texture from a .png, .jpg or .bmp file.");
        AddCommand("/loadtexture:", -1, "PATH -> loads a texture from a .png, .jpg or .bmp file.");
        AddCommand("/sensitivity", 1, "NUMBER -> sets the sensitivity of the mouse view.");

		AddCommand("/translate", 3, "X Y Z -> translates the selected vertices X Y Z units. Example: /translate 0 2 0 will translate the vertices 2 points up.");
		AddCommand("/setpos", 3, "X Y Z -> moves the selected vertices to the same point in worldspace. Example: /setpos % 2 % will set the vertices Y position to 2 while mantaining the X and Z the same as they were.");
		AddCommand("/scale", 3, "X Y Z -> scales the selected vertices using the center of the vertices as the origin. Negative magnitudes will invert the mesh. Example: /scale 2 2 2");
		AddCommand("/rotate", 3, "X Y Z -> rotates the selected vertices according to a number of degrees. Example: /rotate 90 0 0.");
		AddCommand("/selectall", 0, "-> selects all the vertices of the mesh.");

		AddCommand("/testemptybones", 0, "-> checks if there is any bones with an ID of 0, 0, 0.");
		AddCommand("/selectvertex", 1, "ID -> selects a vertex based on its ID. Example: /selectvertex 35");

		AddCommand("/addtriangle", 3, "A B C -> creates a triangle made of ABC vertices.");
		AddCommand("/undotri", 0, "-> deletes the last triangle.");
		AddCommand("/undo", 0, "-> reverts the last rotation, translation or scale (experimental).");

		AddCommand("/groups", 0, "-> shows a list of all the mesh groups.");
		AddCommand("/creategroup", 1, "NAME -> creates a group with a custom name. Example: /creategroup LeftArm");
		AddCommand("/deletegroup", 1, "NAME -> deletes the desired group. It will not delete the vertices in it, just the list of vertices inside the group.");
		AddCommand("/addtogroup", 1, "NAME -> adds the selected vertices to the desired group.");
		AddCommand("/removefromgroup", 1, "NAME -> unlists the selected vertices of a group.");
		AddCommand("/selectgroup", 1, "NAME -> selects all the vertices listed in the desired group.");
		AddCommand("/unselectgroup", 1, "NAME -> unselects all the vertices listed in the desired group");

		AddCommand("/inverttriangles", 0, "-> inverts the triangles of the mesh.");
		AddCommand("/invertnormals", 0, "-> inverts the normals of the mesh.");

		AddCommand("/renderset", 1, "NUMBER -> changes the render mode. 0 = wireframe, 1 = textured, 2 = vertex only.");
		AddCommand("/lighting", 1, "NUMBER -> changes the state of lighting. 0 = disabled, 1 = enabled.");

		AddCommand("/importobj", 0, "-> shows a file explorer to import an .obj file");
		AddCommand("/importobj:", -1, "PATH -> imports an .obj file from a path");
		AddCommand("/impobjpos", 0, "-> shows a file explorer to import only the vertices of an .obj file. Useful if there is no need to change the triangles.");
		AddCommand("/impobjpos:", 1, "PATH -> shows a file explorer to import only the vertices of an .obj file located in PATH. Useful if there is no need to change the triangles.");
		AddCommand("/exportobj", 0, "-> exports an .obj file of the actual model.");
		AddCommand("/exportobj:", -1, "PATH -> exports an .obj file of the actual model in the desired PATH.");

        AddCommand("/autorig_vertical", 3, "A B C -> tries to auto-rig the selected vertices in vertical mode.");
        AddCommand("/autorig_horizontal", 3, "A B C -> tries to auto-rig the selected vertices in horizontal mode.");

		AddCommand("/saveraw", 0, "-> exports the model as a raw file.");

		// Extra commands:
		AddCommand("/savegroupsasxfbin", 0, "-> saves each mesh group to a different .xfbin (experimental)");

		AddCommand("/github", 0, "-> opens the GitHub of UNSME.");

        AddCommand("/savexfbin", -1, "PATH -> saves the current model to an .xfbin");

		int Pages = Commands.Count / 8;
		if(Commands.Count % 8 > 0) Pages = Pages + 1;
		CommandDescription[0] = CommandDescription[0] + " <color=lime>Total /help pages: " + Pages.ToString() + "</color>";
	}

	void AddCommand(string CommandName, int ParameterNumber, string Description)
	{
		Commands.Add(CommandName);
		Parameters.Add(ParameterNumber);
		CommandDescription.Add(Description);
	}

	public void ConsoleBehaviour(string Command)
	{
		string Cmd = "";
		if(Command.Contains(" "))
			Cmd = Command.Substring(0, Command.IndexOf(' '));
		else
			Cmd = Command;

		int CommandID = -1;
		if(Commands.Contains(Cmd))
		{
			CommandID = Commands.IndexOf(Cmd);
			// Get parameter number of the real command
			int CmdParameters = Parameters[CommandID];

			// Get parameter number of the written command
			int TempParameters = Command.Length - Command.Replace(" ", "").Length;

			if(CmdParameters == TempParameters || (CmdParameters == -1 && CmdParameters != 0))
			{
				List<string> Par = new List<string>(Command.Split(' '));
				if(Par.Count == TempParameters + 1)
				{
					DoCommand(Cmd, Par);
				}
			}
			else
			{
				GetComponent<RenderFile>().ConsoleMessage("\nWrong number of parameters.");
			}
		}
		else
		{
			GetComponent<RenderFile>().ConsoleMessage("\nThe command doesn't exist.");
		}
	}

	public void DoCommand(string CommandID, List<string> Params)
	{	
		Params.RemoveAt(0);
		for(int x = 0; x < Params.Count; x++)
		{
			Params[x] = Params[x].Replace(" ", "");
		}
		switch(CommandID)
		{
			case "/start":
				Command_Start();
			break;
			case "/help":
				if(Params.Count > 0) Command_Help(Params[0]);
				else Command_Help("");
			break;
			case "/help:":
				Command_HelpCmd(Params);
			break;
			case "/closefile":
				Command_CloseFile();
			break;
			case "/loadtexture":
				Command_LoadTexture();
			break;
            case "/loadtexture:":
                Command_LoadTexturePath(Params);
            break;
			case "/sensitivity":
				Command_Sensitivity(Params);
			break;
			case "/translate":
				Command_Translate(Params);
			break;
			case "/setpos":
				Command_SetPos(Params);
			break;
			case "/scale":
				Command_Scale(Params);
			break;
			case "/rotate":
				Command_Rotate(Params);
			break;
			case "/selectall":
				Command_SelectAll();
			break;
			case "/selectvertex":
				Command_SelectVertex(Params);
			break;
			case "/addtriangle":
				Command_AddTriangle(Params);
			break;
			case "/undotriangle":
				Command_UndoTriangle();
			break;
			case "/undo":
				Command_Undo();
			break;
			case "/groups":
				Command_Groups();
			break;
			case "/creategroup":
				Command_CreateGroup(Params);
			break;
			case "/deletegroup":
				Command_DeleteGroup(Params);
			break;
			case "/addtogroup":
				Command_AddToGroup(Params);
			break;
			case "/removefromgroup":
				Command_RemoveFromGroup(Params);
			break;
			case "/selectgroup":
				Command_SelectGroup(Params);
			break;
			case "/unselectgroup":
				Command_UnselectGroup(Params);
			break;
			case "/inverttriangles":
				Command_InvertTriangles();
			break;
			case "/invertnormals":
				Command_InvertNormals();
			break;
			case "/renderset":
				Command_Renderset(Params);
			break;
			case "/lighting":
				Command_Lighting(Params);
			break;
			case "/importobj":
				Command_ImportObj();
			break;
			case "/importobj:":
				Command_ImportObjPath(Params);
			break;
			case "/impobjpos":
				Command_ImportObjPos();
			break;
			case "/impobjpos:":
				Command_ImportObjPosPath(Params);
			break;
			case "/exportobj":
				Command_ExportObj();
			break;
			case "/exportobj:":
				Command_ExportObjPath(Params);
			break;
            case "/autorig_vertical":
                //Command_AutorigVertical(Params);
            break;
            case "/autorig_horizontal":
                //Command_AutorigHorizontal(Params);
            break;
            case "/saveraw":
				Command_SaveRaw();
			break;
			case "/savegroupsasxfbin":
				Command_SaveGroupsAsXfbin();
			break;
			case "/github":
				System.Diagnostics.Process.Start("https://github.com/zealottormunds/unsme");
			break;
            case "/savexfbin":
                Command_SaveXfbin(Params);
                break;
		}
	}

    public void Command_SaveXfbin(List<string> Param)
    {
        string path = "";
        for(int x = 0; x < Param.Count; x++)
        {
            path = path + Param[x];
            if (x < Param.Count - 1) path = path + " ";
        }

        DialogResult res = DialogResult.Yes;
        if (R.groupCount > 1)
        {
            res = MessageBox.Show("This model has more than 1 group. Importing a brand new .obj is not recommended and the .xfbin might break. If you have exported this model with the tool and want to import it back after edition, then use /impobjpos. It will import all the vertex positions without changing the count.\n\nDo you want to import an .obj regardless?", "", MessageBoxButtons.YesNo);
        }

        if (res == DialogResult.Yes)
        {
            R.SaveModelToXfbin(path, true);
            R.wasObjImported = true;
        }
    }

    public void Command_Start()
	{
		R.ConsoleMessage("\n" + CommandDescription[0]);
	}

	public void Command_Help(string Cmd)
	{
		int a = 0;
		int Pages = Commands.Count / 8;
		if(Commands.Count % 8 > 0) Pages = Pages + 1;

		if(int.TryParse(Cmd, out a) && int.Parse(Cmd) - 1 <= Pages)
		{
			string ListOfCommands = "";
			for(int x = 0; x < 8; x++)
			{
				if(Commands.Count >= x + (8 * a)) ListOfCommands = ListOfCommands + Commands[x + (8 * a)] +  ", ";
			}

			ListOfCommands = ListOfCommands.Substring(0, ListOfCommands.Length - 2);

			R.ConsoleMessage("\n<color=cyan>Available commands in page " + a.ToString() + ":</color> " + ListOfCommands);
		}
		else if(int.TryParse(Cmd, out a) == true && int.Parse(Cmd) - 1 > Pages)
		{
			R.ConsoleMessage("\n<color=orange>This page doesn't exist.</color>");
		}
		else if(int.TryParse(Cmd, out a) == false)
		{
			if(Commands.Contains(Cmd))
			{
				int ID = Commands.IndexOf(Cmd);
				string ConsoleText = "";
				ConsoleText = "<color=cyan>Cmd</color>" + " " + CommandDescription[ID];
				R.ConsoleMessage(ConsoleText);
			}
			else
			{
				R.ConsoleMessage("\n<color=orange>The command doesn't exist.</color>");
			}
		}
	}

	public void Command_HelpCmd(List<string> Param)
	{
		if(Commands.Contains(Param[0]))
		{
			int ID = -1;
			ID = Commands.IndexOf(Param[0]);
			R.ConsoleMessage("\n<color=cyan>" + Commands[ID] + " " + CommandDescription[ID] + "</color>");
		}
		else
		{
			R.ConsoleMessage("\n<color=orange>The command doesn't exist.</color>");
		}
	}

	public void Command_CloseFile()
	{
		DialogResult result = MessageBox.Show("Do you want to close this file?", "Are you sure?", MessageBoxButtons.OKCancel);
		if(result == DialogResult.OK)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		}
	}

	public void Command_LoadTexture()
	{
		R.LoadTexture();
	}
    
    public void Command_LoadTexturePath(List<string> Param)
    {
        string path = "";
        for (int x = 0; x < Param.Count; x++)
        {
            path = path + Param[x];
            if (x < Param.Count - 1) path = path + " ";
        }

        if (System.IO.File.Exists(path))
        {
            DialogResult res = DialogResult.Yes;
            if (res == DialogResult.Yes)
            {
                R.LoadTexture(path);
            }
        }
        else
        {
            R.ConsoleMessage("\n<color=red>The file doesn't exist.</color>");
        }

    }

	public void Command_Sensitivity(List<string> Params)
	{
		float X_ = 0f;

		if(float.TryParse(Params[0], out X_))
		{
			GetComponent<CameraMovement>().SensitivityMouse = X_;
			string CFG_Sensitivity = GetComponent<CameraMovement>().SensitivityMouse.ToString();
			R.ConsoleMessage("\n" + "<color=lime>Saved sensitivity settings.</color>");
			System.IO.File.WriteAllText(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg", CFG_Sensitivity);
		}
		else
		{
			R.ConsoleMessage("\nIncorrect input.");
		}
	}

	public void Command_Translate(List<string> Params)
	{
		float X = 0;
		float Y = 0;
		float Z = 0;
		if(float.TryParse(Params[0], out X) == true && float.TryParse(Params[1], out Y) == true && float.TryParse(Params[2], out Z) == true)
		{
			if(R.selectedVertex.ToArray().Length == 0)
			{
				R.ConsoleMessage("\n" + "<color=orange>No vertices selected.</color>");
			}
			else
			{
				for(int x = 0; x < R.selectedVertex.Count; x++)
				{
					GameObject vertex_ = R.selectedVertex[x];
					vertex_.transform.position = vertex_.transform.position + new Vector3(X, Y, Z);
					R.meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
				}
				R.vertexPosition = R.meshVertices;
				R.mf.mesh.vertices = R.meshVertices.ToArray();
				R.ConsoleMessage("\n" + "<color=yellow>Translated " + R.selectedVertex.ToArray().Length + " vertices in " + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ".</color>");
			}
		}
		else
		{
			R.ConsoleMessage("\nWrong type of parameters.");
		}
	}

	public void Command_SetPos(List<string> Params)
	{
		float X = 0;
		float Y = 0;
		float Z = 0;
		bool MoveX = true;
		bool MoveY = true;
		bool MoveZ = true;
		bool Error = false;

		if(Params[0] == "%")
		{
			MoveX = false;
		}
		else if(float.TryParse(Params[0], out X) == true)
		{
			X = float.Parse(Params[0]);
		}
		else
		{
			Error = true;
		}

		if(Params[1] == "%")
		{
			MoveY = false;
		}
		else if(float.TryParse(Params[1], out Y) == true)
		{
			Y = float.Parse(Params[1]);
		}
		else
		{
			Error = true;
		}

		if(Params[2] == "%")
		{
			MoveZ = false;
		}
		else if(float.TryParse(Params[2], out Z) == true)
		{
			Z = float.Parse(Params[2]);
		}
		else
		{
			Error = true;
		}

		if(Error == false)
		{
			if(R.selectedVertex.ToArray().Length == 0)
			{
				R.ConsoleMessage("\n" + "<color=orange>No vertices selected.</color>");
			}
			else
			{
				for(int x = 0; x < R.selectedVertex.Count; x++)
				{
					GameObject vertex_ = R.selectedVertex[x];
					Vector3 newPos = R.selectedVertex[x].transform.position;
					if(MoveX)
					{
						newPos.x = X;
					}
					if(MoveY)
					{
						newPos.y = Y;
					}
					if(MoveZ)
					{
						newPos.z = Z;
					}
					vertex_.transform.position = newPos;
					R.meshVertices[int.Parse(vertex_.name)] = vertex_.transform.position;
				}
				R.vertexPosition = R.meshVertices;
				R.mf.mesh.vertices = R.meshVertices.ToArray();
				R.ConsoleMessage("\n" + "<color=yellow>Changed the position of " + R.selectedVertex.ToArray().Length + " vertices.</color>");
			}
		}
		else
		{
			R.ConsoleMessage("\nWrong type of parameters.");
		}
	}

	public void Command_Scale(List<string> Params)
	{
		float X = 0;
		float Y = 0;
		float Z = 0;
		if(float.TryParse(Params[0], out X) == true && float.TryParse(Params[1], out Y) == true && float.TryParse(Params[2], out Z) == true)
		{
			if(R.selectedVertex.ToArray().Length == 0)
			{
				R.ConsoleMessage("\n" + "<color=orange>No vertices selected.</color>");
			}
			else
			{
				R.ScaleModel(X, Y, Z, false);
			}
		}
		else
		{
			R.ConsoleMessage("\nWrong type of parameters.");
		}
	}

	public void Command_Rotate(List<string> Params)
	{
		float X = 0;
		float Y = 0;
		float Z = 0;
		if(float.TryParse(Params[0], out X) == true && float.TryParse(Params[1], out Y) == true && float.TryParse(Params[2], out Z) == true)
		{
			if(R.selectedVertex.ToArray().Length == 0)
			{
				R.ConsoleMessage("\n" + "<color=orange>No vertices selected.</color>");
			}
			else
			{
				R.RotateModel(X, Y, Z, false);
			}
		}
		else
		{
			R.ConsoleMessage("\nWrong type of parameters.");
		}
	}

	public void Command_SelectAll()
	{
		for(int x = 0; x < GameObject.Find("Model Data").transform.childCount; x++)
		{
			GameObject vertex_ = GameObject.Find("Model Data").transform.Find(x.ToString()).gameObject;
			if(vertex_.gameObject.GetComponent<VertexObject>().Selected == false)
			{ 
				vertex_.gameObject.GetComponent<VertexObject>().SelectObject();
			}
		}
		R.ConsoleMessage("\n" + "<color=yellow>Selected " + GameObject.Find("Model Data").transform.childCount.ToString() + " vertices.</color>");
	}

	public void Command_TestEmptyBones()
	{
		DialogResult dial = DialogResult.OK;
		for(int x = 0; x < GameObject.Find("Model Data").transform.childCount; x++)
		{
			if(R.vertexBone[x] == Vector4.zero || R.vertexWeight[x] == Vector4.zero)
			{
				dial = MessageBox.Show("Vertex " + x.ToString() + ", B(0, 0, 0, 0), W(0, 0, 0, 0).", "Bone information", MessageBoxButtons.OKCancel);
			}
			if(dial == DialogResult.Cancel)
			{
				x = GameObject.Find("Model Data").transform.childCount;
				break;
			}
		}
	}

	public void Command_SelectVertex(List<string> Params)
	{
		int V = -1;
		if(int.TryParse(Params[0], out V) == true && GameObject.Find(V.ToString()) != null)
		{
			if(GameObject.Find(V.ToString()).GetComponent<VertexObject>().Selected == false) GameObject.Find(V.ToString()).GetComponent<VertexObject>().SelectObject();
		}
		else
		{
			R.ConsoleMessage("\nIncorrect input or the vertex doesn't exist.");
		}
	}

	public void Command_AddTriangle(List<string> Params)
	{
		int X = 0;
		int Y = 0;
		int Z = 0;
		if(int.TryParse(Params[0], out X) == true && int.TryParse(Params[1], out Y) == true && int.TryParse(Params[2], out Z) == true)
		{
			R.meshTriangles.Add(X);
			R.meshTriangles.Add(Y);
			R.meshTriangles.Add(Z);

			R.mf.mesh.triangles = R.meshTriangles.ToArray();
		}
		else
		{
			R.ConsoleMessage("\nIncorrect input");
		}
	}

	public void Command_UndoTriangle()
	{
		R.meshTriangles.RemoveAt(R.meshTriangles.Count - 1);
		R.meshTriangles.RemoveAt(R.meshTriangles.Count - 1);
		R.meshTriangles.RemoveAt(R.meshTriangles.Count - 1);

		R.mf.mesh.triangles = R.meshTriangles.ToArray();
	}

	public void Command_Undo()
	{
		R.Undo();
	}

	public void Command_Groups()
	{
		GroupSelection g = GetComponent<GroupSelection>();
		R.ConsoleMessage("\n<color=yellow>Groups: </color>");
		for(int x = 0; x < g.Groups.Count; x++)
		{
			R.ConsoleMessage("\"" + g.GroupNames[x] + "\" ");
		}
		if(g.Groups.Count == 0)
		{
			R.ConsoleMessage("None");
		}
	}

	public void Command_CreateGroup(List<string> Params)
	{
		string gName = Params[0];
		GroupSelection g = GetComponent<GroupSelection>();
		if(gName != "")
		{
			if(g.GroupNames.Contains(gName) == false)
			{
				g.GroupNames.Add(gName);
				g.Groups.Add(new List<int>());
				R.ConsoleMessage("\n<color=cyan>Group \"" + gName + "\" has been created. Select vertices and add them to the group with /addtogroup NAME.</color>");
			}
			else
			{
				R.ConsoleMessage("\nA group with this name already exists.");
			}
		}
		else
		{
			R.ConsoleMessage("\nInsert a name.");
		}
	}

	public void Command_DeleteGroup(List<string> Params)
	{
		string gName = Params[0];
		GroupSelection g = GetComponent<GroupSelection>();
		if(gName != "" && g.GroupNames.Count > 0)
		{
			if(g.GroupNames.Contains(gName) == true)
			{
				int ID = g.GroupNames.IndexOf(gName);
				g.GroupNames.Remove(gName);
				g.Groups.RemoveAt(ID);
				R.ConsoleMessage("\n<color=orange>Group \"" + gName + "\" has been deleted.</color>");
			}
			else
			{
				R.ConsoleMessage("\nThe group doesn't exist.");
			}
		}
		else
		{
			R.ConsoleMessage("\nInsert a name.");
		}
	}

	public void Command_AddToGroup(List<string> Params)
	{
		string gName = Params[0];
		GroupSelection g = GetComponent<GroupSelection>();
		if(gName != "" && g.GroupNames.Count > 0)
		{
			if(g.GroupNames.Contains(gName))
			{
				int ID = g.GroupNames.IndexOf(gName);
				for(int x = 0; x < R.selectedVertex.Count; x++)
				{
					g.Groups[ID].Add(R.selectedVertex[x].transform.GetSiblingIndex());
				}
				R.ConsoleMessage("\n<color=cyan>Added vertices to group " + "\"" + gName + "\".</color>");
			}
			else
			{
				R.ConsoleMessage("\nThe group doesn't exist.");
			}
		}
		else
		{
			R.ConsoleMessage("\nInsert a name.");
		}
	}

	public void Command_RemoveFromGroup(List<string> Params)
	{
		string gName = Params[0];
		GroupSelection g = GetComponent<GroupSelection>();
		if(gName != "" && g.GroupNames.Count > 0)
		{
			if(g.GroupNames.Contains(gName))
			{
				int ID = g.GroupNames.IndexOf(gName);
				for(int x = 0; x < R.selectedVertex.Count; x++)
				{
					if(g.Groups[ID].Contains(R.selectedVertex[x].transform.GetSiblingIndex()))
					{
						g.Groups[ID].Remove(R.selectedVertex[x].transform.GetSiblingIndex());
					}
				}
				R.ConsoleMessage("\n<color=cyan>Removed vertices from group " + "\"" + gName + "\".</color>");
			}
			else
			{
				R.ConsoleMessage("\nThe group doesn't exist.");
			}
		}
		else
		{
			R.ConsoleMessage("\nInsert a name.");
		}
	}

	public void Command_SelectGroup(List<string> Params)
	{
		string gName = Params[0];
		GroupSelection g = GetComponent<GroupSelection>();
		if(gName != "" && g.GroupNames.Count > 0)
		{
			if(g.GroupNames.Contains(gName) == true)
			{
				int ID = g.GroupNames.IndexOf(gName);
				GetComponent<GroupSelection>().SelectGroupByID(ID);
				R.ConsoleMessage("\nSelected group.");
			}
			else
			{
				R.ConsoleMessage("\nThe group doesn't exist.");
			}
		}
		else
		{
			R.ConsoleMessage("\nInsert a name.");
		}
	}

	public void Command_UnselectGroup(List<string> Params)
	{
		string gName = Params[0];
		GroupSelection g = GetComponent<GroupSelection>();
		if(gName != "" && g.GroupNames.Count > 0)
		{
			if(g.GroupNames.Contains(gName) == true)
			{
				int ID = g.GroupNames.IndexOf(gName);
				GetComponent<GroupSelection>().UnselectGroupByID(ID);
				R.ConsoleMessage("\nUnselected group.");
			}
			else
			{
				R.ConsoleMessage("\nThe group doesn't exist.");
			}
		}
		else
		{
			R.ConsoleMessage("\nInsert a name.");
		}
	}

	public void Command_InvertTriangles()
	{
		R.InvertTriangles();
	}

	public void Command_InvertNormals()
	{
		R.InvertNormals();
	}

	public void Command_Renderset(List<string> Params)
	{
		int Mode = -1;

		if(int.TryParse(Params[0], out Mode) == true)
		{
			if(Mode == 1)
			{
				R.RenderedMesh.SetActive(true);
				R.RenderedMesh.GetComponent<Renderer>().material = R.RenderedMesh.GetComponent<RenderMaterial>().Materials_[0];
				R.ConsoleMessage("\n" + "<color=yellow>Changed rendering mode to TEXTURED.</color>");
			}
			else if(Mode == 0)
			{
				R.RenderedMesh.SetActive(true);
				R.RenderedMesh.GetComponent<Renderer>().material = R.RenderedMesh.GetComponent<RenderMaterial>().Materials_[1];
				R.ConsoleMessage("\n" + "<color=yellow>Changed rendering mode to WIREFRAME.</color>");
			}
			else if(Mode == 2)
			{
				R.RenderedMesh.SetActive(false);
				R.ConsoleMessage("\n" + "<color=yellow>Changed rendering mode to VERTEX ONLY.</color>");
			}
			else
			{
				R.ConsoleMessage("\n" + "<color=red>Render mode not found.</color>");
			}
		}
		else
		{
			R.ConsoleMessage("\n<color=red>Incorrect input.");
		}
	}

	public void Command_Lighting(List<string> Params)
	{
		int Mode = -1;

		if(int.TryParse(Params[0], out Mode) == true)
		{
			if(Mode == 0)
			{
				GameObject.Find("Directional light").GetComponent<Light>().intensity = 0;
				R.ConsoleMessage("\n" + "<color=yellow>Disabled lighting.</color>");
			}
			else if(Mode == 1)
			{
				GameObject.Find("Directional light").GetComponent<Light>().intensity = 1;
				R.ConsoleMessage("\n" + "<color=yellow>Enabled lighting.</color>");
			}
			else
			{
				R.ConsoleMessage("\n" + "<color=red>Incorrect input.</color>");
			}
		}
		else
		{
			R.ConsoleMessage("\n<color=red>Incorrect input.</color>");
		}
	}

	public void Command_ImportObj()
	{
		OpenFileDialog openFileDialog1 = new OpenFileDialog();
		openFileDialog1.DefaultExt = ".obj";

		openFileDialog1.ShowDialog();

		if(openFileDialog1.FileName != "" && File.Exists(openFileDialog1.FileName))
		{
			DialogResult res = DialogResult.Yes;
			if(R.groupCount > 1)
			{
				//res = MessageBox.Show("This model has more than 1 group. Importing a brand new .obj is not recommended and the .xfbin might break. If you have exported this model with the tool and want to import it back after edition, then use /impobjpos. It will import all the vertex positions without changing the count.\n\nDo you want to import an .obj regardless?", "", MessageBoxButtons.YesNo);
			}

			if(res == DialogResult.Yes)
			{
                //R.ImportModel(openFileDialog1.FileName);
                R.ImportModelRE2(openFileDialog1.FileName);
				R.wasObjImported = true;
			}
		}
		else
		{
			R.ConsoleMessage("\n<color=red>Error importing model.</color>");
		}
	}

	public void Command_ImportObjPath(List<string> Param)
	{
        string path = "";
        for (int x = 0; x < Param.Count; x++)
        {
            path = path + Param[x];
            if (x < Param.Count - 1) path = path + " ";
        }

        if (System.IO.File.Exists(path))
		{
			DialogResult res = DialogResult.Yes;
			if(R.groupCount > 1)
			{
				res = MessageBox.Show("This model has more than 1 group. Importing a brand new .obj is not recommended and the .xfbin might break. If you have exported this model with the tool and want to import it back after edition, then use /impobjpos. It will import all the vertex positions without changing the count.\n\nDo you want to import an .obj regardless?", "", MessageBoxButtons.YesNo);
			}

			if(res == DialogResult.Yes)
			{
				R.ImportModelRE2(path);
				R.wasObjImported = true;
			}
		}
		else
		{
			R.ConsoleMessage("\n<color=red>The file doesn't exist.</color>");
		}
	}

	public void Command_ImportObjPos()
	{
		OpenFileDialog openFileDialog1 = new OpenFileDialog();
		openFileDialog1.DefaultExt = ".obj";

		openFileDialog1.ShowDialog();

		if(openFileDialog1.FileName != "" && File.Exists(openFileDialog1.FileName))
		{
			R.ImportModelPos(openFileDialog1.FileName);
		}
		else
		{
			R.ConsoleMessage("\n<color=red>Error importing model.</color>");
		}
	}

	public void Command_ImportObjPosPath(List<string> Params)
	{
		if(System.IO.File.Exists(Params[0]))
		{
			R.ImportModelPos(Params[0]);
		}
		else
		{
			R.ConsoleMessage("\n<color=red>The file doesn't exist.</color>");
		}
	}

	public void Command_ExportObj()
	{
		SaveFileDialog saveFileDialog1 = new SaveFileDialog();
		saveFileDialog1.DefaultExt = ".obj";

		saveFileDialog1.ShowDialog();

		if(saveFileDialog1.FileName != "")
		{
			R.ExportToObj(saveFileDialog1.FileName);
		}
		else
		{
			R.ConsoleMessage("\nError exporting model.");
		}
	}

	public void Command_ExportObjPath(List<string> Param)
	{
        string path = "";
        for (int x = 0; x < Param.Count; x++)
        {
            path = path + Param[x];
            if (x < Param.Count - 1) path = path + " ";
        }

        R.ExportToObj(path);
	}

    static int SortByScore(GameObject y1, GameObject y2)
    {
        return y1.transform.position.y.CompareTo(y2.transform.position.y);
    }

    public void Command_AutorigVertical(List <string> Params)
    {
        List<GameObject> VerticesToRig = R.selectedVertex;
        VerticesToRig.Sort(SortByScore);

        float MinimumY = VerticesToRig[0].transform.position.y;
        float MaximumY = VerticesToRig[VerticesToRig.Count - 1].transform.position.y;
    }

    public void Command_AutorigHorizontal(List<string> Params)
    {

    }

    public void Command_SaveRaw()
	{
		FolderBrowserDialog op = new FolderBrowserDialog();
		op.ShowDialog();

		if(op.SelectedPath == "") return;

		R.SaveModel(op.SelectedPath);
	}

	public void Command_SaveGroupsAsXfbin()
	{
		DialogResult dial = MessageBox.Show("Fix vertex count and face count? This fix is only useful if you have used /importobj. However, /impobjpos does not need it.", "Warning", MessageBoxButtons.YesNo);
		GetComponent<SaveEachGroup>().dial = dial;
		GetComponent<SaveEachGroup>().SaveEach();
	}
}
