using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
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
	private const float DTWConst = 0.1f;
	private const float AnyStartThr = 2.5f;
    private const float eps = 1e-6f;
	private float KeyWidth = 0f;
	public static Vector2 StartPoint;
	public static Vector2 StartPointRelative = new Vector2(0f, 0f);

	//Parameters
	private float endOffset = 3.0f;
	private float radiusMul = 0.20f, radius = 0;
	private bool debugOn = false;

    public static bool isCut = true;
	public static bool useRadialMenu = true;
	public static Mode mode = Mode.Basic;
	public static UserStudy userStudy = UserStudy.Basic;
	public static Formula locationFormula = Formula.DTW, shapeFormula = Formula.Null;

	//Internal Variables
	private int choose = 0;
	private float dis = 0;
	private int highLight = 0;
	private string[] words;
	private Button[] btn = new Button[CandidatesNum];
	private Text[] radialText = new Text[RadialNum];
    private int panel;
	private Candidate[] cands = new Candidate[CandidatesNum], candsTmp = new Candidate[CandidatesNum];
	private Vector2[] keyPos = new Vector2[26];

	private int[] DTWL = new int[SampleSize + 1], DTWR = new int[SampleSize + 1];
	private float[][] dtw = new float[SampleSize+1][];

	private List<string> phrase = new List<string>();
	private int[] phraseList = new int[30];
	private List<Candidate> history = new List<Candidate>();

	//Definitions
	public enum Mode
	{
		Basic = 0,
		FixStart = 1,
		AnyStart = 2,
		End = 3,
	};
	
	public enum Formula
	{
		Basic = 0,
		MinusR = 1,
		DTW = 2,
		Null = 3,
		End = 4,
	}

	public enum UserStudy
	{
		Basic = 0,
		Train = 1,
		Study1 = 2,
		Study2 = 3,
		End = 4,
	}

	public class Entry
	{
		public string word;
		public int frequency;
		public float languageModelPossibilty;
		public List<Vector2> pts = new List<Vector2>();
		public Vector2[][] locationSample = new Vector2[3][];
		public Vector2[][] shapeSample = new Vector2[3][];
		public Entry(string word, int frequency, Lexicon lexicon)
		{
			this.word = word;
			this.frequency = frequency;
			int key = -1;
			for (int i = 0; i < word.Length; ++i)
			{
				key = word[i] - 97;
				if (key >= 0 && key <= 25 || key+32 >=0 && key+32 <=25) 
				{
					if (key < 0)
						key += 32;
					pts.Add(lexicon.keyPos[key]);
				}
			}
			locationSample[0] = lexicon.TemporalSampling(pts.ToArray());
            bool ins = false;
            if (Vector2.Distance(pts[0], StartPoint) > eps)
                ins = true;
            if (ins)
                pts.Insert(0, StartPoint);
			locationSample[1] = lexicon.TemporalSampling(pts.ToArray());
            if (ins)
                pts.RemoveAt(0);
			for(int i = 0; i < (int)Mode.End; ++i)
				shapeSample[i] = lexicon.Normalize(locationSample[i]);
		}
	}

	public class Candidate
	{
		public string word;
		public float location, shape, language, confidence, DTWDistance;
		public Candidate(string word_ = "")
		{
			word = word_;
			confidence = 0;
			DTWDistance = float.MaxValue;
		}
		public Candidate(Candidate x)
		{
			word = x.word;
			location = x.location;
			shape = x.shape;
			confidence = x.confidence;
			DTWDistance = x.DTWDistance;
		}
	}

	private HashSet<string> allWords = new HashSet<string>();
	private List<Entry> dict = new List<Entry>();

	// Use this for initialization
	void Start () 
	{
		info.Log("Mode", mode.ToString());
		//info.Log("[L]ocation", locationFormula.ToString());
		//info.Log("[S]hape", shapeFormula.ToString());
		history.Clear();
		ChangeCandidatesChoose(false);
		SetRadialMenuDisplay(false);
		CalcKeyLayout();
		CalcLexicon();
		ChangeRadius(0);
		ChangeEndOffset(0);
		InitDTW();
		InitPhrases();

		//Alternative Start Option
		ChangeMode();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void CalcKeyLayout()
	{
		float width = keyboard.rectTransform.rect.width;
		KeyWidth = width * 0.1f;
		for (int i = 0; i < 26; ++i)
		{
			RectTransform key = keyboard.rectTransform.Find(((char)(i + 65)).ToString()).GetComponent<RectTransform>();
			keyPos[i] = new Vector2(key.localPosition.x, key.localPosition.y);
			Debug.Log(((char)(i + 65)).ToString() + ":" + keyPos[i].x.ToString() + "," + keyPos[i].y.ToString());
		}
		StartPoint = keyPos[6]; //keyG
	}

	public void CalcLexicon()
	{
		TextAsset textAsset = Resources.Load("ANC-written-noduplicate+pangram") as TextAsset;
		string[] lines = textAsset.text.Split('\n');
		int size = lines.Length;
		if (LexiconSize > 0)
			size = LexiconSize;
		dict.Clear();
		allWords.Clear();
		for (int i = 0; i < size; ++i)
		{
			string line = lines[i];
			Entry entry = new Entry(line.Split(' ')[0], int.Parse(line.Split(' ')[1]), this);
			if (entry.locationSample[(int)Mode.FixStart].Length > 1)
			{
				dict.Add(entry);
				allWords.Add(entry.word);
			}
		}
        foreach (Entry entry in dict)
            entry.languageModelPossibilty = entry.frequency;
    }

	void InitDTW()
	{
		int w = (int)(SampleSize * DTWConst);
		for (int i = 0; i <= SampleSize; ++i)
		{
			dtw[i] = new float[SampleSize + 1];
			DTWL[i] = Mathf.Max(i - w, 0);
			DTWR[i] = Mathf.Min(i + w, SampleSize);
			for (int j = 0; j <= SampleSize; ++j)
				dtw[i][j] = float.MaxValue;
		}
		dtw[0][0] = 0;
	}

	void InitPhrases()
	{
		TextAsset textAsset = Resources.Load("3000words") as TextAsset;
		string[] lines = textAsset.text.Split('\n');
		for (int i = 0; i < lines.Length; ++i)
		{
			if (lines[i].Length <= 1)
				continue;
            string line = lines[i];
            if (line[line.Length - 1] < 'a' || line[line.Length - 1] > 'z')
                line = line.Substring(0, line.Length - 1);
                
			string[] words = line.Split(' ');
			bool available = true;
			foreach(string word in words)
				if (!allWords.Contains(word))
				{
					available = false;
					break;
				}
			if (available)
				phrase.Add(line);
		}
        /*
        string tmp;
        for (int i = 0; i < phrase.Count; ++i)
            for (int j = i + 1; j < phrase.Count; ++j)
            {
                int iCnt = phrase[i].Split(' ').Length, jCnt = phrase[j].Split(' ').Length;
                if (iCnt > jCnt || iCnt == jCnt && phrase[i].Length < phrase[j].Length)
                {
                    tmp = phrase[i];
                    phrase[i] = phrase[j];
                    phrase[j] = tmp;
                }
            }
        StreamWriter sw = new StreamWriter("available_phrases.txt");
        for (int i = 0; i < phrase.Count; ++i)
            sw.WriteLine(phrase[i]);
        sw.Close();
        */

        Debug.Log("Phrases: " + phrase.Count + "/" + lines.Length);
		textAsset = Resources.Load("pangrams") as TextAsset;
        
		lines = textAsset.text.Split('\n');
		for (int i = 0; i < lines.Length; ++i)
			phrase.Add(lines[i]);
		ChangePhrase();

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
	}

	float sqr(float x)
	{
		return x * x;
	}

	string underline(char ch, int length)
	{
		string under = "";
		for(int i = 0; i < length; ++i)
			under += ch;
		return under;
	}

	float Match(Vector2[] A, Vector2[] B, Formula formula, bool isShape = false)
	{
		if (A.Length != B.Length || formula == Formula.Null)
			return 0;
		/*if (Vector2.Distance(A[0], B[0]) > KeyWidth)
			return 0;*/
		if (Vector2.Distance(A[A.Length - 1], B[B.Length - 1]) > endOffset * KeyWidth)
			return 0;
		dis = 0;
		switch(formula)
		{
			case (Formula.Basic):
				for (int i = 0; i < SampleSize; ++i)
				{
					dis += Vector2.Distance(A[i], B[i]);
				}
				break;
			case (Formula.MinusR):
				for (int i = 0; i < SampleSize; ++i)
				{
					dis += Mathf.Max(0, Vector2.Distance(A[i], B[i]) - radius);
				}
				break;
			case (Formula.DTW):
				for (int i = 0; i < SampleSize; ++i)
				{
					//float gap = float.MaxValue;
					for (int j = DTWL[i]; j < DTWR[i]; ++j)
					{
						dtw[i+1][j+1] = Vector2.Distance(A[i], B[j]) + Mathf.Min(dtw[i][j], Mathf.Min(dtw[i][j+1], dtw[i+1][j]));
						//gap = Mathf.Min(gap, dtw[i+1][j+1]);
					}
					//if (gap > candsTmp[CandidatesNum - 1].DTWDistance)
						//return 0;
				}
				dis = dtw[SampleSize][SampleSize];
				break;
		}
		dis /= SampleSize;
        if (!isShape)
	        return Mathf.Exp(-0.5f * dis * dis / radius / radius);
        else
            return Mathf.Exp(-0.5f * dis * dis / (radiusMul*0.1f) / (radiusMul*0.1f));

    }
	public Vector2[] TemporalSampling(Vector2[] stroke)
	{
		float length = 0;
		int count = stroke.Length;
		Vector2[] vector = new Vector2[SampleSize];
		if (count == 1)
		{
			for (int i = 0; i < SampleSize; ++i)
				vector[i] = stroke[0];
			return vector;
		}

		for (int i = 0; i < count - 1; ++i)
			length += Vector2.Distance(stroke[i], stroke[i + 1]);
		float increment = length / (SampleSize - 1);

		Vector2 last = stroke[0];
		float distSoFar = 0;
		int id = 1, vecID = 1;
		vector[0] = stroke[0];
		while (id < count)
		{
			float dist = Vector2.Distance(last, stroke[id]);
			if (distSoFar + dist >= increment)
			{
				float ratio = (increment - distSoFar) / dist;
				last = last + ratio * (stroke[id] - last);
				vector[vecID++] = last;
				distSoFar = 0;
			}
			else
			{
				distSoFar += dist;
				last = stroke[id++];
			}
		}
		for (int i = vecID; i < SampleSize; ++i)
			vector[i] = stroke[count - 1];
		return vector;
	}

	public Vector2[] Normalize(Vector2[] pts)
	{
		if (pts == null)
			return null;
		float minX = 1f, minY = 1f;
		float maxX = -1f, maxY = -1f;

		Vector2 center = new Vector2(0, 0);
		int size = pts.Length;
		for (int i = 0; i < size; ++i)
		{
			center += pts[i];
			minX = Mathf.Min(minX, pts[i].x);
			maxX = Mathf.Max(maxX, pts[i].x);
			minY = Mathf.Min(minY, pts[i].y);
			maxY = Mathf.Max(maxY, pts[i].y);
		}
		center = center / size;
		float ratio = 1.0f / Mathf.Max(maxX - minX, maxY - minY);
		Vector2[] nPts = new Vector2[size];
		for(int i = 0; i < size; ++i)
			nPts[i] = (pts[i] - center) * ratio;
		return nPts;
	}

	public Candidate[] Recognize(Vector2[] rawStroke)
	{

		Vector2[] stroke = TemporalSampling(rawStroke);
		Vector2[] nStroke = Normalize(stroke);

		for (int i = 0; i < CandidatesNum; ++i)
			candsTmp[i] = new Candidate();

		foreach (Entry entry in dict)
		{
            if (rawStroke.Length == 1 && entry.word.Length != 1)
                continue;
			Candidate newCandidate = new Candidate();
			newCandidate.word = entry.word;
			if (mode == Mode.AnyStart)
			{
				if (Vector2.Distance(entry.locationSample[0][0], stroke[0]) > AnyStartThr * KeyWidth)
					continue;
				entry.pts.Insert(0, stroke[0]);
				entry.locationSample[(int)mode] = TemporalSampling(entry.pts.ToArray());
				entry.shapeSample[(int)mode] = Normalize(entry.locationSample[(int)mode]);
				entry.pts.RemoveAt(0);
			}
			newCandidate.confidence = newCandidate.location = Match(stroke, entry.locationSample[(int)mode], locationFormula);
            if (newCandidate.location == 0)
				continue;
			if (locationFormula == Formula.DTW)
				newCandidate.DTWDistance = dis * SampleSize;
			if (shapeFormula != Formula.Null)
			{
				newCandidate.shape = Match(nStroke, entry.shapeSample[(int)mode], shapeFormula, true);
				if (newCandidate.shape == 0)
					continue;
				newCandidate.confidence *= newCandidate.shape;
			}
			newCandidate.language = entry.languageModelPossibilty;
			newCandidate.confidence *= newCandidate.language;
			if (newCandidate.confidence < candsTmp[CandidatesNum - 1].confidence)
				continue;
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
			underText.text = under + space + underline('_', cands[0].word.Length);
		}
	}

    public void NextCandidatePanel()
    {
        panel = (panel + 1) % 3;
        for (int i = 0; i < 4; ++i)
        {
            int id = panel * 4 + i;
            string text = cands[id].word;
            if (debugOn)
                text += "\n" + cands[id].location.ToString(".00") + " " + cands[id].shape.ToString(".00") + " " + cands[id].language.ToString(".00");
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
		underText.text = under + space + underline('_', cands[choose].word.Length);
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
			under += underline(' ', word.Length);
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
		under = underline(' ', length);
		if (history.Count > 0)
		{
			text += history[history.Count - 1].word;
			under += underline('_', history[history.Count - 1].word.Length);
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
		this.debugOn = debugOn;
		if (cands[0] == null || cands[0].word.Length <= 0)
			return;
		SetCandidates(cands);
	}

	public void ChangeMode()
	{
		mode = mode + 1;
		if (mode >= Mode.End)
			mode = 0;
		info.Log("Mode", mode.ToString());
	}

	public void ChangeLocationFormula()
	{
		locationFormula = locationFormula + 1;
		if (locationFormula >= Formula.End)
			locationFormula = 0;
		if (debugOn)
			info.Log("[L]ocation", locationFormula.ToString());
	}

	public void ChangeShapeFormula()
	{
		shapeFormula = shapeFormula + 1;
		if (shapeFormula >= Formula.End)
			shapeFormula = 0;
		if (debugOn)
			info.Log("[S]hape", shapeFormula.ToString());
	}

	public void ChangeRadius(float delta)
	{
		if (radiusMul + delta <= eps)
			return;
		radiusMul += delta;
		radius = KeyWidth * radiusMul;
		if (debugOn)
			info.Log("[R]adius", radiusMul.ToString("0.00") + "key");
	}

	public void ChangeEndOffset(float delta)
	{
		if (endOffset + delta <= 0)
			return;
		endOffset += delta;
		if (debugOn)
			info.Log("[E]ndOffset", endOffset.ToString("0.0"));
	}

	public void ChangePhrase(int id = -1)
	{
		if (id >= 30)
			id -= 30;
		if (id == -1)
			phraseText.text = phrase[Random.Range(0, phrase.Count - 3)];
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
		if (debugOn)
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
