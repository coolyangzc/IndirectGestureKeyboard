using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Lexicon : MonoBehaviour 
{
	public Image keyboard, candidates;
	public Info info;
	public Text inputText;

	private const int LexiconSize = 10000;
	private const int SampleSize = 64;
	private const int CandidatesNum = 5;
	private const float KeyWidth = 0.1f;
	
	private float radiusMul = 0.5f, radius = KeyWidth * 0.5f;
	private bool debugOn = false;
	private int choose = 0;
	private Button[] btn = new Button[CandidatesNum];
	private Candidate[] cands = new Candidate[CandidatesNum];
	private Vector2[] keyPos = new Vector2[26];

	public static Vector2 StartPoint = new Vector2(0.0f, 0.125f);
	
	private List<Candidate> history = new List<Candidate>();


	private string text = "";

	public enum Mode
	{
		Basic = 0,
		FixStart = 1,
		AnyStart = 2,
		Null = 3,
	};

	private float AnyStartThr = 2.0f;

	public enum Formula
	{
		Basic = 0,
		MinusR = 1,
		Shape = 2,
		Null = 3,
	}
	
	public static Mode mode;
	public static Formula formula;

	public class Entry
	{
		public string word;
		public int frequency;
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
			pts.Insert(0, StartPoint);
			locationSample[1] = lexicon.TemporalSampling(pts.ToArray());
			pts.RemoveAt(0);
			for(int i = 0; i < (int)Mode.Null; ++i)
				shapeSample[i] = lexicon.Normalize(locationSample[i]);
		}
	}

	public class Candidate
	{
		public string word;
		public float location, shape, confidence;
		public Candidate()
		{
			word = "";
			confidence = 0;
		}
		public Candidate(Candidate x)
		{
			word = x.word;
			location = x.location;
			shape = x.shape;
			confidence = x.confidence;
		}
	}

	private List<Entry> dict = new List<Entry>();

	// Use this for initialization
	void Start () 
	{
		info.Log("Mode", mode.ToString());
		info.Log("Radius", radiusMul.ToString());
		info.Log("Formula", formula.ToString());
		history.Clear();
		for (int i = 0; i < CandidatesNum; ++i)
			btn[i] = candidates.transform.FindChild("Candidate" + i.ToString()).GetComponent<Button>();
		CalcKeyLayout();
		CalcLexicon();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void CalcKeyLayout()
	{
		float width = keyboard.rectTransform.rect.width;
		float height = keyboard.rectTransform.rect.height;
		for (int i = 0; i < 26; ++i)
		{
			RectTransform key = keyboard.rectTransform.FindChild(((char)(i + 65)).ToString()).GetComponent<RectTransform>();
			keyPos[i] = new Vector2(key.localPosition.x / width, key.localPosition.y / height);
			//Debug.Log(((char)(i + 65)).ToString() + ":" + keyPos[i].x.ToString() + "," + keyPos[i].y.ToString());
		}
	}

	void CalcLexicon()
	{
		TextAsset textAsset = Resources.Load("en_us_wordlist") as TextAsset;
		string[] lines = textAsset.text.Split('\n');
		int size = lines.Length;
		if (LexiconSize > 0)
			size = LexiconSize;
		for (int i = 0; i < size; ++i)
		{
			string line = lines[i];
			Entry entry = new Entry(line.Split(' ')[0], int.Parse(line.Split(' ')[1]), this);
			if (entry.locationSample[(int)Mode.FixStart].Length > 1)
				dict.Add(entry);
		}
		//for (int i = 0; i < SampleSize; ++i)
			//Debug.Log(dict[0].locationSample[i].x.ToString());
	}

	float Match(Vector2[] A, Vector2[] B, Formula formula)
	{
		if (A.Length != B.Length)
			return 0;
		float dis = 0;
		switch(formula)
		{
			case (Formula.Basic):
				for (int i = 0; i < SampleSize; ++i)
				{
					dis += Vector2.Distance(A[i], B[i]);

				}
				break;
			case (Formula.MinusR):
			case (Formula.Shape):
				for (int i = 0; i < SampleSize; ++i)
				{
					dis += Mathf.Max(0, Vector2.Distance(A[i], B[i]) - radius);
		
				}
				break;
		}
		
		dis /= SampleSize;
		return Mathf.Exp(-0.5f * (dis / radius) * (dis / radius));
	}

	public Vector2[] TemporalSampling(Vector2[] stroke)
	{
		float length = 0;
		int count = stroke.Length;
		if (count == 1)
			return stroke;
		Vector2[] vector = new Vector2[SampleSize];
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
		Candidate[] candidates = new Candidate[CandidatesNum];
		for (int i = 0; i < CandidatesNum; ++i)
			candidates[i] = new Candidate();

		foreach (Entry entry in dict)
		{
			Candidate newCandidate = new Candidate();
			newCandidate.word = entry.word;
			if (mode == Mode.AnyStart)
			{
				if (entry.word == "gesture")
					Debug.Log(Vector2.Distance(stroke[0], entry.pts[0]).ToString());
				if (Vector2.Distance(stroke[0], entry.pts[0]) > AnyStartThr * KeyWidth)
					continue;
				entry.pts.Insert(0, stroke[0]);
				entry.locationSample[(int)mode] = TemporalSampling(entry.pts.ToArray());
				entry.shapeSample[(int)mode] = Normalize(entry.locationSample[(int)mode]);
				entry.pts.RemoveAt(0);
			}
			newCandidate.confidence = newCandidate.location = Match(stroke, entry.locationSample[(int)mode], formula);
			if (newCandidate.location == 0)
				continue;
			if (formula == Formula.Shape)
			{
				newCandidate.shape = Match(nStroke, entry.shapeSample[(int)mode], Formula.Basic);
				if (newCandidate.shape == 0)
					continue;
				newCandidate.confidence *= newCandidate.shape;
			}
			if (newCandidate.confidence < candidates[CandidatesNum - 1].confidence)
				continue;
			for (int i = 0; i < CandidatesNum; ++i)
				if (newCandidate.confidence > candidates[i].confidence)
				{
					for (int j = CandidatesNum - 1; j > i; j--)
						candidates[j] = candidates[j-1];
					candidates[i] = newCandidate;
					break;
				}
		}
		return candidates;
	}

	public void SetCandidates(Candidate[] candList)
	{

		btn[choose = 0].Select();
		for (int i = 0; i < candList.Length; ++i)
		{
			cands[i] = candList[i];
			if (debugOn)
				btn[i].GetComponentInChildren<Text>().text = cands[i].word + "\n" + cands[i].location + "\n" + cands[i].shape;
			else
				btn[i].GetComponentInChildren<Text>().text = cands[i].word;
		}
		if (cands[0].confidence == 0)
			return;
		history.Add(new Candidate(cands[0]));
		
		string space = "";
		if (text.Length > 0)
			space = " ";
		inputText.text = text + space + "<i>" + cands[0].word + "</i>";
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
		inputText.text = text + space + "<i>" + cands[choose].word + "</i>";
		btn[choose].Select();
	}

	public void Accept(int id)
	{
		if (id == -1)
			id = choose;
		if (text.Length > 0)
			text += " ";
		text += cands[id].word;
		inputText.text = text;

		for (int i = 0; i < CandidatesNum; ++i)
		{
			btn[i].GetComponentInChildren<Text>().text = "";
			cands[i].word = "";
		}
	}

	public void Delete()
	{
		btn[choose = 0].Select(); //choose = 0
		for (int i = 0; i < CandidatesNum; ++i)
		{
			btn[i].GetComponentInChildren<Text>().text = "";
			cands[i].word = "";
		}
		if (history.Count == 0)
			return;
		history.RemoveAt(history.Count - 1);
		text = "";
		for (int i = 0; i < history.Count - 1; ++i)
			text += history[i].word + " ";
		if (history.Count > 0)
			text += "<i>" + history[history.Count - 1].word + "</i>";
		inputText.text = text;
	}

	public void SetDebugDisplay(bool debugOn)
	{
		this.debugOn = debugOn;
		if (cands[0] == null || cands[0].word.Length <= 0)
			return;
		if (debugOn)
			for (int i = 0; i < CandidatesNum; ++i)
				btn[i].GetComponentInChildren<Text>().text = cands[i].word + "\n" + cands[i].confidence;
		else
			for (int i = 0; i < CandidatesNum; ++i)
				btn[i].GetComponentInChildren<Text>().text = cands[i].word;
	}

	public void ChangeMode()
	{
		mode = mode + 1;
		if (mode >= Mode.Null)
			mode = 0;
		info.Log("Mode", mode.ToString());
	}

	public void ChangeFormula()
	{
		formula = formula + 1;
		if (formula >= Formula.Null)
			formula = 0;
		info.Log("Formula", formula.ToString());
	}

	public void ChangeRadius(float delta)
	{
		if (radiusMul + delta <= 0)
			return;
		radiusMul += delta;
		radius = KeyWidth * radiusMul;
		info.Log("Radius", radiusMul.ToString());
	}
}
