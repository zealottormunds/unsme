using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandBoxSubmit : MonoBehaviour {
	public RenderFile RenderF;

	public void OnSubmit()
	{
		RenderF.DoCommand();
	}

	public void OnEndEdit()
	{
		GetComponent<InputField>().text = "";
	}
}
