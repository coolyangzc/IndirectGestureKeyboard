using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
    public GameObject nameWindow, textWindow, startBtnObject;
    public Button confirmButton, nextButton, startButton, redoButton;
    public Keyboard keyboard;
    public InputField inputID;

    public Text phraseText;
    public DebugInfo debugInfo;

    private string userID = "";
    private bool warmUp = true, recording = false;
    private int phraseID = 0, highLight = 0;
    private string[] words;

    private List<string> phrase = new List<string>();
    private List<int> phraseList = new List<int>();

    // Use this for initialization
    void Start()
    {
        textWindow.SetActive(false);
        confirmButton.onClick.AddListener(StartStudy);
        startButton.onClick.AddListener(NextBlock);
        nextButton.onClick.AddListener(delegate () { this.NextPhrase(false); });
        redoButton.onClick.AddListener(Redo);
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
            phraseList.Add(i);
        }
        Debug.Log("Phrases: " + phrase.Count);
        for (int i = 2; i < phrase.Count; ++i)
        {
            int j = Random.Range(2, phrase.Count);
            int k = phraseList[i];
            phraseList[i] = phraseList[j];
            phraseList[j] = k;
        }
    }

    public void StartStudy()
    {
        debugInfo.SetPhraseOnly();
        nameWindow.SetActive(false);
        textWindow.SetActive(true);
        phraseID = 0;
        WarmUpPhrase();
    }

    public void WarmUpPhrase()
    {
        debugInfo.Log("Phrase", "Warmup");
        warmUp = true;
        recording = false;
        startBtnObject.SetActive(true);
        NextPhrase();
    }

    public void NextBlock()
    {
        warmUp = false;
        NextPhrase(true);
        startBtnObject.SetActive(false);
    }

    public void NextPhrase(bool force = false)
    {
        if (!warmUp && !force && highLight != words.Length)
            return;
        if (recording)
            keyboard.EndDataFile("Direct");
            
        recording = false;
        
        if (!warmUp)
        {
            if (phraseID % 10 == 0 && !force)
            {
                WarmUpPhrase();
                if (phraseID >= 40)
                    debugInfo.Log("Phrase", "Finish");
                return;
            }
            phraseText.text = phrase[phraseList[phraseID]];
            phraseID++;
            debugInfo.Log("Phrase", phraseID.ToString() + "/40");
            recording = true;
            keyboard.NewDataFile(userID + "_" + phraseID.ToString() + ".txt" + "\n" + phraseText.text, 3);
        }
        else
            phraseText.text = phrase[Random.Range(0, phrase.Count)];

        words = phraseText.text.Split(' ');
        HighLight(-1000);
    }

    public void Redo()
    {
        HighLight(-1000);
        keyboard.Backspace();
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
        if (highLight == words.Length)
            nextButton.GetComponentInChildren<Text>().text = "Next";
        else
            nextButton.GetComponentInChildren<Text>().text = "<color=gray>Next</color>";
    }

}
