using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Client : MonoBehaviour 
{
	public GameObject nameWindow, textWindow;
	public Button confirmButton;
	public Keyboard keyboard;
	public InputField inputID;

	public DebugInfo debugInfo;

    private string userID = "";
    private List<string> phrase = new List<string>();

    // Use this for initialization
    void Start() 
	{
        textWindow.SetActive(false);
        confirmButton.onClick.AddListener(StartStudy);
        InitPhrases();
    }
	
	// Update is called once per frame
	void Update() 
	{

	}

	void OnGUI()
	{
        userID = inputID.text;
	}

    void InitPhrases()
    {
        TextAsset textAsset = Resources.Load("pangrams_phrases") as TextAsset;
        string[] lines = textAsset.text.Split('\n');
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (line.Length <= 1)
                continue;
            if (line[line.Length - 1] < 'a' || line[line.Length - 1] > 'z')
                line = line.Substring(0, line.Length - 1);
            phrase.Add(line);
        }
        Debug.Log("Phrases: " + phrase.Count);
    }

	public void StartStudy()
	{
        debugInfo.SetPhraseOnly();
        nameWindow.SetActive(false);
        textWindow.SetActive(true);
    }
    
}
