using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Windows.Forms;

public class Settings : MonoBehaviour {
	public GameObject WelcomeScreen;
	public GameObject SettingsWindow;
	public Text Sensitivity;

	void OnEnable()
	{
		if(File.Exists(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg"))
		{
			Sensitivity.gameObject.GetComponentInParent<InputField>().text = File.ReadAllText(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg").ToString();
		}
	}

	public void SaveSettings()
	{
		File.WriteAllText(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg", Sensitivity.text);
		MessageBox.Show("Settings saved successfully.");
	}

	public void QuitWindow()
	{
		WelcomeScreen.SetActive(true);
		SettingsWindow.SetActive(false);
	}
}
