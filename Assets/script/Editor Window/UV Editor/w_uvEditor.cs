using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class w_uvEditor : MonoBehaviour {
	private GameObject RenderedMesh;
	private GameObject ModelViewer;

	public void OpenUVEditor()
	{	
		RenderedMesh = GameObject.Find("RENDERED MESH");
		ModelViewer = GameObject.Find("MODEL VIEWER");
		RenderFile r_file = ModelViewer.GetComponent<RenderFile>();
		Mesh actualMesh = r_file.ReturnActualMesh();

		Sprite uvsprite = new Sprite();
		Texture2D _tex = RenderedMesh.GetComponent<RenderMaterial>().Materials_[0].mainTexture as Texture2D;
		uvsprite = Sprite.Create(_tex, new Rect(0, 0, _tex.width, _tex.height), new Vector2());

		GameObject.Find("UV TEXTURE MAP").GetComponent<Image>().sprite = uvsprite;

		foreach(GameObject vertex_ in r_file.selectedVertex)
		{
			GameObject uvCoord = new GameObject();
			uvCoord.name = vertex_.name + "_t";
			uvCoord.transform.SetParent(GameObject.Find("UV TEXTURE MAP").transform);
			Image uvCoordImage = uvCoord.AddComponent<Image>();
			uvCoordImage.color = Color.green;
			uvCoordImage.rectTransform.anchoredPosition = new Vector2(actualMesh.uv[int.Parse(vertex_.name)].x * 128, actualMesh.uv[int.Parse(vertex_.name)].y * -128);
			uvCoordImage.rectTransform.sizeDelta = new Vector2(5, 5);
		}
	}

	public void SaveUVMapping()
	{
		RenderedMesh = GameObject.Find("RENDERED MESH");
		ModelViewer = GameObject.Find("MODEL VIEWER");
		RenderFile r_file = ModelViewer.GetComponent<RenderFile>();
		Mesh actualMesh = r_file.ReturnActualMesh();
		Vector2[] uv_list_new = actualMesh.uv;

		foreach(GameObject vertex_ in r_file.selectedVertex)
		{
			Image uvI = GameObject.Find("UV TEXTURE MAP").transform.Find(vertex_.name.ToString() + "_t").GetComponent<Image>();
			uv_list_new[int.Parse(vertex_.name)] = new Vector2(uvI.rectTransform.anchoredPosition.x / 128, uvI.rectTransform.anchoredPosition.x / -128);
			Destroy(uvI.gameObject);
		}
		r_file.UpdateUV(uv_list_new);
	}

	public void CloseWindow()
	{
		GameObject.Find("MODEL VIEWER").GetComponent<RenderFile>().WindowOpen = false;
		this.gameObject.SetActive(false);
	}
}
