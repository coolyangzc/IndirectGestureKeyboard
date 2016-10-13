using UnityEngine; 
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class Touchscreen : MonoBehaviour {

    public Text debugInfo;
	public Button btn;
    public string path = "sdcard0/";
	private string fileName = "";
	private float recordTime = float.MaxValue;
	// Use this for initialization
	void Start () 
	{
		DateTime time = DateTime.Now;
		fileName = time.Month.ToString() + "." + time.Day.ToString() + "_" + 
				   time.Hour.ToString() + "-" + time.Minute.ToString() + "-" + time.Second.ToString() + ".txt";
		btn.onClick.AddListener (TapBtn);
		string screenInfo = Screen.height.ToString () + "\n" + Screen.width.ToString ();
        CreateFile (path, fileName, screenInfo);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.touchCount > 0 && Time.time > recordTime + 0.1f) 
		{
			Touch touch = Input.GetTouch(0);
            string str = touch.phase.ToString() + " " + Time.time.ToString() + " " + touch.position.x.ToString() + " " + touch.position.y.ToString();
			CreateFile (path, fileName, str);
        }
	}

	void TapBtn()
	{
		if (recordTime == float.MaxValue) 
		{
			btn.GetComponentInChildren<Text> ().text = "Recording";
			recordTime = Time.time;
			CreateFile (path, fileName, "Start");
		} 
		else 
		{
			btn.GetComponentInChildren<Text> ().text = "Start";
			recordTime = float.MaxValue;
			CreateFile (path, fileName, "Finish");
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
