using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Gesture : MonoBehaviour {

	public Server server;
	public Image keyboard;
	public RawImage cursor, radialMenu, spaceArea, deleteArea;
	public Text text;
	public Lexicon lexicon;
    public TextManager textManager;
    public PCControl pcControl;
	
	public bool chooseCandidate = false;
    private bool listExpanded = false;
    private int menuGestureCount = 0;
	private float length, lastBeginTime, lastOutListTime;
	private Vector2 StartPointRelative;
	private Vector2 beginPoint, prePoint, localPoint;
	private List<Vector2> stroke = new List<Vector2>();

	private const float RadiusMenuR = 0.09f;
    private const float eps = 1e-6f;
    private readonly int[] listOrder = new int[4] { 3, 1, 2, 4 };


	// Use this for initialization
	void Start() 
	{
		StartPointRelative = Lexicon.StartPointRelative;
	}
	
	// Update is called once per frame
	void Update() 
	{
		
	}

	public void Begin(float x, float y)
	{
        lastBeginTime = lastOutListTime = Time.time;
        if (chooseCandidate && Lexicon.useRadialMenu)
			cursor.GetComponent<TrailRendererHelper>().Reset(0.3f);
		else
			cursor.GetComponent<TrailRendererHelper>().Reset(0.6f); //0.6f
		beginPoint = new Vector2(x, y);
		prePoint = new Vector2(x, y);
		length = 0;
		if (Lexicon.useRadialMenu && chooseCandidate)
		{
			cursor.transform.localPosition = new Vector3(StartPointRelative.x * Parameter.keyboardWidth, 
			                                             StartPointRelative.y * Parameter.keyboardHeight, -0.2f);
			return;
		}
        switch (Parameter.mode)
		{
			case (Parameter.Mode.Basic):
			case (Parameter.Mode.AnyStart):
				cursor.transform.localPosition = new Vector3(x * Parameter.keyboardWidth, y * Parameter.keyboardHeight, -0.2f);
				stroke.Clear();
				stroke.Add(new Vector2(x, y));
                SetAreaDisplay(x, y);
                break;
			case (Parameter.Mode.FixStart):
				cursor.transform.localPosition = new Vector3(StartPointRelative.x * Parameter.keyboardWidth, 
				                                             StartPointRelative.y * Parameter.keyboardHeight, -0.2f);
				stroke.Clear();
				stroke.Add(Lexicon.StartPointRelative);
				break;
			default:
				break;
		}

	}

	public void Move(float x, float y)
	{
        //Adjust Coor
        localPoint = new Vector2(x, y);
		length += Vector2.Distance(prePoint, localPoint);
		prePoint = localPoint;
		if (Parameter.mode == Parameter.Mode.FixStart || (Lexicon.useRadialMenu && chooseCandidate))
		{
			x = x - beginPoint.x + StartPointRelative.x;
			y = y - beginPoint.y + StartPointRelative.y;
		}
		cursor.transform.localPosition = new Vector3(x * Parameter.keyboardWidth, y * Parameter.keyboardHeight, -0.2f);

        if (Vector2.Distance(new Vector2(x, y), stroke[stroke.Count - 1]) > eps)
		    stroke.Add(new Vector2(x, y));

        if (!Lexicon.useRadialMenu)
        {
            int choose = CandicateListChoose(x, y);
            lexicon.HighLightListMenu(choose);
            if (choose == 0)
                lastOutListTime = Time.time;
            else
                if (Time.time - lastOutListTime > 0.4f && !listExpanded)
                {
                    ExpandList(true);
                    server.Send("Expand", "");
                }
            SetAreaDisplay(x, y, choose == 0);
        }
        else if (Lexicon.useRadialMenu && chooseCandidate)
        {
            int choose = RadialMenuChoose(x, y);
            if (choose >= 0 && choose <= 4)
                textManager.SetCandidate(lexicon.GetChoosedCandidate(choose));
            switch (choose)
            {
                case -1:
                    radialMenu.texture = (Texture)Resources.Load("6Menu/6Menu", typeof(Texture));
                    break;
                case 0:
                    radialMenu.texture = (Texture)Resources.Load("6Menu/6Menu_Right", typeof(Texture));
                    break;
                case 1:
                    radialMenu.texture = (Texture)Resources.Load("6Menu/6Menu_TopRight", typeof(Texture));
                    break;
                case 2:
                    radialMenu.texture = (Texture)Resources.Load("6Menu/6Menu_TopLeft", typeof(Texture));
                    break;
                case 3:
                    radialMenu.texture = (Texture)Resources.Load("6Menu/6Menu_BottomRight", typeof(Texture));
                    break;
                case 4:
                    radialMenu.texture = (Texture)Resources.Load("6Menu/6Menu_BottomLeft", typeof(Texture));
                    break;
                case 5:
                    radialMenu.texture = (Texture)Resources.Load("6Menu/6Menu_Left", typeof(Texture));
                    break;
            }
        }
        else
            SetAreaDisplay(x, y);
	}

	public void End(float x, float y)
	{
        //Adjust Coor
        if (Parameter.mode == Parameter.Mode.FixStart || (Lexicon.useRadialMenu && chooseCandidate))
		{
			x = x - beginPoint.x + StartPointRelative.x;
			y = y - beginPoint.y + StartPointRelative.y;
		}
		cursor.transform.localPosition = new Vector3(x * Parameter.keyboardWidth, y * Parameter.keyboardHeight, -0.2f);

        SetAreaDisplay(x, y, false);
        if (!Lexicon.useRadialMenu && chooseCandidate)
        {
            int choose = CandicateListChoose(x, y);
            if (choose == 0 && listExpanded)
            {
                ExpandList(false);
                server.Send("Cancel", "");
                return;
            }
            string word = lexicon.Accept(ref choose);
            if (Parameter.userStudy == Parameter.UserStudy.Study2)
                server.Send("Accept", choose.ToString() + " " + word);
            ExpandList(false, true);
            if (choose > 0)
            {
                chooseCandidate = false;
                return;
            }
                
        }
        if (Vector2.Distance(new Vector2(x, y), stroke[stroke.Count - 1]) > eps)
            stroke.Add(new Vector2(x, y));
		if (Parameter.userStudy == Parameter.UserStudy.Study1 || Parameter.userStudy == Parameter.UserStudy.Study1_Train)
		{
			textManager.HighLight(+1);
			return;
		}
		if (Parameter.mode == Parameter.Mode.FixStart)
		{
			cursor.GetComponent<TrailRendererHelper>().Reset();
			cursor.transform.localPosition = new Vector3(StartPointRelative.x * Parameter.keyboardWidth, StartPointRelative.y * Parameter.keyboardHeight, -0.2f);
        }
		if (chooseCandidate)
		{
            menuGestureCount++;
			if (Lexicon.useRadialMenu)
			{
                int choose = RadialMenuChoose(x, y);
                if (choose == 5)
                    CancelRadialChoose();
                else if (choose >= 0)
                {
                    string word = lexicon.Accept(ref choose);
                    if (Parameter.userStudy == Parameter.UserStudy.Study2)
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

        if (x <= -0.52f && length - (0.5f - x) <= 1.0f)
		{
			if (Parameter.userStudy == Parameter.UserStudy.Study2)
				server.Send("Delete", "LeftSwipe");
			lexicon.Delete();
			chooseCandidate = false;
			return;
		}
		if (x >= 0.52f && length - (x - 0.5f) <= 1.0f)
        {
            chooseCandidate = false;
            if (Parameter.userStudy == Parameter.UserStudy.Basic)
            {
                if (!chooseCandidate && textManager.InputNumberCorrect())
                    lexicon.ChangePhrase();
            }
            else if (Parameter.userStudy == Parameter.UserStudy.Study2)
            {
                if (!chooseCandidate && textManager.InputNumberCorrect())
                    pcControl.FinishStudy2Phrase();
            }
            return;
        }
            
        if (Lexicon.isCut)
        {
            int l = 0, r = stroke.Count - 1;
            while (OutKeyboard(stroke[l]) && l < r) ++l;
            while (OutKeyboard(stroke[r]) && l < r) --r;
            if (l < r)
            {
                stroke.RemoveRange(r + 1, stroke.Count - r - 1);
                stroke.RemoveRange(0, l);
            }
        }
		for (int i = 0; i < stroke.Count; ++i)
            stroke[i] = new Vector2(stroke[i].x * Parameter.keyboardWidth, stroke[i].y * Parameter.keyboardHeight);
		Lexicon.Candidate[] candidates = lexicon.Recognize(stroke.ToArray());
		
		chooseCandidate = true;
        menuGestureCount = 0;
        if (Lexicon.useRadialMenu)
		{
			cursor.transform.localPosition = new Vector3(StartPointRelative.x * Parameter.keyboardWidth, StartPointRelative.y * Parameter.keyboardHeight, -0.2f);
			cursor.GetComponent<TrailRendererHelper>().Reset();
			lexicon.SetRadialMenuDisplay(true);
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

    private bool OutKeyboard(Vector2 v)
    {
        return v.x > 0.5 || v.x < -0.5 || v.y > 0.5 || v.y < -0.5;
    }

    private int RadialMenuChoose(float x, float y)
    {
        float rx = x - StartPointRelative.x;
        float ry = y - StartPointRelative.y;
        if (new Vector2(rx, ry).sqrMagnitude < RadiusMenuR * RadiusMenuR)
            return -1;
        if (rx > 0)
        {
            if (Mathf.Abs(ry) * Mathf.Sqrt(3) > Mathf.Abs(rx))
            {
                if (ry > 0)
                    return 1;
                else
                    return 3;
            }
            else
                return 0;
        }
        else
        {
            if (Mathf.Abs(ry) * Mathf.Sqrt(3) > Mathf.Abs(rx))
            {
                if (ry > 0)
                    return 2;
                else
                    return 4;
            }
            else
                return 5;
        }
    }

    private int CandicateListChoose(float x, float y)
    {
        x *= Parameter.keyboardWidth;
        y *= Parameter.keyboardHeight;
        int line = listExpanded ? 3 : 1;
        for (int i = 0; i < 4; ++i)
            if (-500 + i * 250 < x && x < -250 + i * 250)
                for (int j = 0; j < line; ++j)
                    if (y > 310 - 120 * j && y < 430 - 120 * j)
                        return j * 4 + listOrder[i];
        return 0;
    }

    private void CancelRadialChoose()
    {
        if (Parameter.userStudy == Parameter.UserStudy.Study2)
            server.Send("Cancel", "");
        chooseCandidate = false;
        lexicon.SetRadialMenuDisplay(false);
        textManager.CancelCandidate();
    }

    private void SetAreaDisplay(float x, float y, bool display = true)
    {
        Color color = deleteArea.color;
        color.a = (x <= -0.52f && length - (0.5f - x) <= 1.0f && display) ? 0.9f : 0;
        deleteArea.color = color;
        color = spaceArea.color;
        color.a = (x >= 0.52f && length - (x - 0.5f) <= 1.0f && display) ? 0.9f : 0;
        spaceArea.color = color;
    }

    private void ExpandList(bool expand, bool clear = false)
    {
        listExpanded = expand;
        if (expand)
            lexicon.SetListMenuDisplay(true, 12);
        else
            lexicon.SetListMenuDisplay(true, 4);
        if (clear)
            lexicon.CleanList();
    }
}
