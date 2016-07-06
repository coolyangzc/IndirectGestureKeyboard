using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TouchInput : MonoBehaviour 
{
	public Image keyboard;
	public DebugInfo debugInfo;
	public Client client;

	private float keyboardWidth;
	private float keyboardHeight;
	// Use this for initialization
	void Start() 
	{
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		debugInfo.Log("Width", keyboardWidth.ToString());
		debugInfo.Log("Height", keyboardHeight.ToString());
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (Input.touchCount > 0) 
		{
			var touch = Input.GetTouch(Input.touchCount - 1);
			Vector2 local;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				keyboard.transform as RectTransform, touch.position, Camera.main, out local);
			debugInfo.Log("World", touch.position.x.ToString() + "," + touch.position.y.ToString());
			debugInfo.Log("Local", local.x.ToString() + "," + local.y.ToString());
			Vector2 relative = new Vector2(local.x / keyboardWidth, local.y / keyboardHeight);
			debugInfo.Log("Relative", relative.x.ToString() + "," + relative.y.ToString());
			client.Send("Touch", relative.x.ToString() + "," + relative.y.ToString());
		}
	}
}
