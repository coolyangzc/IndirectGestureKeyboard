using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Lexicon : MonoBehaviour 
{
	public Image keyboard, candidates;
	public RawImage radialMenu;
	public Info info;
	public Gesture gesture;
	public Lexicon lexicon;
	public Text inputText, underText, phraseText;
	public string text = "", under = "";
	
	//Constants
	private const int LexiconSize = 10000;
	private const int SampleSize = 32;
	private const int RadialNum = 4;
	private const int CandidatesNum = 12;
	private const float AnyStartThr = 2.5f;
    private const float eps = Parameter.eps;

    public static Vector2 StartPoint;
	public static Vector2 StartPointRelative = new Vector2(0f, 0f);

    //Parameters
    public static bool isCut = false;
	public static bool useRadialMenu = true;

	//Internal Variables
	private int choose = 0;
	private int highLight = 0;
	private string[] words;
	private Button[] btn = new Button[CandidatesNum];
	private Text[] radialText = new Text[RadialNum];
    private int panel;
	private Candidate[] cands = new Candidate[CandidatesNum], candsTmp = new Candidate[CandidatesNum];
	private Vector2[] keyPos = new Vector2[26];

	private List<string> phrase = new List<string>();
	private int[] phraseList = new int[30];
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
        for (int i = 1; i <= bigramsNum; ++i)
        {
            string[] data = lines[i].Split(' ');
            bigramMap.Add(data[0] + ' ' + data[1], float.Parse(data[2]));
        }
        int unigramsNum = int.Parse(lines[bigramsNum + 1]);
        Debug.Log(unigramsNum);
        dict.Clear();
        for (int i = 0; i < unigramsNum; ++i)
        {
            string[] data = lines[i + bigramsNum + 2].Split(' ');
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
            if (line[line.Length - 1] < 'a' || line[line.Length - 1] > 'z')
                line = line.Substring(0, line.Length - 1);
			phrase.Add(line);
		}

        Debug.Log("Phrases: " + phrase.Count + "/" + lines.Length);
		
		ChangePhrase();

        /*
		int[] id = new int[24];
		for (int i = 0; i < 24; ++i)
			id[i] = i;
		for (int i = 0; i < 24; ++i) 
		{
			int j = Random.Range(0, 23);
			int k = id[i]; id[i] = id[j]; id[j] = k;
		}
		int x;
		for(int i = 0; i < 3; ++i)
		{
			x = Random.Range((i*10), (i+1)*10 - 1);
			phraseList[x] = phrase.Count - 2;
			do
				x = Random.Range((i*10), (i+1)*10 - 1);
			while (phraseList[x] > 0);
			phraseList[x] = phrase.Count - 1;
		}
		int p = 0;
		for (int i = 0; i < 24; ++i)
		{
			while (phraseList[p] > 0) ++p;
			phraseList[p++] = id[i];
        }
        */
	}

	string Underline(char ch, int length)
	{
		string under = "";
		for(int i = 0; i < length; ++i)
			under += ch;
		return under;
	}

	public Candidate[] Recognize(Vector2[] rawStroke)
	{
		Vector2[] stroke = PathCalc.TemporalSampling(rawStroke);

		for (int i = 0; i < CandidatesNum; ++i)
			candsTmp[i] = new Candidate();

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
                > Parameter.endOffset * Parameter.keyWidth)
                continue;
            int w = history.Count - 1;
            w = System.Math.Min(w, words.Length - 1);
            string pre = "<s>";
            if (w >= 0)
                pre = words[w];
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
        return candsTmp;
	}

	public void SetCandidates(Candidate[] candList)
	{
		if (!useRadialMenu)
			btn[choose = 0].Select();
        
		for (int i = 0; i < candList.Length; ++i)
			cands[i] = candList[i];
        panel = -1;
        NextCandidatePanel();

        if (cands[0].confidence == 0)
			return;
		if (!useRadialMenu)
		{
			history.Add(new Candidate(cands[0]));
			string space = "";
			if (text.Length > 0)
				space = " ";
			inputText.text = text + space + cands[0].word;
			underText.text = under + space + Underline('_', cands[0].word.Length);
		}
	}

    public void NextCandidatePanel()
    {
        panel = (panel + 1) % 3;
        for (int i = 0; i < 4; ++i)
        {
            int id = panel * 4 + i;
            string text = cands[id].word;
            if (Parameter.debugOn)
                text += "\n" + cands[id].location.ToString(".000") + " " + cands[id].language.ToString(".000") 
                    + " " + cands[id].confidence.ToString(".000");
            if (useRadialMenu)
                radialText[i].text = text;
            else
                btn[i].GetComponentInChildren<Text>().text = text;
        }

    }

	public void NextCandidate()
	{
		choose = (choose + 1) % CandidatesNum;
		if (cands[choose].confidence == 0)
			choose = 0;
		history[history.Count - 1] = new Candidate(cands[choose]);
		string space = "";
		if (text.Length > 0)
			space = " ";
		inputText.text = text + space + cands[choose].word;
		underText.text = under + space + Underline('_', cands[choose].word.Length);
		btn[choose].Select();
	}

	public string Accept(ref int id)
	{
		if (id == -1)
			id = choose;
		if (text.Length > 0)
		{
			text += " ";
			under += " ";
		}
        id = panel * 4 + id;
        string word = cands[id].word;
        text += word;
		inputText.text = text;
		if (useRadialMenu)
			history.Add(new Candidate(word));
		else
		{
			under += Underline(' ', word.Length);
			underText.text = under;
		}
		
		for (int i = 0; i < CandidatesNum; ++i)
		{
			//btn[i].GetComponentInChildren<Text>().text = "";
			cands[i].word = "";
		}

		for (int i = 0; i < RadialNum; ++i)
			radialText[i].text = "";
		return word;
	}

	public void Delete()
	{
		if (!useRadialMenu)
			btn[choose = 0].Select();
		for (int i = 0; i < CandidatesNum; ++i)
		{
			//btn[i].GetComponentInChildren<Text>().text = "";
			cands[i].word = "";
		}
		if (history.Count == 0)
			return;
		history.RemoveAt(history.Count - 1);
		text = ""; 
		int length = 0;
		for (int i = 0; i < history.Count - 1; ++i)
		{
			text += history[i].word + " ";
			length += history[i].word.Length + 1;
		}
		under = Underline(' ', length);
		if (history.Count > 0)
		{
			text += history[history.Count - 1].word;
			under += Underline('_', history[history.Count - 1].word.Length);
		}
		inputText.text = text;
		if (!useRadialMenu)
			underText.text = under;
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
		SetRadialMenuDisplay(false);
		gesture.chooseCandidate = false;
		history.Clear();
		inputText.text = underText.text = under = text = "";
	}

	public char TapSingleKey(Vector2 point)
	{
		float minDis = float.MaxValue;
		char key = ' ';
		for (int i = 0; i < 26; ++i)
		{
			float dis = Vector2.Distance(point, keyPos[i]);
			if (dis < minDis)
			{
				minDis = dis;
				key = (char)(i + 97);
			}
		}

		if (text.Length > 0)
			text += " ";
		text += key.ToString();
		inputText.text = text;
		history.Add(new Candidate(key.ToString()));
		return key;
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
		if (id == -1)
			phraseText.text = phrase[Random.Range(0, phrase.Count)];
		else
			phraseText.text = phrase[phraseList[id]];
		words = phraseText.text.Split(' ');
		Clear();
		if (!useRadialMenu)
			btn[0].Select();
	}

	public void ChangeCandidatesChoose(bool change)
	{
		for (int i = 0; i < RadialNum; ++i)
			radialText[i] = radialMenu.transform.Find("Candidate" + i.ToString()).GetComponent<Text>();
		//for (int i = 0; i < CandidatesNum; ++i)
			//btn[i] = candidates.transform.Find("Candidate" + i.ToString()).GetComponent<Button>();
		useRadialMenu ^= change;
		if (Parameter.debugOn)
			if (useRadialMenu)
			{
				info.Log("[C]hoose", "Radial");
			}
			else
			{
				info.Log("[C]hoose", "Tap");
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
		radialMenu.texture = (Texture)Resources.Load("RadialMenu", typeof(Texture));
		Color color = radialMenu.color;
		color.a = display?0.9f:0;
		radialMenu.color = color;
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

	public void UpdateSizeMsg(string msg)
	{
		info.Log("Size", msg.Split(' ')[0]);
		info.Log("Scale", msg.Split(' ')[1]);
	}
}
