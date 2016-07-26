using UnityEngine; 
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class Touchscreen : MonoBehaviour {

    public Text debugInfo;
    public string path = "sdcard0/";
	// Use this for initialization
	void Start () 
	{
        CreateFile (path, "data.txt", "");
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.touchCount > 0) 
		{
			Touch touch = Input.GetTouch(Input.touchCount - 1);
            string str = touch.phase.ToString() + " " + touch.position.x.ToString() + " " + touch.position.y.ToString();
            CreateFile (path, "data.txt", str);
        }
	}

	void CreateFile(string Path, string name, string info)
	{
		StreamWriter sw;
		FileInfo t = new FileInfo(Path + "//" + name);
        
		if (!t.Exists)
            sw = t.CreateText();
        else
            sw = t.AppendText();
        debugInfo.text = t.DirectoryName + t.FullName;
        sw.WriteLine(info);
        sw.Close();
        sw.Dispose();
	}
}
