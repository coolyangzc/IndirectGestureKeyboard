using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Info : MonoBehaviour 
{
	public Text info;
	
	private List<string> logs = new List<string>();
	private List<string> tags = new List<string>();

	private bool changed = false;
	
	// Use this for initialization
	void Start() 
	{
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (!changed)
		{
			return;
		}
		
		info.text = "";
		for (int i = 0; i < logs.Count; ++i)
			info.text += tags[i] + ':' + logs[i] + '\n';
		
		changed = false;
	}
	
	public void Log(string tag, string log)
	{
		if (tags.Contains(tag))
		{
			for (int i = 0; i < tags.Count; ++i)
				if (tags[i] == tag)
				{
					logs[i] = log;
					break;
				}
		}
		else
		{
			tags.Add(tag);
			logs.Add(log);
		}
		changed = true;
	}
}
