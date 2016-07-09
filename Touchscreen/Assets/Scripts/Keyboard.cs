using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Keyboard : MonoBehaviour 
{
	public Image keyboard;
	public DebugInfo debugInfo;
	public Client client;

	private float[] Ratio = {1f, 0.8f, 0.5f};
	private int current = 0;
	private Vector2 preLocal;
	private float keyboardWidth, keyboardHeight;

	// Use this for initialization
	void Start () 
	{
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		SetKeyboard(1.0f);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.touchCount > 0) 
		{
			Touch touch = Input.GetTouch(Input.touchCount - 1);
			Vector2 local;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				keyboard.transform as RectTransform, touch.position, Camera.main, out local);
			debugInfo.Log("World", touch.position.x.ToString() + "," + touch.position.y.ToString());
			debugInfo.Log("Local", local.x.ToString() + "," + local.y.ToString());
			Vector2 relative = new Vector2(local.x / keyboardWidth, local.y / keyboardHeight);
			debugInfo.Log("Relative", relative.x.ToString() + "," + relative.y.ToString());
			string coor = relative.x.ToString() + "," + relative.y.ToString();
			switch (touch.phase)
			{
				case TouchPhase.Began:
					client.Send("Touch.Began", coor);
					preLocal = local;
					break;
				case TouchPhase.Moved:
					if (Vector2.Distance(local, preLocal) > 0.1f)
					{
						client.Send("Touch.Moved", coor);
						preLocal = local;
					}
					break;
				case TouchPhase.Ended:
					client.Send("Touch.Ended", coor);
					break;
			}
			//client.Send("Touch", );
		}
	}

	void SetKeyboard(float ratio)
	{
		keyboard.rectTransform.localScale = new Vector3(ratio, ratio, ratio);
		debugInfo.Log("Width", (keyboardWidth * ratio).ToString());
		debugInfo.Log("Height", (keyboardHeight * ratio).ToString());
	}

	public void ZoomIn()
	{
		if (current > 0)
		{
			current--;
			SetKeyboard(Ratio[current]);
		}
		
	}

	public void ZoomOut()
	{
		if (current + 1 < Ratio.Length)
		{
			current++;
			SetKeyboard(Ratio[current]);
		}
	}
}