﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Gesture : MonoBehaviour {

	public Server server;
	public Image keyboard;
	public GameObject sphere;
	public Text text;
	public Lexicon lexicon;

	//public enum Stage

	private bool chooseCandidate = false;
	private float keyboardWidth, keyboardHeight;
	private float length;
	private Vector2 StartPoint;
	private Vector2 beginPoint, prePoint, localPoint;
	private List<Vector2> stroke = new List<Vector2>();

	// Use this for initialization
	void Start() 
	{
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		StartPoint = Lexicon.StartPoint;
	}
	
	// Update is called once per frame
	void Update() 
	{
		
	}

	public void Begin(float x, float y)
	{
		sphere.GetComponent<TrailRendererHelper>().Reset();
		beginPoint = new Vector2(x, y);
		prePoint = new Vector2(x, y);
		length = 0;
		switch (Server.mode)
		{
			case (Server.Mode.Basic):
				sphere.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.1f);
				stroke.Clear();
				stroke.Add(new Vector2(x, y));
				break;
			case (Server.Mode.Fix):
				sphere.transform.localPosition = new Vector3(StartPoint.x * keyboardWidth, StartPoint.y * keyboardHeight, -0.1f);
				stroke.Clear();
				stroke.Add(Lexicon.StartPoint);
				break;
			default:
				break;
		}

	}

	public void Move(float x, float y)
	{
		localPoint = new Vector2(x, y);
		length += Vector2.Distance(prePoint, localPoint);
		if (length > 0.1f && chooseCandidate)
		{
			chooseCandidate = false;
			lexicon.Accept(-1);
		}
		prePoint = localPoint;
		if (Server.mode == Server.Mode.Fix)
		{
			x = x - beginPoint.x + StartPoint.x;
			y = y - beginPoint.y + StartPoint.y;
		}
		sphere.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.1f);
		stroke.Add(new Vector2(x, y));
	}

	public void End(float x, float y)
	{
		if (Server.mode == Server.Mode.Fix)
		{
			x = x - beginPoint.x + StartPoint.x;
			y = y - beginPoint.y + StartPoint.y;
		}
		if (0.3 <= x && y <= 0)
		{
			lexicon.Delete();
			chooseCandidate = false;
			return;
		}

		if (length < 0.1f && chooseCandidate)
		{
			lexicon.NextCandidate();
			return;
		}
		

		sphere.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.1f);
		stroke.Add(new Vector2(x, y));

		Lexicon.Candidate[] candidates = lexicon.Recognize(stroke.ToArray());
		chooseCandidate = true;
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
