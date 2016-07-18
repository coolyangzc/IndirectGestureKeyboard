using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Keyboard : MonoBehaviour 
{
	public Image keyboard;
	public Image candidates;
	public Button button;
	public DebugInfo debugInfo;
	public Client client;

	private const int CandidateNum = 5;

	private const float eps = 1e-10f;
	private float heightRatio = 1f, widthRatio = 1f;
	private int current = 0;
	private Vector2 preLocal;
	private Button[] btn = new Button[CandidateNum];
	private float keyboardWidth, keyboardHeight;

	// Use this for initialization
	void Start () 
	{
		for (int i = 1; i < CandidateNum; ++i)
		{
			btn[i] = candidates.transform.FindChild("Candidate" + i.ToString()).GetComponent<Button>();
		}
		btn[1].onClick.AddListener(delegate(){ChooseCandidate(1);});
		btn[2].onClick.AddListener(delegate(){ChooseCandidate(2);});
		btn[3].onClick.AddListener(delegate(){ChooseCandidate(3);});
		btn[4].onClick.AddListener(delegate(){ChooseCandidate(4);});
		Button del = keyboard.transform.FindChild("Del").GetComponent<Button>();
		del.onClick.AddListener(TapDelete);
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		SetKeyboard();
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

	void SetKeyboard()
	{
		keyboard.rectTransform.localScale = new Vector3(widthRatio, heightRatio, 1f);
		debugInfo.Log("Width", (keyboardWidth * widthRatio).ToString());
		debugInfo.Log("Height", (keyboardHeight * heightRatio).ToString());
	}

	void ChooseCandidate(int id)
	{
		client.Send("Choose Candidate", id.ToString());
	}

	void TapDelete()
	{
		client.Send("Delete", "");
	}

	public void ZoomIn(bool isWidth)
	{
		if (isWidth)
		{
			widthRatio += 0.1f;
			if (widthRatio > 1)
				widthRatio = 1;
		} else
		{
			heightRatio += 0.1f;
			if (heightRatio > 1)
				heightRatio = 1;
		}

		SetKeyboard();
	}

	public void ZoomOut(bool isWidth)
	{
		if (isWidth)
		{
			widthRatio -= 0.1f;
			if (widthRatio < eps)
				widthRatio = 0.1f;
		} else
		{
			heightRatio -= 0.1f;
			if (heightRatio < eps)
				heightRatio = 0.1f;
		}
		SetKeyboard();
	}

	public void SetCandidates(string[] word)
	{
		for (int i = 0; i < word.Length; ++i)
			btn[i+1].GetComponentInChildren<Text>().text = word[i];
	}
}