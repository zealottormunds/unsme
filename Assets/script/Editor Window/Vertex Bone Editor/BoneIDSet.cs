using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoneIDSet : MonoBehaviour {
	public Dropdown dropDown;

	public void SetValue()
	{
		if(int.Parse(this.GetComponent<InputField>().text) - 1 < dropDown.options.Count)
		{
			dropDown.value = int.Parse(this.GetComponent<InputField>().text) - 1;
		}
		else
		{
			dropDown.value = 0;
			dropDown.RefreshShownValue();
		}
		this.GetComponent<InputField>().text = "";
	}
}
