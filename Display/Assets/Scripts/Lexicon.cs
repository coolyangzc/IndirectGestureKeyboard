using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Lexicon : MonoBehaviour 
{
	public Image keyboard, candidates;

	private const int LexiconSize = 10000;
	private const int SampleSize = 64;
	private const int CandidatesNum = 4;
	private const float KeyWidth = 0.1f;
	private const float Delta = KeyWidth;
	private Vector2[] keyPos = new Vector2[26];

	public static Vector2 StartPoint = new Vector2(0f, 0f);

	public class Entry
	{
		public string word;
		public int frequency;
		public Vector2[][] locationSample = new Vector2[2][];
		public Vector2[][] shapeSample = new Vector2[2][];
		public Entry(string word, int frequency, Lexicon lexicon)
		{
			this.word = word;
			this.frequency = frequency;
			List<Vector2> pts = new List<Vector2>();
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
			//shapeSample = lexicon.Normalize(locationSample);
		}
	}

	public class Candidate
	{
		public string word;
		public float confidence;
	}

	private List<Entry> dict = new List<Entry>();

	// Use this for initialization
	void Start () 
	{
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
			if (entry.locationSample.Length > 1)
				dict.Add(entry);
		}
		//for (int i = 0; i < SampleSize; ++i)
			//Debug.Log(dict[0].locationSample[i].x.ToString());
	}

	float Match(Vector2[] A, Vector2[] B)
	{
		float dis = 0;
		for (int i = 0; i < SampleSize; ++i)
		{
			dis += Vector2.Distance(A[i], B[i]);
			if (dis > 2.5f * (i+1) * KeyWidth)
				return 0;
		}
		dis /= SampleSize;
		return Mathf.Exp(-0.5f * (dis / Delta) * (dis / Delta));
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
		Candidate[] candidates = new Candidate[4];
		for (int i = 0; i < CandidatesNum; ++i)
			candidates[i] = new Candidate();

		foreach (Entry entry in dict)
		{
			Candidate newCandidate = new Candidate();
			newCandidate.word = entry.word;
			newCandidate.confidence = Match(stroke, entry.locationSample[(int)Server.mode]);

			if (newCandidate.confidence == 0)
				continue;
			//newCandidate.confidence *= Match(nStroke, entry.shapeSample);
			//if (newCandidate.confidence == 0)
				//continue;
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

	public void SetCandidates(Candidate[] cands)
	{
		for (int i = 0; i < cands.Length; ++i)
		{
			Button btn = candidates.transform.FindChild("Candidate" + i.ToString()).GetComponent<Button>();
			btn.GetComponentInChildren<Text>().text = cands[i].word;
		}
	}
}
