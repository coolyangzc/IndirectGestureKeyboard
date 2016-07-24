using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PCControl : MonoBehaviour {

	public GameObject trackingSpace;
	public Server server;
	public Canvas canvas;
	public Image keyboard;
	public Lexicon lexicon;
	public Gesture gesture;
	public Info info;
	public int phraseID = 0; 
	public InputField userID;
	//public Text userID;

	private bool mouseHidden = false;
	private bool debugOn = false;
	private bool ratioChanged = false;
	private float distance = 0;

	private float MinDistance = 4;
	private float MaxDistance = 12;
	private float ScrollKeySpeed = -1f;
	private float rotationX, rotationY;
	
	// Use this for initialization
	void Start() 
	{
		distance = canvas.transform.localPosition.z;
		info.Log("Debug", debugOn.ToString());
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			KeyControl();
			MouseControl();
		}
	}

	void KeyControl()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
				server.Send("TouchScreen Keyboard Width", "+");
			if (Input.GetKeyDown(KeyCode.LeftArrow))
				server.Send("TouchScreen Keyboard Width", "-");
			if (Input.GetKeyDown(KeyCode.D))
			{
				debugOn ^= true;
				info.Log("Debug", debugOn.ToString());
				lexicon.SetDebugDisplay(debugOn);
			}
			if (Input.GetKeyDown(KeyCode.M))
				lexicon.ChangeMode();
			if (Input.GetKeyDown(KeyCode.S))
				lexicon.ChangeShapeFormula();
			if (Input.GetKeyDown(KeyCode.L))
				lexicon.ChangeLocationFormula();
			if (Input.GetKeyDown(KeyCode.N))
				lexicon.ChangePhrase();
			if (Input.GetKeyDown(KeyCode.C))
				lexicon.ChangeCandidatesChoose(true);
			if (Input.GetKeyDown(KeyCode.R))
			{
			    server.Send("Change Ratio", "");
				server.Send("Get Keyboard Size", "");
			}
			if (Input.GetKeyDown(KeyCode.T))
			{
				gesture.ChangeRatio();
				lexicon.CalcKeyLayout();
				lexicon.CalcLexicon();
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Lexicon.userStudy = Lexicon.UserStudy.Study1;
				lexicon.ChangePhrase(phraseID);
				SendPhraseMessage();
				lexicon.HighLight(-100);
				info.Clear();
				info.Log("Phrase", (phraseID+1).ToString() + "/60");
				server.Send("Get Keyboard Size", "");
				ColorBlock cb = userID.colors;
				Color c = userID.colors.normalColor;
				c.a = 0;
				cb.normalColor = c;
				userID.colors = cb;
				c = userID.transform.FindChild("Text").GetComponent<Text>().color;
				c.a = 0;
				userID.transform.FindChild("Text").GetComponent<Text>().color = c;
			}
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (Input.GetKey(KeyCode.E))
				lexicon.ChangeEndOffset(0.1f);
			if (Input.GetKey(KeyCode.R))
				lexicon.ChangeRadius(0.1f);
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				server.Send("TouchScreen Keyboard Height", "+");
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				server.Send("TouchScreen Keyboard Size", "+");
				server.Send("Get Keyboard Size", "");
			}
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (Input.GetKey(KeyCode.E))
				lexicon.ChangeEndOffset(-0.1f);
			if (Input.GetKey(KeyCode.R))
				lexicon.ChangeRadius(-0.1f);
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				server.Send("TouchScreen Keyboard Height", "-");
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				server.Send("TouchScreen Keyboard Size", "-");
				server.Send("Get Keyboard Size", "");
			}
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (Lexicon.userStudy == Lexicon.UserStudy.Study1)
			{
				server.Send("Study1 End Phrase", Lexicon.mode.ToString());
				phraseID++;

				if (phraseID % 10 == 0)
				{
					Lexicon.userStudy = Lexicon.UserStudy.Train;
					lexicon.ChangePhrase();
					lexicon.HighLight(-100);
					info.Log("Phrase", "Warmup");
					return;
				}
				lexicon.ChangePhrase(phraseID);
				SendPhraseMessage();
				lexicon.HighLight(-100);
				info.Log("Phrase", (phraseID+1).ToString() + "/60");
			}
			if (Lexicon.userStudy == Lexicon.UserStudy.Train)
			{
				lexicon.ChangePhrase();
				lexicon.HighLight(-100);
			}
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			if (Lexicon.userStudy == Lexicon.UserStudy.Study1)
				server.Send("Study1 Backspace", "");
			if (Lexicon.userStudy == Lexicon.UserStudy.Study1 || Lexicon.userStudy == Lexicon.UserStudy.Train)
				lexicon.HighLight(-100);
		}

	}

	void MouseControl() 
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetAxis("Mouse ScrollWheel") != 0)
			{
				distance += Input.GetAxis("Mouse ScrollWheel") * ScrollKeySpeed;
				if (distance < MinDistance)
					distance = MinDistance;
				else if (distance > MaxDistance)
					distance = MaxDistance;
			}
			Vector3 pos = canvas.transform.localPosition;
			canvas.transform.localPosition = new Vector3(pos.x, pos.y, distance);
		}
		if (Input.GetKeyDown(KeyCode.Escape)) 
			mouseHidden ^= true;

		if (mouseHidden) 
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			rotationX = trackingSpace.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 5f;
			rotationY += Input.GetAxis("Mouse Y") * 5f; 
			rotationY = Mathf.Clamp(rotationY, -60f, 60f);
			trackingSpace.transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		} else 
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	void SendPhraseMessage()
	{
		server.Send("Study1 New Phrase", 
		            userID.text + "_" + phraseID.ToString() + ".txt" + "\n" + 
		            lexicon.phraseText.text);
		Debug.Log(userID.text + "_" + phraseID.ToString() + ".txt" + "\n" + 
		          lexicon.phraseText.text);
	}
}
