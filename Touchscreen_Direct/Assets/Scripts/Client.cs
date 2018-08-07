using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
    public GameObject nameWindow, textWindow;
    public Button confirmButton, nextButton;
    public Keyboard keyboard;
    public InputField inputID;

    public Text phraseText;
    public DebugInfo debugInfo;

    private string userID = "";
    private int phraseID = 0, highLight = 0;
    private string[] words;

    private List<string> phrase = new List<string>();

    // Use this for initialization
    void Start()
    {
        textWindow.SetActive(false);
        confirmButton.onClick.AddListener(StartStudy);
        nextButton.onClick.AddListener(NextPhrase);
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
        debugInfo.Log("Phrase", "Warmup");
        phraseID = 0;
    }

    public void NextPhrase()
    {
        if (phraseID == 0)
            phraseText.text = phrase[Random.Range(0, phrase.Count)];
        words = phraseText.text.Split(' ');
        HighLight(-1000);
    }

    public void HighLight(int delta)
    {
        highLight += delta;
        if (highLight < 0)
            highLight = 0;
        phraseText.text = "";
        for (int i = 0; i < words.Length; ++i)
            if (i == highLight)
                phraseText.text += "<color=red>" + words[i] + "</color> ";
            else
                phraseText.text += words[i] + " ";
    }

}
