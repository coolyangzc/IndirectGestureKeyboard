using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Gesture : MonoBehaviour {

	public Image keyboard;
	public GameObject sphere;

	private float keyboardWidth;
	private float keyboardHeight;

	// Use this for initialization
	void Start() 
	{
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		//MoveCursor(0.3f, 0.4f);
	}
	
	// Update is called once per frame
	void Update() 
	{
	
	}

	public void MoveCursor(float x, float y)
	{
		sphere.transform.localPosition = new Vector3(x * keyboardWidth, y * keyboardHeight, -0.1f);
		//sphere.transform.position = new Vector3(x * keyboardWidth, y * keyboardHeight, 0);
	}
}
