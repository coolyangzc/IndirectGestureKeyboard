using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Lexicon : MonoBehaviour 
{
	public Image keyboard, candidatesList;
	public RawImage radialMenu, textWindow;
	public Info info;
	public Gesture gesture;
	public Lexicon lexicon;
    public TextManager textManager;
	public string text = "", under = "";
	
	//Constants
	private const int LexiconSize = 10000;
	private const int SampleSize = 32;
	private const int RadialNum = 5;
	private const int CandidatesNum = 13;
	private const float AnyStartThr = 2.5f;
    private const float eps = Parameter.eps;

    public static Vector2 StartPoint;
	public static Vector2 StartPointRelative = new Vector2(0f, 0f);

    //Parameters
    public static bool isCut = false;
	public static bool useRadialMenu = true;

	//Internal Variables
    private string[] words;
	private Text[] radialText = new Text[RadialNum], listText = new Text[CandidatesNum];
    private Image[] candidateBtn = new Image[CandidatesNum];
    private int panel;
	private Candidate[] cands = new Candidate[CandidatesNum], candsTmp = new Candidate[CandidatesNum];
	private Vector2[] keyPos = new Vector2[26];

	private List<string> phrase = new List<string>();
	private List<int> phraseList = new List<int>();
	private List<Candidate> history = new List<Candidate>();
    private List<Entry> dict = new List<Entry>();
    private Dictionary<string, float> katzAlpha = new Dictionary<string, float>();
    private Dictionary<string, float> bigramMap = new Dictionary<string, float>();

    public class Entry
	{
		public string word;
		public long frequency;
		public List<Vector2> pts = new List<Vector2>();
		public Vector2[][] locationSample = new Vector2[3][];
		public Vector2[][] shapeSample = new Vector2[3][];
		public Entry(string word, long frequency, Lexicon lexicon)
		{
			this.word = word;
			this.frequency = frequency;
			int key = -1;
			for (int i = 0; i < word.Length; ++i)
			{
				key = word[i] - 'a';
				pts.Add(lexicon.keyPos[key]);
			}
			locationSample[0] = PathCalc.TemporalSampling(pts.ToArray());
            bool ins = false;
            if (Vector2.Distance(pts[0], StartPoint) > eps)
                ins = true;
            if (ins)
                pts.Insert(0, StartPoint);
			locationSample[1] = PathCalc.TemporalSampling(pts.ToArray());
            if (ins)
                pts.RemoveAt(0);
			for(int i = 0; i < (int)Parameter.Mode.End; ++i)
				shapeSample[i] = PathCalc.Normalize(locationSample[i]);
		}
	}

	public class Candidate
	{
		public string word;
		public float location, shape, language, confidence;
		public Candidate(string word_ = "")
		{
			word = word_;
			confidence = -Parameter.inf;
		}
		public Candidate(Candidate x)
		{
			word = x.word;
			location = x.location;
			shape = x.shape;
			confidence = x.confidence;
		}
	}

	// Use this for initialization
	void Start () 
	{
		info.Log("Mode", Parameter.mode.ToString());
		//info.Log("[L]ocation", locationFormula.ToString());
		//info.Log("[S]hape", shapeFormula.ToString());
		history.Clear();
		ChangeCandidatesChoose(false);
		SetRadialMenuDisplay(false);
		//CalcKeyLayout();
		//CalcLexicon();
		InitPhrases();
	}

	// Update is called once per frame
	void Update () 
	{
        
	}

	public void CalcKeyLayout()
	{
		for (int i = 0; i < 26; ++i)
		{
			RectTransform key = keyboard.rectTransform.Find(((char)(i + 65)).ToString()).GetComponent<RectTransform>();
			keyPos[i] = new Vector2(key.localPosition.x, key.localPosition.y);
		}
		StartPoint = keyPos[6]; //keyG
	}

	public void CalcLexicon()
	{
        TextAsset textAsset = Resources.Load("bigrams-LDC-10k-katz-1m-phrase") as TextAsset;
        string[] lines = textAsset.text.Split('\n');
        int bigramsNum = int.Parse(lines[0]);
        if (bigramMap.Count == 0)
        {
            for (int i = 1; i <= bigramsNum; ++i)
            {
                string[] data = lines[i].Split(' ');
                bigramMap.Add(data[0] + ' ' + data[1], float.Parse(data[2]));
            }
        }
        int unigramsNum = int.Parse(lines[bigramsNum + 1]);
        Debug.Log("Unigram: " + unigramsNum);
        dict.Clear();
        for (int i = 0; i < unigramsNum; ++i)
        {
            string[] data = lines[i + bigramsNum + 2].Split(' ');
            if (!katzAlpha.ContainsKey(data[0]))
                katzAlpha.Add(data[0], float.Parse(data[2]));
            if (data[0] == "<s>")
                continue;
            Entry entry = new Entry(data[0], long.Parse(data[1]), this);
            dict.Add(entry);
        }
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
        SetPhraseList(Parameter.UserStudy.Study1);
        ChangePhrase();
	}

    public void SetPhraseList(Parameter.UserStudy study)
    {
        for (int i = 0; i < phrase.Count; ++i)
            phraseList[i] = i;
        for (int i = 2; i < phrase.Count; ++i)
        {
            int j = Random.Range(2, phrase.Count);
            int k = phraseList[i];
            phraseList[i] = phraseList[j];
            phraseList[j] = k;
        }
        if (study == Parameter.UserStudy.Study2)
        {
            for (int b = 1; b < 8; ++b)
                phraseList[b * 10] = phraseList[1];
        }
    }

	public Candidate[] Recognize(Vector2[] rawStroke)
	{
		Vector2[] stroke = PathCalc.TemporalSampling(rawStroke);

		for (int i = 0; i < CandidatesNum; ++i)
			candsTmp[i] = new Candidate();
        int h = history.Count;
        string pre = (h == 0) ? "<s>" : history[h - 1].word.ToLower();
        Debug.Log(pre);
        foreach (Entry entry in dict)
		{
            if (rawStroke.Length == 1 && entry.word.Length != 1)
                continue;
            Candidate newCandidate = new Candidate
            {
                word = entry.word
            };
            if (Parameter.mode == Parameter.Mode.AnyStart)
			{
				if (Vector2.Distance(entry.locationSample[0][0], stroke[0]) > AnyStartThr * Parameter.keyWidth)
					continue;
				entry.pts.Insert(0, stroke[0]);
				entry.locationSample[(int)Parameter.mode] = PathCalc.TemporalSampling(entry.pts.ToArray());
				entry.shapeSample[(int)Parameter.mode] = PathCalc.Normalize(entry.locationSample[(int)Parameter.mode]);
				entry.pts.RemoveAt(0);
			}
            if (Vector2.Distance(stroke[SampleSize - 1], entry.locationSample[(int)Parameter.mode][SampleSize - 1]) 
                > Parameter.endOffset * Parameter.keyWidth && 
                (h >= words.Length || entry.word != words[h]))
                continue;

            float biF = 0;
            if (bigramMap.ContainsKey(pre + ' ' + entry.word))
                biF = bigramMap[pre + ' ' + entry.word];
            else
            {
                biF = katzAlpha[pre] * entry.frequency;
                if (biF == 0)
                    continue;
            }
            newCandidate.language = Mathf.Log(biF);
            newCandidate.location = PathCalc.Match(stroke, entry.locationSample[(int)Parameter.mode], Parameter.locationFormula,
                (newCandidate.language - candsTmp[CandidatesNum - 1].confidence) / 100 * Parameter.keyboardWidth * SampleSize);
            if (newCandidate.location == Parameter.inf)
                continue;

            newCandidate.confidence = newCandidate.language - 100 * newCandidate.location;
            //if (newCandidate.confidence < candsTmp[CandidatesNum - 1].confidence)
				//continue;
			for (int i = 0; i < CandidatesNum; ++i)
				if (newCandidate.confidence > candsTmp[i].confidence)
				{
					for (int j = CandidatesNum - 1; j > i; j--)
						candsTmp[j] = candsTmp[j-1];
					candsTmp[i] = newCandidate;
					break;
				}
		}
        if (h < words.Length)
            for (int i = 0; i < CandidatesNum; ++i)
                if (candsTmp[i].word == words[h].ToLower())
                    candsTmp[i].word = words[h];
        return candsTmp;
	}

	public void SetCandidates(Candidate[] candList)
	{
		for (int i = 0; i < candList.Length; ++i)
        {
            cands[i] = candList[i];
            listText[i].text = cands[i].word;
        }
			
        panel = -1;
        NextCandidatePanel();

        if (cands[0].confidence == 0)
			return;
        textManager.SetCandidate(cands[0].word);
	}

    public void NextCandidatePanel()
    {
        panel = (panel + 1) % 3;
        for (int i = 0; i <= 4; ++i)
        {
            int id = (i == 0) ? 0 : panel * 4 + i;
            string text = cands[id].word;
            if (Parameter.debugOn)
                text += "\n" + cands[id].location.ToString(".000") + " " + cands[id].language.ToString(".000") 
                    + " " + cands[id].confidence.ToString(".000");
            radialText[i].text = text;
        }
    }

	public string Accept(ref int id)
	{
        if (useRadialMenu)
            id = (id == 0)? 0 : panel * 4 + id;
        string word = cands[id].word;
        for (int i = 0; i < CandidatesNum; ++i)
            cands[i].word = "";
        for (int i = 0; i < RadialNum; ++i)
            radialText[i].text = "";

        if (word != "")
        {
            Debug.Log("Accept " + word);
            textManager.AddWord(word);
            history.Add(new Candidate(word));
        }
        else
            word = "#";
		
		return word;
	}

	public void Delete()
	{
		for (int i = 0; i < CandidatesNum; ++i)
		{
			cands[i].word = "";
		}
		if (history.Count == 0)
			return;
		history.RemoveAt(history.Count - 1);
        textManager.Delete(history);
	}

    public void CleanList()
    {
        for (int i = 0; i < CandidatesNum; ++i)
            listText[i].text = "";
    }

	public void Clear()
	{
		for (int i = 0; i < CandidatesNum; ++i)
		{
			//btn[i].GetComponentInChildren<Text>().text = "";
			if (cands[i] != null)
				cands[i].word = "";
		}
		for (int i = 0; i < RadialNum; ++i)
			radialText[i].text = "";
        for (int i = 0; i < CandidatesNum; ++i)
            listText[i].text = "";
        SetRadialMenuDisplay(false);
		gesture.chooseCandidate = false;
		history.Clear();
        textManager.Clear();
	}

	public void SetDebugDisplay(bool debugOn)
	{
		Parameter.debugOn = debugOn;
		if (cands[0] == null || cands[0].word.Length <= 0)
			return;
		SetCandidates(cands);
	}

	public void ChangePhrase(int id = -1)
	{
        if (id == -3)
            textManager.SetPhrase("Thanks for watching");
        else if (id == -2)
            textManager.SetPhrase("a subject one can really enjoy");
        else if (id == -1)
            textManager.SetPhrase(phrase[Random.Range(0, phrase.Count)]);
		else
            textManager.SetPhrase(phrase[phraseList[id]]);
		words = textManager.GetWords();
		Clear();
	}

	public void ChangeCandidatesChoose(bool change)
	{
		for (int i = 0; i < RadialNum; ++i)
			radialText[i] = radialMenu.transform.Find("Candidate" + i.ToString()).GetComponent<Text>();
        for (int i = 0; i < CandidatesNum; ++i)
        {
            candidateBtn[i] = candidatesList.transform.Find("Candidate" + i.ToString()).GetComponent<Image>();
            listText[i] = candidatesList.transform.Find("Candidate" + i.ToString()).GetComponentInChildren<Text>();
        }
            
        useRadialMenu ^= change;
        info.Log("[C]hoose", useRadialMenu ? "Radial" : "List");
        Vector3 pos = textWindow.GetComponent<RectTransform>().localPosition;
        pos.y = useRadialMenu ? 4 : 5.1f;
        textWindow.GetComponent<RectTransform>().localPosition = pos;
        if (useRadialMenu)
            SetListMenuDisplay(false);
        else
            SetListMenuDisplay(true, 4);
    }

    public void SetListMenuDisplay(bool display, int num = CandidatesNum)
    {
        for (int i = 0; i < CandidatesNum; ++i)
        {
            Color c = listText[i].color;
            c.a = (display && i <= num) ? 1f : 0;
            listText[i].color = c;
            c = candidateBtn[i].color;
            c.a = (display && i <= num) ? 0.9f : 0;
            candidateBtn[i].color = c;
        }
    }

    public void HighLightListMenu(int id = -1)
    {
        for (int i = 0; i < CandidatesNum; ++i)
        {
            Color c = candidateBtn[i].color;
            c.r = c.g = c.b = 1;
            candidateBtn[i].color = c;
        }
        if (id > 0)
        {
            Color c = candidateBtn[id].color;
            c.r = c.g = c.b = 0.8f;
            candidateBtn[id].color = c;
        }
    }

	public void SetRadialMenuDisplay(bool display)
	{
		for (int i = 0; i < RadialNum; ++i)
		{
			Color c = radialText[i].color;
			c.a = display?1f:0;
			radialText[i].color = c;
		}
		radialMenu.texture = (Texture)Resources.Load("6Menu/6Menu", typeof(Texture));
		Color color = radialMenu.color;
		color.a = display?0.9f:0;
		radialMenu.color = color;
	}

	public void UpdateSizeMsg(string msg)
	{
		info.Log("Size", msg.Split(' ')[0]);
		info.Log("Scale", msg.Split(' ')[1]);
	}

    public string GetChoosedCandidate(int choose)
    {
        int id = (choose == 0) ? 0 : panel * 4 + choose;
        return cands[id].word;
    }
}
