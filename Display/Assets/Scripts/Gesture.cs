using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Gesture : MonoBehaviour {

	public Server server;
	public Image keyboard;
	public RawImage cursor, radialMenu;
	public Text text;
	public Lexicon lexicon;
	
	public bool chooseCandidate = false;
	private bool ratioChanged = false;
    private int menuGestureCount = 0;
	private float keyboardWidth, keyboardHeight;
	private float length, lastBeginTime, lastEndTime;
	private Vector2 StartPointRelative;
	private Vector2 beginPoint, prePoint, localPoint;
	private List<Vector2> stroke = new List<Vector2>();

	private const float RadiusMenuR = 0.09f;
    private const float eps = 1e-6f;

	// Use this for initialization
	void Start() 
	{
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		StartPointRelative = Lexicon.StartPointRelative;
        lastEndTime = -1;
	}
	
	// Update is called once per frame
	void Update() 
	{
		
	}

	public void ChangeRatio()
	{
		Vector3 pos = keyboard.GetComponent<RectTransform>().localPosition;
		Vector2 size = keyboard.GetComponent<RectTransform>().sizeDelta;
		if (ratioChanged)
		{
			size.x = 1000;
			size.y = 300;
			pos.y = 0.5f;
		}
		else
		{
			size.x = 700;
			size.y = 630;
			pos.y = -0.1f;
		}
		keyboard.GetComponent<RectTransform>().sizeDelta = size;
		keyboard.GetComponent<RectTransform>().localPosition = pos;
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		ratioChanged ^= true;
	}

	public void Begin(float x, float y)
	{
        lastBeginTime = Time.time;
        if (chooseCandidate)
			cursor.GetComponent<TrailRendererHelper>().Reset(0.3f);
		else
			cursor.GetComponent<TrailRendererHelper>().Reset(0.6f);
		beginPoint = new Vector2(x, y);
		prePoint = new Vector2(x, y);
		length = 0;
		if (Lexicon.useRadialMenu && chooseCandidate)
		{
			cursor.transform.localPosition = new Vector3(StartPointRelative.x * keyboardWidth, 
			                                             StartPointRelative.y * keyboardHeight, -0.2f);
			return;
		}
		switch (Lexicon.mode)
		{
			case (Lexicon.Mode.Basic):
			case (Lexicon.Mode.AnyStart):
				cursor.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.2f);
				stroke.Clear();
				stroke.Add(new Vector2(x, y));
				break;
			case (Lexicon.Mode.FixStart):
				cursor.transform.localPosition = new Vector3(StartPointRelative.x * keyboardWidth, 
				                                             StartPointRelative.y * keyboardHeight, -0.2f);
				stroke.Clear();
				stroke.Add(Lexicon.StartPointRelative);
				break;
			default:
				break;
		}

	}

	public void Move(float x, float y)
	{
		localPoint = new Vector2(x, y);
		length += Vector2.Distance(prePoint, localPoint);
		prePoint = localPoint;
		if (Lexicon.mode == Lexicon.Mode.FixStart || (Lexicon.useRadialMenu && chooseCandidate))
		{
			x = x - beginPoint.x + StartPointRelative.x;
			y = y - beginPoint.y + StartPointRelative.y;
		}
		cursor.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.2f);
        if (Vector2.Distance(new Vector2(x, y), stroke[stroke.Count - 1]) > eps)
		    stroke.Add(new Vector2(x, y));

		if (Lexicon.useRadialMenu && chooseCandidate)
		{
			if (Vector2.Distance(new Vector2(x, y), StartPointRelative) < RadiusMenuR)
			{
				radialMenu.texture = (Texture)Resources.Load("RadialMenu", typeof(Texture));
				return;
			}
			float rx = x - StartPointRelative.x;
			float ry = y - StartPointRelative.y;
			if (Mathf.Abs(rx) > Mathf.Abs(ry))
			{
				if (rx > 0)
					radialMenu.texture = (Texture)Resources.Load("RadialMenu_RIGHT", typeof(Texture));
				else
					radialMenu.texture = (Texture)Resources.Load("RadialMenu_LEFT", typeof(Texture));
			}
			else
			{
				if (ry > 0)
					radialMenu.texture = (Texture)Resources.Load("RadialMenu_UP", typeof(Texture));
				else
					radialMenu.texture = (Texture)Resources.Load("RadialMenu_DOWN", typeof(Texture));
			}
		}
		else
		{
            /*
			if (length > 0.1f)
			{
				lexicon.under = lexicon.under.Replace("_", " ");
				lexicon.underText.text = lexicon.under;
				if (chooseCandidate)
				{
					chooseCandidate = false;
					lexicon.Accept(ref -1);
				}
			}
            */
		}
	}

	public void End(float x, float y)
	{
		if (Lexicon.mode == Lexicon.Mode.FixStart || (Lexicon.useRadialMenu && chooseCandidate))
		{
			x = x - beginPoint.x + StartPointRelative.x;
			y = y - beginPoint.y + StartPointRelative.y;
		}
		cursor.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.2f);
        if (lastEndTime == -1 || Time.time - lastEndTime > 0.2)
            lastEndTime = Time.time;
        else
        {
            if (chooseCandidate)
            {
                if (Lexicon.userStudy == Lexicon.UserStudy.Study2)
                    server.Send("Cancel", "");
                chooseCandidate = false;
                lexicon.SetRadialMenuDisplay(false);
                if (menuGestureCount == 0)
                {
                    lexicon.Delete();
                    server.Send("Delete", "DoubleClick");
                }
            }
            lastEndTime = -1;
            return;
        }
        if (Vector2.Distance(new Vector2(x, y), stroke[stroke.Count - 1]) > eps)
            stroke.Add(new Vector2(x, y));
		if (Lexicon.userStudy == Lexicon.UserStudy.Study1 || Lexicon.userStudy == Lexicon.UserStudy.Train)
		{
			lexicon.HighLight(+1);
			return;
		}
		if (Lexicon.mode == Lexicon.Mode.FixStart)
		{
			cursor.GetComponent<TrailRendererHelper>().Reset();
			cursor.transform.localPosition = new Vector3(StartPointRelative.x * keyboardWidth, StartPointRelative.y * keyboardHeight, -0.2f);
		}
		if (chooseCandidate)
		{
            menuGestureCount++;
            if (length < 0.1f)
			{
				if (!Lexicon.useRadialMenu)
				{
					lexicon.NextCandidate();
					return;
				}
			}
			if (Lexicon.useRadialMenu)
			{
                if (Vector2.Distance(new Vector2(x, y), StartPointRelative) > RadiusMenuR)
                {
                    x -= StartPointRelative.x;
                    y -= StartPointRelative.y;
                    int choose = -1;
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        if (x > 0)
                            choose = 2;
                        else
                            choose = 1;
                    }
                    else
                    {
                        if (y > 0)
                            choose = 0;
                        else
                            choose = 3;
                    }
                    string word = lexicon.Accept(ref choose);
                    if (Lexicon.userStudy == Lexicon.UserStudy.Study2)
                        server.Send("Accept", choose.ToString() + " " + word);
                    chooseCandidate = false;
                    lexicon.SetRadialMenuDisplay(false);
                }
                else
                {
                    server.Send("NextCandidatePanel", "");
                    lexicon.NextCandidatePanel();
                }
				return;
			}
		}

        /*
        if (Lexicon.useRadialMenu && length <= 0.05f)
		{
			char key = lexicon.TapSingleKey(new Vector2(x * keyboardWidth, y * keyboardHeight));
			server.Send("SingleKey", key.ToString());
			return;
		}
        */

		if (stroke[stroke.Count - 1].x - stroke[0].x <= -0.15f && length <= 1.0f && Time.time - lastBeginTime < 0.2)
		{
			if (Lexicon.userStudy == Lexicon.UserStudy.Study2)
				server.Send("Delete", "LeftSwipe");
			lexicon.Delete();
			chooseCandidate = false;
			return;
		}
		if (x >= 0.55f && length <= 2.0f && Lexicon.userStudy == Lexicon.UserStudy.Basic)
		{
			lexicon.ChangePhrase();
			return;
		}
        if (Lexicon.isCut)
        {
            int l = 0, r = stroke.Count - 1;
            while (outKeyboard(stroke[l]) && l < r) ++l;
            while (outKeyboard(stroke[r]) && l < r) --r;
            if (l < r)
            {
                stroke.RemoveRange(r + 1, stroke.Count - r - 1);
                stroke.RemoveRange(0, l);
            }
        }
		for (int i = 0; i < stroke.Count; ++i)
            stroke[i] = new Vector2(stroke[i].x * keyboardWidth, stroke[i].y * keyboardHeight);
		Lexicon.Candidate[] candidates = lexicon.Recognize(stroke.ToArray());
		if (candidates[0].confidence > 0)
		{
			chooseCandidate = true;
            menuGestureCount = 0;
            if (Lexicon.useRadialMenu)
			{
				cursor.transform.localPosition = new Vector3(StartPointRelative.x * keyboardWidth, StartPointRelative.y * keyboardHeight, -0.2f);
				cursor.GetComponent<TrailRendererHelper>().Reset();
				lexicon.SetRadialMenuDisplay(true);
			}
		}
		lexicon.SetCandidates(candidates);
		string msg = "";
		for (int i = 0; i < candidates.Length; ++i)
			if (i < candidates.Length - 1)
				msg += candidates[i].word + ",";
			else
				msg += candidates[i].word;
		server.Send("Candidates", msg);
	}

    private bool outKeyboard(Vector2 v)
    {
        return v.x > 0.5 || v.x < -0.5 || v.y > 0.5 || v.y < -0.5;
    }
}
