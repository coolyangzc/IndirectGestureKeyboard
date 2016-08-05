using UnityEngine; 
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class Touchscreen : MonoBehaviour {

    public Text debugInfo;
    public string path = "sdcard0/";
	private string fileName = "";
	// Use this for initialization
	void Start () 
	{
		DateTime time = DateTime.Now;
		fileName = time.Month.ToString() + "." + time.Day.ToString() + "_" + 
				   time.Hour.ToString() + "-" + time.Minute.ToString() + "-" + time.Second.ToString() + ".txt";
		string screenInfo = Screen.height.ToString () + "\n" + Screen.width.ToString ();
        CreateFile (path, fileName, screenInfo);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.touchCount > 0) 
		{
			Touch touch = Input.GetTouch(0);
            string str = touch.phase.ToString() + " " + Time.time.ToString() + " " + touch.position.x.ToString() + " " + touch.position.y.ToString();
			CreateFile (path, fileName, str);
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
        debugInfo.text = t.FullName;
        sw.WriteLine(info);
        sw.Close();
        sw.Dispose();
	}
}
