﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Gesture : MonoBehaviour {

	public Image keyboard;
	public GameObject sphere;
	public Text text;
	public Lexicon lexicon;

	private float keyboardWidth;
	private float keyboardHeight;
	private List<Vector2> stroke = new List<Vector2>();

	// Use this for initialization
	void Start() 
	{
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
	}
	
	// Update is called once per frame
	void Update() 
	{
		
	}

	public void Begin(float x, float y)
	{
		sphere.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.1f);
		stroke.Clear();
		stroke.Add(new Vector2(x, y));
	}
	public void Move(float x, float y)
	{
		sphere.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.1f);
		stroke.Add(new Vector2(x, y));
	}
	public void End(float x, float y)
	{
		sphere.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.1f);
		stroke.Add(new Vector2(x, y));
		Lexicon.Candidate[] candidates = lexicon.Recognize(stroke.ToArray());
		text.text = "";
		foreach (Lexicon.Candidate candidate in candidates)
			text.text += candidate.word + "";
	}
}
