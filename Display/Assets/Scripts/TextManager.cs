using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour {

    public Text inputText, underText, phraseText;
    private int highLight;
    private string text = "", under = "";
    private string[] words;

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void SetPhrase(string phrase)
    {
        phraseText.text = phrase;
        words = phrase.Split(' ');
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

    public bool InputNumberCorrect()
    {
        if (Parameter.userStudy == Parameter.UserStudy.Study1)
            return highLight == words.Length;
        else if (Parameter.userStudy == Parameter.UserStudy.Study2 || Parameter.userStudy == Parameter.UserStudy.Basic)
            return inputText.text.Split(' ').Length == words.Length;
        return false;
    }

    public string[] GetWords()
    {
        return words;
    }

    public void SetCandidate(string word)
    {
        string space = (text.Length > 0) ? " " : "";
        inputText.text = text + space + word;
        underText.text = under + space + Underline('_', word.Length);
    }

    public void CancelCandidate()
    {
        inputText.text = text;
        underText.text = under;
    }

    public void AddWord(string word)
    {
        string space = (text.Length > 0) ? " " : "";
        text += space + word;
        under = Underline(' ', text.Length);
        inputText.text = text;
        underText.text = under;
    }

    public void Delete(List<Lexicon.Candidate> history)
    {
        text = "";
        for (int i = 0; i < history.Count; ++i)
            text += ((i > 0) ? " " : "") + history[i].word;
        Debug.Log(history.Count);
        Debug.Log(text);
        under = Underline(' ', text.Length);
        inputText.text = text;
        underText.text = under;
    }

    public void Clear()
    {
        inputText.text = underText.text = under = text = "";
    }

    private string Underline(char ch, int length)
    {
        string under = "";
        for (int i = 0; i < length; ++i)
            under += ch;
        return under;
    }
}
