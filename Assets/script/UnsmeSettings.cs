using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnsmeSettings : MonoBehaviour
{
    RenderFile r;

    void Start()
    {
        r = this.GetComponent<RenderFile>();
        StartCoroutine(CheckForUpdates());
    }

    IEnumerator CheckForUpdates()
    {
        WWW www = new WWW("https://pastebin.com/raw/AFmJNVdC");
        yield return www;

        string t = "";
        for (int x = 1; x < www.text.Length; x++)
        {
            t = t + www.text[x];
        }

        float VERSION = 1.75f;

        yield return t;

        float v = float.Parse(t);
        if (v < VERSION)
        {
            r.ConsoleMessage("\n<color=yellow>You seem to have a pre-release or development version. If you see any bugs, report them as soon as possible.</color>");
        }
        if (v == VERSION)
        {
            r.ConsoleMessage("\n<color=yellow>This is the last version available.</color>");
        }
        if (v > VERSION)
        {
            r.ConsoleMessage("\n<color=yellow>There's a new version available! Do /github to open the download page.</color>");
        }
    }
}
