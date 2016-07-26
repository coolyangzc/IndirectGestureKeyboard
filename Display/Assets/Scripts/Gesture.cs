﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Gesture : MonoBehaviour {

	public Server server;
	public Image keyboard, radialCandidates;
	public RawImage cursor;
	public Text text;
	public Lexicon lexicon;
	
	private bool chooseCandidate = false;
	private bool ratioChanged = false;
	private float keyboardWidth, keyboardHeight;
	private float length;
	private Vector2 StartPointRelative;
	private Vector2 beginPoint, prePoint, localPoint;
	private List<Vector2> stroke = new List<Vector2>();

	// Use this for initialization
	void Start() 
	{
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		StartPointRelative = Lexicon.StartPointRelative;
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
			size.x = 600;
			size.y = 540;
			pos.y = -0.7f;
		}
		keyboard.GetComponent<RectTransform>().sizeDelta = size;
		keyboard.GetComponent<RectTransform>().localPosition = pos;
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		ratioChanged ^= true;
	}

	public void Begin(float x, float y)
	{
		cursor.GetComponent<TrailRendererHelper>().Reset();
		beginPoint = new Vector2(x, y);
		prePoint = new Vector2(x, y);
		length = 0;
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
		if (length > 0.1f)
		{
			lexicon.under = lexicon.under.Replace("_", " ");
			lexicon.underText.text = lexicon.under;
			if (chooseCandidate && !Lexicon.useRadialMenu)
			{
				chooseCandidate = false;
				lexicon.Accept(-1);
			}
		}
		prePoint = localPoint;
		if (Lexicon.mode == Lexicon.Mode.FixStart)
		{
			x = x - beginPoint.x + StartPointRelative.x;
			y = y - beginPoint.y + StartPointRelative.y;
		}
		cursor.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.2f);
		stroke.Add(new Vector2(x, y));
	}

	public void End(float x, float y)
	{
		if (Lexicon.mode == Lexicon.Mode.FixStart)
		{
			x = x - beginPoint.x + StartPointRelative.x;
			y = y - beginPoint.y + StartPointRelative.y;
		}
		cursor.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.2f);
		if (Lexicon.userStudy == Lexicon.UserStudy.Study1 || Lexicon.userStudy == Lexicon.UserStudy.Train)
		{
			lexicon.HighLight(+1);
			return;
		}
		if (chooseCandidate)
		{
			if (length < 0.1f)
			{
				if (!Lexicon.useRadialMenu )
				{
					lexicon.NextCandidate();
				} else
				{
					lexicon.Delete();
					chooseCandidate = false;
					lexicon.SetRadialMenuDisplay(false);
				}
				return;
			}
			if (Lexicon.useRadialMenu)
			{
				x -= StartPointRelative.x;
				y -= StartPointRelative.y;
				if (Mathf.Abs(x) > Mathf.Abs(y))
				{
					if (x > 0)
						lexicon.Accept(2);
					else
						lexicon.Accept(1);
				}
				else
				{
					if (y > 0)
						lexicon.Accept(0);
					else
						lexicon.Accept(3);
				}
				chooseCandidate = false;
				lexicon.SetRadialMenuDisplay(false);
				return;
			}
		}
		if (y <= -0.5f && length <= 2.0f)
		{
			lexicon.Delete();
			chooseCandidate = false;
			return;
		}

		stroke.Add(new Vector2(x, y));
		for (int i = 0; i < stroke.Count; ++i)
			stroke[i] = new Vector2(stroke[i].x * keyboardWidth, stroke[i].y * keyboardHeight);
		Lexicon.Candidate[] candidates = lexicon.Recognize(stroke.ToArray());
		if (candidates[0].confidence > 0)
		{
			chooseCandidate = true;
			if (Lexicon.useRadialMenu)
				lexicon.SetRadialMenuDisplay(true);
		}
		lexicon.SetCandidates(candidates);
		string msg = "";
		for (int i = 1; i < candidates.Length; ++i)
			if (i < candidates.Length - 1)
				msg += candidates[i].word + ",";
			else
				msg += candidates[i].word;
		server.Send("Candidates", msg);
	}
}
