using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DebugInfo : MonoBehaviour 
{
	public Text info;

	private List<string> logs = new List<string>();
	private List<string> tags = new List<string>();
	private bool changed = false, phraseOnly = false;
    private float nowTime = 0;
    private const float ClearTime = 5;
	
	
	// Use this for initialization
	void Start() 
	{
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (!changed)
		{
			nowTime += Time.deltaTime;
			if (nowTime > ClearTime && !phraseOnly)
			{
				info.text = "";
				logs.Clear();
				tags.Clear();
				nowTime = 0;
			}
			return;
		}

		info.text = "";
		nowTime = 0;
		for (int i = 0; i < logs.Count; ++i)
            if (!phraseOnly || tags[i] == "Phrase")
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
    
    public void ChangeVisibility(bool visible)
    {
        Color c = info.color;
        if (visible)
            c.a = 255;
        else
            c.a = 0;
        info.color = c;
    }

    public void SetPhraseOnly(bool only = true)
    {
        phraseOnly = only;
        changed = true;
    }
}
